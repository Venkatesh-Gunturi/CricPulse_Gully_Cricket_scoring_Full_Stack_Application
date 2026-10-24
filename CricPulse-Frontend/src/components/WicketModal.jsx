
import { useEffect, useMemo, useState } from 'react';
import '../styles/wicket-modal.css';

const WICKET_TYPES = [
  { value: 'BOWLED', label: 'Bowled', autoDismissStriker: true },
  { value: 'CAUGHT', label: 'Caught', autoDismissStriker: true },
  { value: 'STUMPED', label: 'Stumped', autoDismissStriker: true },
  { value: 'RUN OUT', label: 'Run Out', autoDismissStriker: false },
  { value: 'LBW', label: 'LBW', autoDismissStriker: true },
  { value: 'HIT WICKET', label: 'Hit Wicket', autoDismissStriker: true },
];

export default function WicketModal({
  innings,
  isSubmitting = false,
  error = '',
  onClose,
  onSubmit,
}) {
  const [wicketType, setWicketType] = useState(innings?.isFreeHit ? 'RUN OUT' : '');
  const [dismissedId, setDismissedId] = useState('');
  const [fielderId, setFielderId] = useState('');
  const [runsCompleted, setRunsCompleted] = useState('0');
  const [crossed, setCrossed] = useState(false);
  const [newBatterId, setNewBatterId] = useState('');
  const [validationError, setValidationError] = useState('');

  const isFreeHit = innings?.isFreeHit === true;
  const availableWicketTypes = isFreeHit
    ? WICKET_TYPES.filter((type) => type.value === 'RUN OUT')
    : WICKET_TYPES;

  const fieldingPlayers = innings?.fieldingPlayers ?? [];
  const eligibleNewBatters = innings?.yetToBatPlayers ?? [];

  const hasEligibleNewBatter = eligibleNewBatters.length > 0;

  const currentBatters = useMemo(
    () =>
      [
        {
          matchPlayerId: innings?.strikerMatchPlayerId,
          name: innings?.strikerName,
        },
        {
          matchPlayerId: innings?.nonStrikerMatchPlayerId,
          name: innings?.nonStrikerName,
        },
      ].filter((player) => player.matchPlayerId),
    [
      innings?.strikerMatchPlayerId,
      innings?.strikerName,
      innings?.nonStrikerMatchPlayerId,
      innings?.nonStrikerName,
    ]
  );

  const selectedType = WICKET_TYPES.find(
    (type) => type.value === wicketType
  );

  const requiresFielder = [
    'CAUGHT',
    'STUMPED',
    'RUN OUT',
  ].includes(wicketType);

  const eligibleFielders = fieldingPlayers.filter((player) => {
    if (
      wicketType === 'STUMPED' &&
      player.matchPlayerId === innings?.bowlerMatchPlayerId
    ) {
      return false;
    }

    return true;
  });

 const inningsStatus = (innings?.status ?? '').toLowerCase();

const inningsFinished =
  innings?.completionPending === true ||
  (inningsStatus !== '' && inningsStatus !== 'live');

  useEffect(() => {
    setWicketType(isFreeHit ? 'RUN OUT' : '');
  }, [isFreeHit]);

  useEffect(() => {
   setDismissedId(
  wicketType === 'RUN OUT'
    ? ''
    : innings?.strikerMatchPlayerId ?? ''
);

    setFielderId('');
    setRunsCompleted('0');
    setCrossed(false);
    setNewBatterId('');
    setValidationError('');
  }, [
    wicketType,
    selectedType?.autoDismissStriker,
    innings?.strikerMatchPlayerId,
  ]);

  function handleSubmit(event) {
    event.preventDefault();
    setValidationError('');

    if (!wicketType) {
      setValidationError('Select a wicket type.');
      return;
    }

    if (isFreeHit && wicketType !== 'RUN OUT') {
      setValidationError('Only Run Out is allowed on a Free Hit.');
      return;
    }

    if (!dismissedId) {
      setValidationError(
        'Unable to identify the dismissed batter.'
      );
      return;
    }

    if (requiresFielder && !fielderId) {
      setValidationError('Select the fielder involved.');
      return;
    }

    if (
      !inningsFinished &&
      hasEligibleNewBatter &&
      !newBatterId
    ) {
      setValidationError('Select the incoming batter.');
      return;
    }

    const wicketData = {
      wicketType,
      dismissedMatchPlayerId: dismissedId,
      newBatterMatchPlayerId: newBatterId || null,

      caughtByMatchPlayerId:
        wicketType === 'CAUGHT' ? fielderId : null,

      runOutByMatchPlayerId:
        wicketType === 'RUN OUT' ? fielderId : null,

      stumpedByMatchPlayerId:
        wicketType === 'STUMPED' ? fielderId : null,

      runsCompleted:
        wicketType === 'RUN OUT'
          ? Number(runsCompleted)
          : 0,

      battersCrossed:
        wicketType === 'RUN OUT' ? crossed : false,
    };

    onSubmit?.(wicketData);
  }

  return (
    <div
      className="wicket-modal-backdrop"
      onMouseDown={(event) => {
        if (
          event.target === event.currentTarget &&
          !isSubmitting
        ) {
          onClose?.();
        }
      }}
    >
      <section
        className="wicket-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="wicket-modal-title"
      >
        <header className="wicket-modal-header">
          <div>
            <p className="wicket-modal-eyebrow">
              LIVE SCORING
            </p>
            <h2 id="wicket-modal-title">
              {isFreeHit ? 'Record a wicket — Free Hit' : 'Record a wicket'}
            </h2>
          </div>

          <button
            type="button"
            className="wicket-modal-close"
            onClick={onClose}
            disabled={isSubmitting}
            aria-label="Close wicket modal"
          >
            ×
          </button>
        </header>

        <form onSubmit={handleSubmit}>
          {isFreeHit && (
            <p className="wicket-modal-note" role="status">
              FREE HIT: only Run Out is available on this delivery.
            </p>
          )}

          <div className="wicket-type-grid">
            {availableWicketTypes.map((type) => (
              <button
                key={type.value}
                type="button"
                className={`wicket-type-button ${
                  wicketType === type.value ? 'selected' : ''
                }`}
                onClick={() => setWicketType(type.value)}
                disabled={isSubmitting}
                aria-pressed={wicketType === type.value}
              >
                {type.label}
              </button>
            ))}
          </div>

          {wicketType && (
            <div className="wicket-modal-fields">
              {wicketType === 'RUN OUT' && (
                <label className="wicket-field">
                  <span>Who got out?</span>

                  <select
                    value={dismissedId}
                    onChange={(event) =>
                      setDismissedId(event.target.value)
                    }
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
                        {player.matchPlayerId ===
                        innings?.strikerMatchPlayerId
                          ? ' (Striker)'
                          : ' (Non-striker)'}
                      </option>
                    ))}
                  </select>
                </label>
              )}

              {wicketType === 'RUN OUT' && (
                <>
                  <label className="wicket-field">
                    <span>
                      Completed runs before the run-out
                    </span>

                    <select
                      value={runsCompleted}
                      onChange={(event) =>
                        setRunsCompleted(event.target.value)
                      }
                      disabled={isSubmitting}
                    >
                      {[0, 1, 2, 3].map((runs) => (
                        <option key={runs} value={runs}>
                          {runs} {runs === 1 ? 'run' : 'runs'}
                        </option>
                      ))}
                    </select>
                  </label>

                  <label className="wicket-checkbox-field">
                    <input
                      type="checkbox"
                      checked={crossed}
                      onChange={(event) =>
                        setCrossed(event.target.checked)
                      }
                      disabled={isSubmitting}
                    />

                    <span>
                      Have the batters crossed each other?
                    </span>
                  </label>
                </>
              )}

              {requiresFielder && (
                <label className="wicket-field">
                  <span>
                    {wicketType === 'CAUGHT'
                      ? 'Who caught the ball?'
                      : wicketType === 'STUMPED'
                        ? 'Who is the wicketkeeper?'
                        : 'Who effected the run-out?'}
                  </span>

                  <select
                    value={fielderId}
                    onChange={(event) =>
                      setFielderId(event.target.value)
                    }
                    required
                    disabled={isSubmitting}
                  >
                    <option value="">Select fielder</option>

                    {eligibleFielders.map((player) => (
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

              {!inningsFinished && hasEligibleNewBatter && (
                <label className="wicket-field">
                  <span>Incoming batter</span>

                  <select
                    value={newBatterId}
                    onChange={(event) =>
                      setNewBatterId(event.target.value)
                    }
                    required
                    disabled={isSubmitting}
                  >
                    <option value="">
                      Select incoming batter
                    </option>

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

              {!inningsFinished && !hasEligibleNewBatter && (
                <p className="wicket-modal-note">
                  No eligible batters remain. This may be the
                  final wicket of the innings.
                </p>
              )}
            </div>
          )}

          {(validationError || error) && (
            <p className="wicket-modal-error" role="alert">
              {validationError || error}
            </p>
          )}

          <footer className="wicket-modal-actions">
            <button
              type="button"
              className="wicket-cancel-button"
              onClick={onClose}
              disabled={isSubmitting}
            >
              Cancel
            </button>

            <button
              type="submit"
              className="wicket-submit-button"
              disabled={isSubmitting || !wicketType}
            >
              {isSubmitting ? 'Recording…' : 'Record Wicket'}
            </button>
          </footer>
        </form>
      </section>
    </div>
  );
}
