
import { useEffect, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getMatchById } from '../services/matchService';
import apiClient from '../services/apiClient';
import { useAuth } from '../context/AuthContext';
import '../styles/innings-setup.css';
import CancelMatchModal from '../components/CancelMatchModal';

const getValue = (object, ...keys) => {
  for (const key of keys) {
    if (object?.[key] !== undefined && object[key] !== null) {
      return object[key];
    }
  }
  return null;
};

const getPlayerId = (player) =>
  getValue(
    player,
    'matchPlayerId',
    'MatchPlayerId',
    'id',
    'Id'
  );

const getPlayerName = (player) =>
  getValue(player, 'playerName', 'PlayerName', 'displayName', 'DisplayName') ||
  [
    getValue(player, 'firstName', 'FirstName'),
    getValue(player, 'lastName', 'LastName'),
  ].filter(Boolean).join(' ') ||
  'Player';

const getErrorMessage = (error) =>
  error.response?.data?.message ||
  error.response?.data?.title ||
  (typeof error.response?.data === 'string'
    ? error.response.data
    : '') ||
  error.message ||
  'Something went wrong. Please try again.';

export default function InningsSetup() {
  const { matchId } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();

  const createdByUserId = getValue(
    user, 'userId', 'UserId', 'id', 'userID'
  );

  const [match, setMatch] = useState(null);
  const [strikerId, setStrikerId] = useState('');
  const [nonStrikerId, setNonStrikerId] = useState('');
  const [bowlerId, setBowlerId] = useState('');
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState('');

  useEffect(() => {
    let cancelled = false;

    const loadMatch = async () => {
      if (!matchId || !createdByUserId) {
        setError('Your umpire account could not be identified.');
        setLoading(false);
        return;
      }

      try {
        const result = await getMatchById(matchId, createdByUserId);
        if (cancelled) return;

        setMatch(result);
        const stateResponse = await apiClient.get(
  `/Scoring/${matchId}/state`
);

const innings = stateResponse.data?.innings ?? [];
const currentInnings = innings[innings.length - 1];

if (
  currentInnings?.status === 'Live' &&
  currentInnings?.strikerMatchPlayerId &&
  currentInnings?.nonStrikerMatchPlayerId &&
  currentInnings?.bowlerMatchPlayerId
) {
  navigate(`/umpire/matches/${matchId}/live-scoring`, {
    replace: true,
  });
  return;
}

        const tossAgreed = getValue(
          result, 'tossAgreed', 'TossAgreed'
        );
        const tossWinner = getValue(
          result, 'tossWinnerTeam', 'TossWinnerTeam'
        );
        const tossDecision = getValue(
          result, 'tossDecision', 'TossDecision'
        );

        if (
          !tossAgreed ||
          !tossWinner ||
          !tossDecision
        ) {
          setError('Please complete and agree to the toss first.');
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
    };
  }, [matchId, createdByUserId]);

  const players = getValue(match, 'players', 'Players') || [];

  const teamAName =
    getValue(match, 'teamAName', 'TeamAName') || 'Team A';

  const teamBName =
    getValue(match, 'teamBName', 'TeamBName') || 'Team B';

  const tossWinner = getValue(
    match, 'tossWinnerTeam', 'TossWinnerTeam'
  );

  const tossDecision = String(
    getValue(match, 'tossDecision', 'TossDecision') || ''
  ).toUpperCase();

  const battingTeam =
    tossDecision === 'BAT'
      ? tossWinner
      : tossWinner === 'TeamA'
        ? 'TeamB'
        : tossWinner === 'TeamB'
          ? 'TeamA'
          : '';

  const bowlingTeam =
    battingTeam === 'TeamA'
      ? 'TeamB'
      : battingTeam === 'TeamB'
        ? 'TeamA'
        : '';

  const battingTeamName =
    battingTeam === 'TeamA'
      ? teamAName
      : battingTeam === 'TeamB'
        ? teamBName
        : '';

  const bowlingTeamName =
    bowlingTeam === 'TeamA'
      ? teamAName
      : bowlingTeam === 'TeamB'
        ? teamBName
        : '';

  const battingPlayers = players.filter(
    (player) =>
      getValue(player, 'team', 'Team') === battingTeam
  );

  const bowlingPlayers = players.filter(
    (player) =>
      getValue(player, 'team', 'Team') === bowlingTeam
  );

  const samePlayer =
    strikerId && nonStrikerId && strikerId === nonStrikerId;

  const submitSetup = async () => {
    setError('');

    if (!createdByUserId) {
      setError('Your umpire account could not be identified.');
      return;
    }

    if (!strikerId || !nonStrikerId || !bowlerId) {
      setError('Select both opening batters and the opening bowler.');
      return;
    }

    if (strikerId === nonStrikerId) {
      setError('Striker and non-striker must be different players.');
      return;
    }

    setSaving(true);

    try {
      await apiClient.post(
        `/Scoring/${matchId}/innings/setup`,
        {
          createdByUserId,
          strikerMatchPlayerId: strikerId,
          nonStrikerMatchPlayerId: nonStrikerId,
          bowlerMatchPlayerId: bowlerId,
        }
      );

      // The innings is now saved. Connect this to the existing
      // live-scoring route once that route is confirmed.
      navigate(`/umpire/matches/${matchId}/live-scoring`, {
  replace: true,
});
    } catch (requestError) {
      setError(getErrorMessage(requestError));
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return (
      <main className="innings-setup-page">
        <p className="innings-loading">Loading match details...</p>
      </main>
    );
  }

  if (!match || error === 'Please complete and agree to the toss first.') {
    return (
      <main className="innings-setup-page">
        <section className="innings-setup-content">
          <p className="innings-eyebrow">CRICPULSE · MATCH SETUP</p>
          <h1>Innings setup</h1>
          <p className="innings-error" role="alert">
            {error || 'Unable to load match details.'}
          </p>
          <button
            type="button"
            className="innings-button innings-button-secondary"
            onClick={() => navigate(`/umpire/matches/${matchId}/toss`)}
          >
            Return to toss
          </button>
        </section>
      </main>
    );
  }

  const renderPlayerSelect = (
    label,
    value,
    onChange,
    teamPlayers,
    placeholder,
    excludedId = ''
  ) => (
    <label className="innings-field">
      <span>{label}</span>
      <select
        value={value}
        onChange={(event) => onChange(event.target.value)}
        required
      >
        <option value="">{placeholder}</option>
        {teamPlayers.map((player) => {
          const id = getPlayerId(player);
          if (!id || id === excludedId) return null;

          return (
            <option key={id} value={id}>
              {getPlayerName(player)}
              {getValue(player, 'isCaptain', 'IsCaptain')
                ? ' (Captain)'
                : ''}
            </option>
          );
        })}
      </select>
    </label>
  );

  return (
    <main className="innings-setup-page">
      <header className="innings-header">
        <button
          type="button"
          className="innings-back"
          onClick={() => navigate('/umpire')}
        >
          Back to Dashboard
        </button>
        <span className="innings-brand">CRICPULSE</span>
        <span className="innings-header-label">INNINGS 1</span>
      </header>

      <section className="innings-setup-content">
        <p className="innings-eyebrow">READY, UMPIRE?</p>
        <h1>Set the opening lineup.</h1>
        <p className="innings-subtitle">
          Choose the two batters and the bowler before the first ball.
        </p>

        <section className="innings-teams">
          <article className="innings-team-card innings-batting-card">
            <span className="innings-team-label">BATTING FIRST</span>
            <h2>{battingTeamName || 'Not determined'}</h2>
            <span className="innings-role-pill">Batting</span>
          </article>

          <article className="innings-team-card innings-bowling-card">
            <span className="innings-team-label">BOWLING FIRST</span>
            <h2>{bowlingTeamName || 'Not determined'}</h2>
            <span className="innings-role-pill">Bowling</span>
          </article>
        </section>

        <section className="innings-selection-card">
          <div className="innings-section-heading">
            <span className="innings-step">01</span>
            <div>
              <h2>Opening batters</h2>
              <p>Select two different players from {battingTeamName}.</p>
            </div>
          </div>

          {renderPlayerSelect(
            'Striker',
            strikerId,
            setStrikerId,
            battingPlayers,
            'Select striker',
            nonStrikerId
          )}

          {renderPlayerSelect(
            'Non-striker',
            nonStrikerId,
            setNonStrikerId,
            battingPlayers,
            'Select non-striker',
            strikerId
          )}

          {samePlayer && (
            <p className="innings-error">
              Choose two different batters.
            </p>
          )}

          <div className="innings-section-heading innings-bowler-heading">
            <span className="innings-step">02</span>
            <div>
              <h2>Opening bowler</h2>
              <p>Select one player from {bowlingTeamName}.</p>
            </div>
          </div>

          {renderPlayerSelect(
            'Bowler',
            bowlerId,
            setBowlerId,
            bowlingPlayers,
            'Select opening bowler'
          )}
        </section>

        {error && (
          <p className="innings-error" role="alert">{error}</p>
        )}

        <button
          type="button"
          className="innings-button innings-button-primary"
          onClick={submitSetup}
          disabled={
            saving ||
            !strikerId ||
            !nonStrikerId ||
            !bowlerId ||
            Boolean(samePlayer) ||
            !battingTeam ||
            !bowlingTeam
          }
        >
          {saving ? 'Setting up innings...' : 'Confirm lineup'}
        </button>
      </section>
    </main>
  );
}
