
import RunScoringControls from './RunScoringControls';

const extraGroups = [
  ['BYE', 'Bye'],
  ['LEG BYE', 'Leg Bye'],
  ['WIDE', 'Wide'],
  ['NO BALL', 'No Ball'],
];

export default function ScoringControls({
  onScoreRun,
  onScoreExtra,
  onWicket,
  onUndo,
  runsDisabled = false,
  extrasDisabled = false,
  wicketDisabled = false,
  undoDisabled = true,
  disabled = false,
}) {
  const isRunsDisabled = disabled || runsDisabled;
  const isExtrasDisabled = disabled || extrasDisabled;

  return (
    <section
      className="scoring-controls"
      aria-label="Live scoring controls"
    >
      <div className="scoring-controls-grid">
        <RunScoringControls
          onScoreRun={onScoreRun}
          disabled={isRunsDisabled}
        />

        <div className="scoring-extras">
          <h2 className="scoring-control-heading">EXTRAS</h2>

          <div className="scoring-extras-grid">
            {extraGroups.map(([type, label]) => (
              <button
                key={type}
                type="button"
                className="scoring-extra-button"
                onClick={() => onScoreExtra?.(type)}
                disabled={isExtrasDisabled}
              >
                {label}
              </button>
            ))}
          </div>
        </div>
      </div>

      <div className="scoring-bottom-actions">
        <button
          type="button"
          className="scoring-zero-button"
          onClick={() => onScoreRun?.(0)}
          disabled={isRunsDisabled}
          aria-label="Score 0 runs"
        >
          0
        </button>

        <button
          type="button"
          className="scoring-wicket-button"
          onClick={onWicket}
          disabled={disabled || wicketDisabled}
        >
          WICKET
        </button>
      </div>

      <button
        type="button"
        className="scoring-undo-button"
        onClick={onUndo}
        disabled={disabled || undoDisabled}
      >
        Undo Last Ball
      </button>
    </section>
  );
}
