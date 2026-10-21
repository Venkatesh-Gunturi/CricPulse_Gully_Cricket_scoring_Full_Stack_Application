
import { useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import ScoringControls from '../components/ScoringControls';
import { useAuth } from '../context/AuthContext';
import { getMatchState, scoreRuns } from '../services/scoringService';
import { cancelMatch } from '../services/matchService';
import '../styles/live-scoring.css';
import '../styles/match-cancellation-modal.css';
import CancelMatchModal from '../components/CancelMatchModal';

export default function LiveScoring() {
  const { matchId } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();

  const [activeTab, setActiveTab] = useState('batting');
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef(null);

  const [matchState, setMatchState] = useState(null);
  const [loading, setLoading] = useState(true);
  const [isScoring, setIsScoring] = useState(false);
  const [pageError, setPageError] = useState('');
const [cancelError, setCancelError] = useState('');
const [cancelSubmitting, setCancelSubmitting] = useState(false);
const [showCancelModal, setShowCancelModal] = useState(false);

  useEffect(() => {
    if (!menuOpen) return;

    function handleOutsideClick(event) {
      if (menuRef.current && !menuRef.current.contains(event.target)) {
        setMenuOpen(false);
      }
    }

    document.addEventListener('pointerdown', handleOutsideClick);

    return () => {
      document.removeEventListener('pointerdown', handleOutsideClick);
    };
  }, [menuOpen]);

  useEffect(() => {
    let cancelled = false;

    async function loadState() {
      setLoading(true);
      setPageError('');

      try {
        const state = await getMatchState(matchId);

        if (!cancelled) {
          setMatchState(state);
        }
      } catch (error) {
        if (!cancelled) {
          setPageError(
            error.response?.data?.message ||
            'Unable to load the match state.'
          );
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    if (matchId) {
      loadState();
    } else {
      setPageError('Match ID is missing from the URL.');
      setLoading(false);
    }

    return () => {
      cancelled = true;
    };
  }, [matchId]);

  const inningsList = matchState?.innings ?? [];

  const activeInnings =
    inningsList.find(
      (innings) => innings.status?.toLowerCase() === 'live'
    ) ??
    [...inningsList].sort(
      (a, b) => b.inningsNumber - a.inningsNumber
    )[0] ??
    null;

  const displayTeam = (team) => {
    if (team === 'TeamA') return matchState?.teamAName ?? 'Team A';
    if (team === 'TeamB') return matchState?.teamBName ?? 'Team B';
    return team || '—';
  };

  const legalBalls = activeInnings?.legalBalls ?? 0;

  const formatOvers = (balls) =>
    `${Math.floor(balls / 6)}.${balls % 6}`;

  // Only use fields currently supplied by MatchStateDto.
  // Player names and scorecards will be connected separately.
  const match = {
    teamA: matchState?.teamAName ?? 'Loading match…',
    teamB: matchState?.teamBName ?? '',
    innings: activeInnings?.inningsNumber ?? 1,
    score: activeInnings?.totalRuns ?? 0,
    wickets: activeInnings?.wickets ?? 0,
    overs: formatOvers(legalBalls),
    crr: activeInnings?.currentRunRate?.toFixed(2) ?? '0.00',
    target: activeInnings?.targetRuns ?? 0,
    rrr: activeInnings?.requiredRunRate?.toFixed(2) ?? null,
    runsRequired: activeInnings?.runsRequired ?? 0,
    ballsRemaining: activeInnings?.ballsRemaining ?? 0,
    battingTeam: displayTeam(activeInnings?.battingTeam),
    bowlingTeam: displayTeam(activeInnings?.bowlingTeam),

    striker: {
  name: activeInnings?.strikerName ?? '—',
  runs: activeInnings?.strikerRuns ?? 0,
  balls: activeInnings?.strikerBalls ?? 0,
  fours: activeInnings?.strikerFours ?? 0,
  sixes: activeInnings?.strikerSixes ?? 0,
},

nonStriker: {
  name: activeInnings?.nonStrikerName ?? '—',
  runs: activeInnings?.nonStrikerRuns ?? 0,
  balls: activeInnings?.nonStrikerBalls ?? 0,
  fours: activeInnings?.nonStrikerFours ?? 0,
  sixes: activeInnings?.nonStrikerSixes ?? 0,
},

bowler: {
  name: activeInnings?.bowlerName ?? '—',
overs: String(activeInnings?.bowlerOvers ?? 0),
  wickets: activeInnings?.bowlerWickets ?? 0,
  runs: activeInnings?.bowlerRunsConceded ?? 0,
},

currentOver: activeInnings?.currentOver ?? [],
yetToBat: activeInnings?.yetToBat ?? [],
  };

  const isSecondInnings = match.innings === 2;

  async function handleScoreRun(runs) {
    if (!activeInnings?.inningsId) {
      setPageError('No innings is available to score. Set up the innings first.');
      return;
    }

    if (!user?.userId) {
      setPageError('Your organizer ID is missing. Please log in again.');
      return;
    }

    if (isScoring || loading) return;

    setIsScoring(true);
    setPageError('');

    try {
      const updatedState = await scoreRuns(
        activeInnings.inningsId,
        user.userId,
        runs
      );

      setMatchState(updatedState);
    } catch (error) {
      setPageError(
        error.response?.data?.message ||
        error.response?.data?.title ||
        'Unable to record the run.'
      );
    } finally {
      setIsScoring(false);
    }
  }


function openCancelModal() {
  setMenuOpen(false);
  setCancelError('');
  setShowCancelModal(true);
}

function closeCancelModal() {
  if (cancelSubmitting) return;
  setShowCancelModal(false);
  setCancelError('');
}

function handleContinueCancellation() {
  if (!selectedCancellationReason) {
    setCancelError('Please select a cancellation reason.');
    return;
  }

  if (
    selectedCancellationReason === 'Others' &&
    !otherCancellationExplanation.trim()
  ) {
    setCancelError('Please explain the reason for choosing Others.');
    return;
  }

  setCancelError('');
  setCancelModalStep('confirm');
}

async function handleConfirmCancellation(cancellationData) {
  if (!matchId || !user?.userId) {
    setCancelError(
      'Unable to identify the match or umpire account. Please log in again.'
    );
    return;
  }

  setCancelSubmitting(true);
  setCancelError('');

  try {
    await cancelMatch(matchId, user.userId, cancellationData);

    setShowCancelModal(false);

    navigate('/umpire', {
      state: {
        toastMessage: 'Match cancelled successfully.',
      },
    });
  } catch (error) {
    setCancelError(
      error.response?.data?.message ||
      error.response?.data?.title ||
      'Unable to cancel the match. Please try again.'
    );
  } finally {
    setCancelSubmitting(false);
  }
}

  return (
    <main className="live-scoring-page">
      <header className="live-scoring-header">
        <button
          type="button"
          className="live-dashboard-button"
          onClick={() => navigate('/umpire')}
        >
          ← Back to Dashboard
        </button>

        <div className="live-header-center">
          <h1>{match.teamA} vs {match.teamB}</h1>
          <span className="live-header-caption">
            <span className="live-status-dot" aria-hidden="true" />
            LIVE SCORING
          </span>
        </div>

        <div className="live-menu-wrap" ref={menuRef}>
          <button
            type="button"
            className="live-menu-button"
            aria-label="Open match menu"
            aria-expanded={menuOpen}
            onClick={() => setMenuOpen((open) => !open)}
          >
            ⋮
          </button>

          {menuOpen && (
            <div className="live-menu-dropdown">
              <button
                type="button"
                onClick={() => {
                  setMenuOpen(false);
                  navigate(`/umpire/matches/${matchId}`);
                }}
              >
                Match Info
              </button>
              <button
                type="button"
                className="live-cancel-action"
                onClick={openCancelModal}
              >
                Cancel Match
              </button>
            </div>
          )}
        </div>
      </header>

      <div className="live-scoring-content">
        {loading && <p role="status">Loading match state…</p>}

        {pageError && (
          <p role="alert" className="live-scoring-error">
            {pageError}
          </p>
        )}

        <p className="live-match-label">
          {isSecondInnings ? '2ND INNINGS' : '1ST INNINGS'} · {match.battingTeam} BATTING
        </p>

        <section className="live-scoreboard">
          <div className="live-score-left">
            <span className="live-score-label">{match.battingTeam}</span>
            <div className="live-score-value">
              {match.score}<span>/{match.wickets}</span>
            </div>
            <div className="live-score-overs">Overs {match.overs}</div>
          </div>

          <div className="live-score-right">
            {isSecondInnings && (
              <div className="live-score-stat">
                <span className="live-score-stat-label">Target</span>
                <strong>{match.target} runs</strong>
              </div>
            )}

            <div className="live-score-stat">
              <span className="live-score-stat-label">CRR</span>
              <strong>{match.crr}</strong>
            </div>

            {isSecondInnings && (
              <div className="live-score-stat">
                <span className="live-score-stat-label">RRR</span>
                <strong>{match.rrr ?? '—'}</strong>
              </div>
            )}
          </div>

          {isSecondInnings && (
            <div className="live-runs-required">
              Need <strong>{match.runsRequired} runs</strong> from{' '}
              <strong>{match.ballsRemaining} balls</strong> to win
            </div>
          )}
        </section>

        <section className="live-player-grid" aria-label="Players on field">
          <PlayerCard
            role="STRIKER"
            symbol="🏏"
            player={match.striker}
            detail={`${match.striker.runs} (${match.striker.balls})`}
          />
          <PlayerCard
            role="NON-STRIKER"
            player={match.nonStriker}
            detail={`${match.nonStriker.runs} (${match.nonStriker.balls})`}
          />
          <PlayerCard
            role="BOWLER"
            symbol="🔴"
            player={match.bowler}
            detail={`${match.bowler.overs}-${match.bowler.wickets}-${match.bowler.runs}`}
          />
        </section>

        
        <section className="live-section-card">
          <h2 className="live-section-heading">
            Current Over - {match.overs}
          </h2>

          <div className="live-current-over">
            {match.currentOver.length === 0 ? (
              <span>Ball-by-ball details will appear here.</span>
            ) : (
              match.currentOver.map((ball, index) => {
                const label = String(ball).trim().toUpperCase();

                const isWicket =
                  label === 'W' || label.includes('WICKET');

                const isBoundary = label === '4' || label === '6';

                return (
                  <span
                    key={`${label}-${index}`}
                    className={`live-ball ${
                      isWicket
                        ? 'live-ball-wicket'
                        : isBoundary
                          ? 'live-ball-boundary'
                          : ''
                    }`}
                    title={`Delivery ${index + 1}: ${label}`}
                  >
                    {label}
                  </span>
                );
              })
            )}
          </div>
        </section>


        <ScoringControls
          runsDisabled={loading || isScoring || !activeInnings}
          extrasDisabled
          wicketDisabled
          undoDisabled
          onScoreRun={handleScoreRun}
          onScoreExtra={() => {}}
          onWicket={() => {}}
          onUndo={() => {}}
        />

        
        <section className="live-section-card">
          <h2 className="live-section-heading">
            Current Over — {match.overs}
          </h2>

          <div className="live-current-over">
            {match.currentOver.length === 0 ? (
              <span>—</span>
            ) : (
              match.currentOver.map((ball, index) => {
                const label = String(ball).trim().toUpperCase();

                const isWicket =
                  label === 'W' || label.includes('WICKET');

                const isBoundary = label === '4' || label === '6';

                return (
                  <span
                    key={`${label}-${index}`}
                    className={`live-ball ${
                      isWicket
                        ? 'live-ball-wicket'
                        : isBoundary
                          ? 'live-ball-boundary'
                          : ''
                    }`}
                    title={`Ball ${index + 1}: ${label}`}
                  >
                    {label}
                  </span>
                );
              })
            )}
          </div>
        </section>

      </div>
      
{showCancelModal && (
  <CancelMatchModal
    match={{
      name: `${match.teamA} vs ${match.teamB}`,
      teamAName: match.teamA,
      teamBName: match.teamB,
    }}
    isSubmitting={cancelSubmitting}
    error={cancelError}
    onClose={closeCancelModal}
    onConfirm={handleConfirmCancellation}
  />
)}

    </main>
  );
}

function PlayerCard({ role, symbol, player, detail, subdetail }) {
  return (
    <article className="live-player-card">
      <span className="live-player-role">
        {symbol && <span aria-hidden="true">{symbol}</span>} {role}
      </span>
      <h2 className="live-player-name">{player.name}</h2>
      <div className="live-player-stats">{detail}</div>
      {subdetail && (
        <div className="live-player-substats">{subdetail}</div>
      )}
    </article>
  );
}

