
import { useState } from 'react';
import '../styles/extra-scoring-modal.css';

const RUNS_FOR_EXTRAS = [0, 1, 2, 3, 4];
const RUNS_FOR_NO_BALL = [0, 1, 2, 3, 4, 5, 6];

const EXTRA_LABELS = {
  BYE: 'Bye',
  'LEG BYE': 'Leg Bye',
  WIDE: 'Wide (+1)',
  'NO BALL': 'No Ball (+1)',
};

export default function ExtraScoringModal({
  extraType,
  innings,
  isSubmitting = false,
  error = '',
  onClose,
  onSubmit,
}) {
  const isNoBall = extraType === 'NO BALL';
  const isWide = extraType === 'WIDE';
  const penaltyRuns = isNoBall || isWide ? 1 : 0;

  const [stage, setStage] = useState(
    isNoBall ? 'choice' : 'runs'
  );
  const [noBallAction, setNoBallAction] = useState('');
  const [dismissedId, setDismissedId] = useState('');
  const [fielderId, setFielderId] = useState('');
  const [runsCompleted, setRunsCompleted] = useState('0');
  const [crossed, setCrossed] = useState(false);
  const [newBatterId, setNewBatterId] = useState('');
  const [validationError, setValidationError] = useState('');

  const fieldingPlayers = innings?.fieldingPlayers ?? [];
  const eligibleNewBatters = innings?.yetToBatPlayers ?? [];

  const currentBatters = [
    {
      matchPlayerId: innings?.strikerMatchPlayerId,
      name: innings?.strikerName,
    },
    {
      matchPlayerId: innings?.nonStrikerMatchPlayerId,
      name: innings?.nonStrikerName,
    },
  ].filter((player) => player.matchPlayerId);

  const inningsStatus = (innings?.status ?? '').toLowerCase();

  const inningsFinished =
    innings?.completionPending === true ||
    (inningsStatus !== '' && inningsStatus !== 'live');

  const isRunOut = stage === 'runOut';

  const completedRunsLimit = isNoBall ? 5 : 4;

  function closeModal() {
    if (!isSubmitting) {
      onClose?.();
    }
  }

  function chooseNoBallAction(action) {
    setNoBallAction(action);
    setValidationError('');

    setStage('runs');
  }

  function selectRuns(runs) {
    setValidationError('');

    const selectedBatterRuns =
      isNoBall && noBallAction === 'HIT & RUN'
        ? runs
        : 0;

    onSubmit?.({
      extraType,
      totalRuns: runs + penaltyRuns,
      batterRuns: selectedBatterRuns,
      runsCompleted: runs,
    });
  }

  function handleRunOutSubmit(event) {
    event.preventDefault();
    setValidationError('');

    if (!dismissedId) {
      setValidationError('Select the dismissed batter.');
      return;
    }

    if (!fielderId) {
      setValidationError('Select the fielder who effected the run-out.');
      return;
    }

    if (
      !inningsFinished &&
      eligibleNewBatters.length > 0 &&
      !newBatterId
    ) {
      setValidationError('Select the incoming batter.');
      return;
    }

    const completed = Number(runsCompleted);
    onSubmit?.({
      extraType,
      totalRuns: completed + penaltyRuns,
      batterRuns:
        isNoBall && noBallAction === 'HIT & RUN'
          ? completed
          : 0,
      runsCompleted: completed,
      dismissedMatchPlayerId: dismissedId,
      runOutByMatchPlayerId: fielderId,
      newBatterMatchPlayerId: newBatterId || null,
      battersCrossed: crossed,
    });
  }

  const title = EXTRA_LABELS[extraType] ?? extraType;

  return (
    <div
      className="extra-modal-backdrop"
      onMouseDown={(event) => {
        if (
          event.target === event.currentTarget &&
          !isSubmitting
        ) {
          closeModal();
        }
      }}
    >
      <section
        className="extra-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="extra-modal-title"
      >
        <header className="extra-modal-header">
          <div>
            <p className="extra-modal-eyebrow">LIVE SCORING</p>
            <h2 id="extra-modal-title">
              {isRunOut ? `${title} — Run Out` : title}
            </h2>
          </div>

          <button
            type="button"
            className="extra-modal-close"
            onClick={closeModal}
            disabled={isSubmitting}
            aria-label="Close extras modal"
          >
            ×
          </button>
        </header>

        {stage === 'choice' && isNoBall && (
          <div className="extra-modal-choice-grid">
            <button
              type="button"
              className="extra-choice-button"
              onClick={() => chooseNoBallAction('HIT & RUN')}
              disabled={isSubmitting}
            >
              Hit &amp; Run
            </button>

            <button
              type="button"
              className="extra-choice-button"
              onClick={() => chooseNoBallAction('LEG BYE')}
              disabled={isSubmitting}
            >
              Leg Bye
            </button>

          </div>
        )}

        {stage === 'runs' && (
          <>
            {isNoBall && (
              <p className="extra-modal-description">
                {noBallAction === 'HIT & RUN'
                  ? 'Select runs scored off the bat, or choose Run Out.'
                  : 'Select completed leg-bye runs, or choose Run Out.'}
                {' '}One No Ball penalty run will be added.
              </p>
            )}

            {!isNoBall && (
              <p className="extra-modal-description">
                Select the completed runs, or choose Run Out.
                {isWide && ' One Wide penalty run will be added.'}
              </p>
            )}

            <div className="extra-modal-runs-grid">
              {(isNoBall
                ? RUNS_FOR_NO_BALL
                : RUNS_FOR_EXTRAS
              ).map((runs) => (
                <button
                  key={runs}
                  type="button"
                  className="extra-run-button"
                  onClick={() => selectRuns(runs)}
                  disabled={isSubmitting}
                >
                  {runs}
                </button>
              ))}
            </div>

            <button
              type="button"
              className="extra-choice-button extra-choice-run-out"
              onClick={() => {
                setValidationError('');
                setStage('runOut');
              }}
              disabled={isSubmitting}
            >
              Run Out
            </button>
          </>
        )}

        {isRunOut && (
          <form
            className="extra-modal-form"
            onSubmit={handleRunOutSubmit}
          >
            <label className="extra-modal-field">
              <span>Who got out?</span>
              <select
                value={dismissedId}
                onChange={(event) => setDismissedId(event.target.value)}
                required
                disabled={isSubmitting}
              >
                <option value="">Select batter</option>
                {currentBatters.map((player) => (
                  <option
                    key={player.matchPlayerId}
                    value={player.matchPlayerId}
                  >
                    {player.name}
                    {player.matchPlayerId === innings?.strikerMatchPlayerId
                      ? ' (Striker)'
                      : ' (Non-striker)'}
                  </option>
                ))}
              </select>
            </label>

            <label className="extra-modal-field">
              <span>Completed runs before the run-out</span>
              <select
                value={runsCompleted}
                onChange={(event) => setRunsCompleted(event.target.value)}
                disabled={isSubmitting}
              >
                {Array.from(
                  { length: completedRunsLimit + 1 },
                  (_, index) => index
                ).map((runs) => (
                  <option key={runs} value={runs}>
                    {runs} {runs === 1 ? 'run' : 'runs'}
                  </option>
                ))}
              </select>
            </label>

            <label className="extra-modal-field">
              <span>Who effected the run-out?</span>
              <select
                value={fielderId}
                onChange={(event) => setFielderId(event.target.value)}
                required
                disabled={isSubmitting}
              >
                <option value="">Select fielder</option>
                {fieldingPlayers.map((player) => (
                  <option
                    key={player.matchPlayerId}
                    value={player.matchPlayerId}
                  >
                    {player.name}
                  </option>
                ))}
              </select>
            </label>

            <label className="extra-modal-checkbox">
              <input
                type="checkbox"
                checked={crossed}
                onChange={(event) => setCrossed(event.target.checked)}
                disabled={isSubmitting}
              />
              <span>Have the batters crossed each other?</span>
            </label>

            {!inningsFinished && eligibleNewBatters.length > 0 && (
              <label className="extra-modal-field">
                <span>Incoming batter</span>
                <select
                  value={newBatterId}
                  onChange={(event) => setNewBatterId(event.target.value)}
                  required
                  disabled={isSubmitting}
                >
                  <option value="">Select incoming batter</option>
                  {eligibleNewBatters.map((player) => (
                    <option
                      key={player.matchPlayerId}
                      value={player.matchPlayerId}
                    >
                      {player.name}
                    </option>
                  ))}
                </select>
              </label>
            )}

            {!inningsFinished && eligibleNewBatters.length === 0 && (
              <p className="extra-modal-description">
                No eligible incoming batters are available.
              </p>
            )}

            {(validationError || error) && (
              <p className="extra-modal-error" role="alert">
                {validationError || error}
              </p>
            )}

            <footer className="extra-modal-actions">
              <button
                type="button"
                className="extra-modal-cancel"
                onClick={() => {
                  setValidationError('');
                  setStage('runs');
                }}
                disabled={isSubmitting}
              >
                Back
              </button>

              <button
                type="submit"
                className="extra-modal-submit"
                disabled={isSubmitting}
              >
                {isSubmitting ? 'Recording…' : 'Record Run Out'}
              </button>
            </footer>
          </form>
        )}

        {stage !== 'runOut' && error && (
          <p className="extra-modal-error" role="alert">
            {error}
          </p>
        )}

        {stage !== 'runOut' && (
          <footer className="extra-modal-actions">
            <button
              type="button"
              className="extra-modal-cancel"
              onClick={closeModal}
              disabled={isSubmitting}
            >
              Cancel
            </button>
          </footer>
        )}
      </section>
    </div>
  );
}
