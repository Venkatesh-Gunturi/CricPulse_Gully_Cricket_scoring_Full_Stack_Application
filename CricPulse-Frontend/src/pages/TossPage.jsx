
import { useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getMatchById } from '../services/matchService';
import apiClient from '../services/apiClient';
import { useAuth } from '../context/AuthContext';
import '../styles/toss-page.css';

const getValue = (object, ...keys) => {
  for (const key of keys) {
    if (object?.[key] !== undefined && object[key] !== null) {
      return object[key];
    }
  }
  return null;
};

const getErrorMessage = (error) =>
  error.response?.data?.message ||
  error.response?.data?.title ||
  error.message ||
  'Something went wrong. Please try again.';

function TossPage() {
  const { matchId } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();

  const umpireUserId = getValue(
    user, 'userId', 'UserId', 'id', 'userID'
  );

  const [match, setMatch] = useState(null);
  const [loading, setLoading] = useState(true);
  const [isFlipping, setIsFlipping] = useState(false);
  const [winner, setWinner] = useState('');
  const [stage, setStage] = useState('draw');
  const [decision, setDecision] = useState('');
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const flipTimer = useRef(null);

  useEffect(() => {
    let cancelled = false;

    const loadMatch = async () => {
      if (!matchId || !umpireUserId) {
        setError('Your umpire account could not be identified.');
        setLoading(false);
        return;
      }

      try {
        const result = await getMatchById(matchId, umpireUserId);
        if (cancelled) return;

        setMatch(result);

        
const tossAgreed = getValue(
  result, 'tossAgreed', 'TossAgreed'
);

const savedWinner = getValue(
  result, 'tossWinnerTeam', 'TossWinnerTeam'
);

const savedDecision = getValue(
  result, 'tossDecision', 'TossDecision'
);

const tossAlreadyCollected =
  tossAgreed === true ||
  tossAgreed === 'true' ||
  Boolean(savedWinner && savedDecision);

if (tossAlreadyCollected) {
  navigate(`/umpire/matches/${matchId}/innings-setup`, {
    replace: true,
  });
  return;
}

      } catch (requestError) {
        if (!cancelled) setError(getErrorMessage(requestError));
      } finally {
        if (!cancelled) setLoading(false);
      }
    };

    loadMatch();

    return () => {
      cancelled = true;
      if (flipTimer.current) clearTimeout(flipTimer.current);
    };
 }, [matchId, umpireUserId, navigate]);

  const teamAName =
    getValue(match, 'teamAName', 'TeamAName') || 'Team A';

  const teamBName =
    getValue(match, 'teamBName', 'TeamBName') || 'Team B';

  const winnerName =
    winner === 'TeamA'
      ? teamAName
      : winner === 'TeamB'
        ? teamBName
        : '';

  const drawToss = () => {
    if (isFlipping || saving || stage === 'saved') return;

    setError('');
    setMessage('');
    setWinner('');
    setDecision('');
    setStage('draw');
    setIsFlipping(true);

    // Decide the result at the beginning, but reveal it only
    // after the complete three-second animation.
    
    const randomByte = crypto.getRandomValues(new Uint8Array(1))[0];
    const result = randomByte < 128 ? 'TeamA' : 'TeamB';

    setWinner(result);
    setDecision('');
    setStage('draw');
    setIsFlipping(true);

    flipTimer.current = setTimeout(() => {
      setIsFlipping(false);
      setStage('result');
      flipTimer.current = null;
    }, 3000);

  };

  const agreeToToss = () => {
    if (!winner || isFlipping || saving) return;

    setStage('decision');
    setMessage('');
    setError('');
  };

  const saveTossDecision = async (selectedDecision) => {
    if (!winner || !umpireUserId || saving) return;

    setSaving(true);
    setError('');
    setMessage('');

    try {
      // TossDto uses CreatedByUserId. The winner sent to the
      // backend must be TeamA or TeamB, not the display name.
      const payload = {
        createdByUserId: umpireUserId,
        tossWinnerTeam: winner,
        tossDecision: selectedDecision,
        agree: true,
      };

      await apiClient.post(
        `/Scoring/${matchId}/toss`,
        payload
      );

      setDecision(selectedDecision);
      setStage('saved');
      setMessage(
        `${winnerName} won the toss and chose to ${
          selectedDecision === 'BAT' ? 'bat' : 'bowl'
        }.`
      );
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <main className="toss-page">
        <p className="toss-loading">Loading match...</p>
      </main>
    );
  }

  if (!match) {
    return (
      <main className="toss-page">
        <div className="toss-content">
          <p className="toss-error" role="alert">
            {error || 'Unable to load this match.'}
          </p>
          <button
            type="button"
            className="toss-button toss-button-secondary"
            onClick={() => navigate('/umpire')}
          >
            Back to dashboard
          </button>
        </div>
      </main>
    );
  }

  return (
    <main className="toss-page">
      <header className="toss-header">
        <button
          type="button"
          className="toss-back-button"
          onClick={() => navigate('/umpire')}
        >
          ← Dashboard
        </button>
        <span className="toss-brand">CRICPULSE</span>
        <span className="toss-header-label">MATCH TOSS</span>
      </header>

      <section className="toss-content">
        <p className="toss-eyebrow">THE MATCH BEGINS</p>
        <h1>It's time for the toss.</h1>
        <p className="toss-subtitle">
          One flip. One winner. Let the game begin.
        </p>

        <div className="toss-team-strip">
          <div className="toss-team">
            <span className="toss-team-label">TEAM A</span>
            <strong>{teamAName}</strong>
          </div>

          <span className="toss-versus">VS</span>

          <div className="toss-team">
            <span className="toss-team-label">TEAM B</span>
            <strong>{teamBName}</strong>
          </div>
        </div>

        <div className="toss-stage">
          <div
            className={[
              'toss-coin',
              isFlipping ? 'is-flipping' : '',
              winner === 'TeamB' ? 'show-team-b' : '',
            ].filter(Boolean).join(' ')}
            aria-label={
              isFlipping
                ? 'Coin flipping'
                : winner
                  ? `Toss result: ${winnerName}`
                  : 'Toss coin'
            }
          >
            <div className="toss-coin-inner">
              <div className="toss-coin-face toss-coin-front">
                <span className="toss-coin-caption">CRICPULSE</span>
                <strong>{teamAName}</strong>
                <span className="toss-coin-star">★</span>
              </div>

              <div className="toss-coin-face toss-coin-back">
                <span className="toss-coin-caption">CRICPULSE</span>
                <strong>{teamBName}</strong>
                <span className="toss-coin-star">★</span>
              </div>
            </div>
          </div>
        </div>

        <div className="toss-result" aria-live="polite">
          {isFlipping && (
            <>
              <h2>The coin is in the air...</h2>
              <p>Let's see who wins the toss.</p>
            </>
          )}

          {!isFlipping && stage === 'draw' && (
            <>
              <h2>Ready for the toss?</h2>
              <p>Press Draw to flip the coin.</p>
            </>
          )}

          {!isFlipping && stage === 'result' && (
            <>
              <span className="toss-result-label">TOSS WINNER</span>
              <h2>{winnerName}</h2>
              <p>Does the result look right?</p>
            </>
          )}

          {stage === 'decision' && (
            <>
              <span className="toss-result-label">TOSS CONFIRMED</span>
              <h2>{winnerName} won the toss!</h2>
              <p>What has the winning team decided?</p>
            </>
          )}

          {stage === 'saved' && (
            <button
              type="button"
              className="toss-button toss-button-primary"
              onClick={() =>
                navigate(`/umpire/matches/${matchId}/innings-setup`)
              }
            >
              Set Up Innings →
            </button>
          )}
        </div>

        {error && (
          <p className="toss-error" role="alert">{error}</p>
        )}

        {message && stage === 'saved' && (
          <p className="toss-success" role="status">{message}</p>
        )}

        <div className="toss-actions">
          {stage === 'draw' && (
            <button
              type="button"
              className="toss-button toss-button-primary"
              onClick={drawToss}
              disabled={isFlipping}
            >
              {isFlipping ? 'Flipping...' : 'Draw Toss'}
            </button>
          )}

          {stage === 'result' && (
            <>
              <button
                type="button"
                className="toss-button toss-button-primary"
                onClick={agreeToToss}
              >
                Agree
              </button>

              <button
                type="button"
                className="toss-button toss-button-secondary"
                onClick={drawToss}
                disabled={isFlipping}
              >
                Redraw
              </button>
            </>
          )}

          {stage === 'decision' && (
            <>
              <button
                type="button"
                className="toss-button toss-button-primary"
                onClick={() => saveTossDecision('BAT')}
                disabled={saving}
              >
                {saving ? 'Saving...' : 'Choose Bat'}
              </button>

              <button
                type="button"
                className="toss-button toss-button-secondary"
                onClick={() => saveTossDecision('BOWL')}
                disabled={saving}
              >
                {saving ? 'Saving...' : 'Choose Bowl'}
              </button>
            </>
          )}

          {stage === 'saved' && (
            <p className="toss-next-step">
              Toss complete. Next: innings setup.
            </p>
          )}
        </div>
      </section>
    </main>
  );
}

export default TossPage;
