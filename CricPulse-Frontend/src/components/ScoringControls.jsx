
const runGroups = [
  [1, 2, 3],
  [4, 5, 6],
];

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
  undoDisabled = true,
  disabled = false,
}) {
  return (
    <section className="scoring-controls" aria-label="Live scoring controls">
      <div className="scoring-controls-grid">
        <div className="scoring-runs">
          <h2 className="scoring-control-heading">RUNS</h2>

          {runGroups.map((group, index) => (
            <div className="scoring-run-row" key={index}>
              {group.map((run) => (
                <button
                  key={run}
                  type="button"
                  className="scoring-run-button"
                  onClick={() => onScoreRun?.(run)}
                  disabled={disabled}
                  aria-label={`Score ${run} run${run === 1 ? '' : 's'}`}
                >
                  {run}
                </button>
              ))}
            </div>
          ))}
        </div>

        <div className="scoring-extras">
          <h2 className="scoring-control-heading">EXTRAS</h2>

          <div className="scoring-extras-grid">
            {extraGroups.map(([type, label]) => (
              <button
                key={type}
                type="button"
                className="scoring-extra-button"
                onClick={() => onScoreExtra?.(type)}
                disabled={disabled}
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
    disabled={disabled}
  >
    0
  </button>

  <button
    type="button"
    className="scoring-wicket-button"
    onClick={onWicket}
    disabled={disabled}
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
