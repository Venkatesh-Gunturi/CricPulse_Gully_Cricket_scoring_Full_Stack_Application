
import '../styles/run-scoring-controls.css';

const runGroups = [
  [1, 2, 3],
  [4, 5, 6],
];

export default function RunScoringControls({
  onScoreRun,
  disabled = false,
}) {
  return (
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
  );
}
