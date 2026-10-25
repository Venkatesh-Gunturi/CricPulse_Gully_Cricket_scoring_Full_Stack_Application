
import { useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import ScoringControls from '../components/ScoringControls';
import { useAuth } from '../context/AuthContext';
import {
  getMatchState,
  scoreRuns,
  scoreWicket,
  scoreExtra,
  undoLastBall,
  changeBowler,
  completeInnings,
} from '../services/scoringService';
import ExtraScoringModal from '../components/ExtraScoringModal';
import WicketModal from '../components/WicketModal';
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
  const [selectedExtraType, setSelectedExtraType] = useState(null);
const [extraError, setExtraError] = useState('');
const [showWicketModal, setShowWicketModal] = useState(false);
const [wicketError, setWicketError] = useState('');
const [showBowlerModal, setShowBowlerModal] = useState(false);
const [selectedBowlerId, setSelectedBowlerId] = useState('');
const [bowlerError, setBowlerError] = useState('');
const lastPromptedOverRef = useRef(null);

  const [matchState, setMatchState] = useState(null);
  const [loading, setLoading] = useState(true);
  const [isScoring, setIsScoring] = useState(false);
  const [pageError, setPageError] = useState('');
const [cancelError, setCancelError] = useState('');
const [cancelSubmitting, setCancelSubmitting] = useState(false);
const [showCancelModal, setShowCancelModal] = useState(false);
const [completionSubmitting, setCompletionSubmitting] = useState(false);
const [completionError, setCompletionError] = useState('');

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

  const firstInnings =
    inningsList.find((innings) => innings.inningsNumber === 1) ?? null;
  const firstInningsTarget = (firstInnings?.totalRuns ?? 0) + 1;
  const inningsOversLimit =
    firstInnings?.totalOvers ?? matchState?.overs ?? 0;

  useEffect(() => {
    const isLive = activeInnings?.status?.toLowerCase() === 'live';
    const overIsComplete = legalBalls > 0 && legalBalls % 6 === 0;

    if (!isLive || activeInnings?.completionPending) {
      lastPromptedOverRef.current = null;
      setShowBowlerModal(false);
      return;
    }

    // Keep the completed-over key while the first ball of the next over is
    // being scored. Undoing that ball returns to the same boundary and must
    // not ask for another bowler. handleUndo clears the key only when undoing
    // the sixth legal ball back to five.
    if (!overIsComplete) {
      setShowBowlerModal(false);
      return;
    }

    const overKey = `${activeInnings.inningsId}-${legalBalls}`;
    if (lastPromptedOverRef.current !== overKey) {
      lastPromptedOverRef.current = overKey;
      setSelectedBowlerId('');
      setBowlerError('');
      setShowBowlerModal(true);
    }
  }, [
    activeInnings?.inningsId,
    activeInnings?.status,
    activeInnings?.completionPending,
    legalBalls,
  ]);

  const availableBowlers = (activeInnings?.fieldingPlayers ?? []).filter(
    (player) =>
      player.matchPlayerId !== activeInnings?.bowlerMatchPlayerId &&
      player.matchPlayerId !== activeInnings?.strikerMatchPlayerId &&
      player.matchPlayerId !== activeInnings?.nonStrikerMatchPlayerId
  );

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
  if (!activeInnings?.inningsId || activeInnings.status?.toLowerCase() !== 'live') {
    setPageError('No live innings is available.');
    return;
  }

  if (!user?.userId) {
    setPageError('Your organizer ID is missing. Please log in again.');
    return;
  }

  if (isScoring || loading || showBowlerModal) return;

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
      'Unable to record the runs.'
    );
  } finally {
    setIsScoring(false);
  }
}


function handleScoreExtra(extraType) {
  if (
    !activeInnings?.inningsId ||
    activeInnings.status?.toLowerCase() !== 'live'
  ) {
    setPageError('No live innings is available.');
    return;
  }

  if (!user?.userId) {
    setPageError('Your organizer ID is missing. Please log in again.');
    return;
  }

  if (isScoring || loading || showBowlerModal) return;

  setPageError('');
  setExtraError('');
  setSelectedExtraType(extraType);
}

async function handleSubmitExtra(extraData) {
  if (!activeInnings?.inningsId || !user?.userId) {
    setExtraError('Unable to identify the live innings or organizer.');
    return;
  }

  if (isScoring || loading || showBowlerModal) return;

  setIsScoring(true);
  setExtraError('');
  setPageError('');

  try {
    const updatedState = await scoreExtra(
      activeInnings.inningsId,
      {
        createdByUserId: user.userId,
        ...extraData,
      }
    );

    setMatchState(updatedState);
    setSelectedExtraType(null);
  } catch (error) {
    setExtraError(
      error.response?.data?.message ||
      error.response?.data?.title ||
      'Unable to record the extra.'
    );
  } finally {
    setIsScoring(false);
  }
}

async function handleScoreWicket(wicketData) {
  if (!activeInnings?.inningsId) {
    setWicketError('No live innings is available.');
    return;
  }

  if (!user?.userId) {
    setWicketError('Your organizer ID is missing. Please log in again.');
    return;
  }

  if (isScoring || loading || showBowlerModal) return;

  setIsScoring(true);
  setWicketError('');
  setPageError('');

  try {
    const updatedState = await scoreWicket(
      activeInnings.inningsId,
      {
        createdByUserId: user.userId,
        ...wicketData,
      }
    );

    setMatchState(updatedState);
    setShowWicketModal(false);
  } catch (error) {
    setWicketError(
      error.response?.data?.message ||
      error.response?.data?.title ||
      'Unable to record the wicket.'
    );
  } finally {
    setIsScoring(false);
  }
}

async function handleUndo() {
  if (!activeInnings?.inningsId || !user?.userId) {
    setPageError(
      'Unable to identify the live innings or umpire account.'
    );
    return;
  }

  if (isScoring || loading) return;

  setIsScoring(true);
  setPageError('');

  try {
    const updatedState = await undoLastBall(
      activeInnings.inningsId,
      user.userId
    );

    setMatchState(updatedState);

    // Undoing the first ball of the new over (e.g. 7 -> 6 legal balls)
    // should preserve the bowler already selected for that over. But if the
    // sixth ball itself is undone (e.g. 6 -> 5), allow the prompt to reappear
    // after that sixth ball is entered again.
    const updatedInnings = (updatedState.innings ?? []).find(
      (innings) => innings.inningsId === activeInnings.inningsId
    );
    if ((updatedInnings?.legalBalls ?? 0) % 6 !== 0) {
      lastPromptedOverRef.current = null;
    }
    setShowBowlerModal(false);
    setPageError(
      'Last ball undone successfully. You can only undo one ball at a time, to undo you have to score new ball.'
    );
  } catch (error) {
    setPageError(
      error.response?.data?.message ||
      error.response?.data?.title ||
      'Unable to undo the last ball.'
    );
  } finally {
    setIsScoring(false);
  }
}

async function handleConfirmFirstInnings() {
  if (!activeInnings?.inningsId || !user?.userId) {
    setCompletionError('Unable to identify the innings or umpire account.');
    return;
  }

  if (isScoring || loading || completionSubmitting) return;

  setCompletionSubmitting(true);
  setCompletionError('');
  setPageError('');

  try {
    const updatedState = await completeInnings(
      activeInnings.inningsId,
      user.userId
    );
    setMatchState(updatedState);
    navigate(`/umpire/matches/${matchId}/innings-setup`, {
      state: { inningsNumber: 2 },
    });
  } catch (error) {
    setCompletionError(
      error.response?.data?.message ||
      error.response?.data?.title ||
      'Unable to complete the first innings. Please try again.'
    );
  } finally {
    setCompletionSubmitting(false);
  }
}


async function handleConfirmSecondInnings() {
  if (!activeInnings?.inningsId || !user?.userId) {
    setCompletionError('Unable to identify the innings or umpire account.');
    return;
  }

  if (isScoring || loading || completionSubmitting) return;

  setCompletionSubmitting(true);
  setCompletionError('');
  setPageError('');

  try {
    const updatedState = await completeInnings(
      activeInnings.inningsId,
      user.userId
    );

    setMatchState(updatedState);

    navigate(`/umpire/matches/${matchId}/result`, {
      replace: true,
    });
  } catch (error) {
    setCompletionError(
      error.response?.data?.message ||
      error.response?.data?.title ||
      'Unable to complete the match. Please try again.'
    );
  } finally {
    setCompletionSubmitting(false);
  }
}

async function handleChangeBowler() {
  if (!activeInnings?.inningsId || !user?.userId) {
    setBowlerError('Unable to identify the live innings or organizer.');
    return;
  }

  if (!selectedBowlerId) {
    setBowlerError('Please select the next bowler.');
    return;
  }

  if (isScoring || loading) return;

  setIsScoring(true);
  setBowlerError('');
  setPageError('');

  try {
    const updatedState = await changeBowler(
      activeInnings.inningsId,
      user.userId,
      selectedBowlerId
    );

    setMatchState(updatedState);
    setShowBowlerModal(false);
    setSelectedBowlerId('');
  } catch (error) {
    setBowlerError(
      error.response?.data?.message ||
      error.response?.data?.title ||
      'Unable to select the next bowler.'
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

                const ballClass = getBallClassName(label);

                return (
                  <span
                    key={`${label}-${index}`}
                    className={`live-ball ${ballClass}`}
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
          onScoreRun={handleScoreRun}
          onScoreExtra={handleScoreExtra}
          onWicket={() => {
            setWicketError('');
            setShowWicketModal(true);
          }}
          runsDisabled={
            loading ||
            isScoring ||
            showBowlerModal ||
            activeInnings?.status?.toLowerCase() !== 'live'
          }
          extrasDisabled={
            loading ||
            isScoring ||
            showBowlerModal ||
            activeInnings?.status?.toLowerCase() !== 'live'
          }
          wicketDisabled={
            loading ||
            isScoring ||
            showBowlerModal ||
            activeInnings?.status?.toLowerCase() !== 'live'
          }
          undoDisabled={
            loading ||
            isScoring ||
            showBowlerModal ||
            activeInnings?.status?.toLowerCase() !== 'live' ||
            !match.currentOver.length ||
            activeInnings?.undoUsed === true
          }
          onUndo={handleUndo}
        />
        
        

        <section className="live-section-card">
          <h2 className="live-section-heading">Yet to Bat</h2>

          <div className="live-yet-to-bat">
            {match.yetToBat.length === 0 ? (
              <span>No players remaining to bat.</span>
            ) : (
              match.yetToBat.map((player, index) => (
                <div
                  key={`${player}-${index}`}
                  className="live-yet-to-bat-player"
                >
                  {player}
                </div>
              ))
            )}
          </div>
        </section>



      </div>
      
{showBowlerModal && activeInnings && (
  <div className="live-bowler-modal-overlay" role="presentation">
    <section
      className="live-bowler-modal"
      role="dialog"
      aria-modal="true"
      aria-labelledby="next-bowler-title"
    >
      <h2 id="next-bowler-title">Over Complete</h2>
      <p>Select the next bowler to continue scoring.</p>

      <label htmlFor="next-bowler-select">Next bowler</label>
      <select
        id="next-bowler-select"
        value={selectedBowlerId}
        onChange={(event) => {
          setSelectedBowlerId(event.target.value);
          setBowlerError('');
        }}
        disabled={isScoring}
      >
        <option value="">Choose a bowler</option>
        {availableBowlers.map((player) => (
          <option key={player.matchPlayerId} value={player.matchPlayerId}>
            {player.name}
          </option>
        ))}
      </select>

      {availableBowlers.length === 0 && (
        <p role="alert">No eligible bowler is available. Check the fielding team and current batters.</p>
      )}
      {bowlerError && <p role="alert" className="live-scoring-error">{bowlerError}</p>}

      <button
        type="button"
        onClick={handleChangeBowler}
        disabled={isScoring || !selectedBowlerId || availableBowlers.length === 0}
      >
        {isScoring ? 'Saving…' : 'Confirm Bowler'}
      </button>
    </section>
  </div>
)}

{activeInnings?.completionPending &&
  activeInnings?.inningsNumber === 1 &&
  firstInnings && (
    <div className="first-innings-completion-overlay" role="presentation">
      <section
        className="first-innings-completion-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="first-innings-completion-title"
      >
        <h2 id="first-innings-completion-title">First Innings Complete</h2>
        <p>
          {displayTeam(firstInnings.battingTeam)} scored{' '}
          <strong>{firstInnings.totalRuns}/{firstInnings.wickets}</strong> in{' '}
          <strong>{formatOvers(firstInnings.legalBalls)} overs</strong>.
        </p>
        <p>
          {displayTeam(firstInnings.bowlingTeam)} need{' '}
          <strong>{firstInningsTarget} runs</strong> in{' '}
          <strong>{inningsOversLimit} overs</strong> to win.
        </p>

        {completionError && (
          <p role="alert" className="live-scoring-error">
            {completionError}
          </p>
        )}

        <p className="first-innings-completion-warning">
          Review the score before moving to the second innings.
        </p>

        <div className="first-innings-completion-actions">
          <button
            type="button"
            onClick={handleUndo}
            disabled={isScoring || loading || completionSubmitting}
          >
            Undo Last Ball
          </button>
          <button
            type="button"
            onClick={handleConfirmFirstInnings}
            disabled={isScoring || loading || completionSubmitting}
          >
            {completionSubmitting
              ? 'Confirming…'
              : 'Confirm and Set Up Second Innings'}
          </button>
        </div>
      </section>
    </div>
  )}

  
{activeInnings?.completionPending &&
  activeInnings?.inningsNumber === 2 &&
  firstInnings && (
    <div className="first-innings-completion-overlay" role="presentation">
      <section
        className="first-innings-completion-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="second-innings-completion-title"
      >
        <h2 id="second-innings-completion-title">
          Confirm Match Result
        </h2>

        <p>
          {displayTeam(firstInnings.battingTeam)} scored{' '}
          <strong>
            {firstInnings.totalRuns}/{firstInnings.wickets}
          </strong>{' '}
          in <strong>{formatOvers(firstInnings.legalBalls)} overs</strong>.
        </p>

        <p>
          {displayTeam(activeInnings.battingTeam)} scored{' '}
          <strong>
            {activeInnings.totalRuns}/{activeInnings.wickets}
          </strong>{' '}
          in <strong>{formatOvers(activeInnings.legalBalls)} overs</strong>.
        </p>

        <p className="first-innings-completion-warning">
          {activeInnings.totalRuns > firstInnings.totalRuns
            ? `${displayTeam(activeInnings.battingTeam)} have chased the target.`
            : activeInnings.totalRuns < firstInnings.totalRuns
              ? `${displayTeam(firstInnings.battingTeam)} are ahead by ${
                  firstInnings.totalRuns - activeInnings.totalRuns
                } runs.`
              : 'The scores are level. The match will be tied.'}
        </p>

        {completionError && (
          <p role="alert" className="live-scoring-error">
            {completionError}
          </p>
        )}

        <p className="first-innings-completion-warning">
          Review both scores before confirming. The match will only be
          finalized after you confirm the result.
        </p>

        <div className="first-innings-completion-actions">
          <button
            type="button"
            onClick={handleUndo}
            disabled={isScoring || loading || completionSubmitting}
          >
            Undo Last Ball
          </button>

          <button
            type="button"
            onClick={handleConfirmSecondInnings}
            disabled={isScoring || loading || completionSubmitting}
          >
            {completionSubmitting ? 'Confirming…' : 'Confirm Result'}
          </button>
        </div>
      </section>
    </div>
  )}


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

{showWicketModal && activeInnings && (
  <WicketModal
    innings={activeInnings}
    isSubmitting={isScoring}
    error={wicketError}
    onClose={() => {
      if (isScoring) return;

      setShowWicketModal(false);
      setWicketError('');
    }}
    onSubmit={handleScoreWicket}
  />
)}

{selectedExtraType && activeInnings && (
  <ExtraScoringModal
    extraType={selectedExtraType}
    innings={activeInnings}
    isSubmitting={isScoring}
    error={extraError}
    onClose={() => {
      if (isScoring) return;

      setSelectedExtraType(null);
      setExtraError('');
    }}
    onSubmit={handleSubmitExtra}
  />
)}
    </main>
  );
}

function getBallClassName(label) {
  if (label === 'W' || /(^|\s)W($|\s)|WICKET/.test(label)) {
    return 'live-ball-wicket';
  }
  if (label === '4') return 'live-ball-four live-ball-boundary';
  if (label === '6') return 'live-ball-six live-ball-boundary';
  if (/^(WD|NB|B\+|LB\+)/.test(label)) return 'live-ball-extra';
  if (label === '0' || label === '•') return 'live-ball-dot';
  return '';
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

