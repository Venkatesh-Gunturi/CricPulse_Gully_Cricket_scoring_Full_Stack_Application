import { useEffect, useState } from "react";

const NoBallScoringModal = ({
  onClose,
  onConfirm,
  loading
}) => {
  const [scoringType, setScoringType] = useState(null);
  const [selectedRuns, setSelectedRuns] = useState(null);

  useEffect(() => {
    setScoringType(null);
    setSelectedRuns(null);
  }, []);

  const handleScoringType = (type) => {
    setScoringType(type);
    setSelectedRuns(null);
  };

  const handleConfirm = () => {
    if (!scoringType || selectedRuns === null) {
      return;
    }

    let totalRuns = 0;
    let batterRuns = 0;

    if (scoringType === "HIT_AND_RAN") {
      batterRuns = selectedRuns;
      totalRuns = 1 + batterRuns;
    }

    if (scoringType === "BYE_RUNS") {
      batterRuns = 0;
      totalRuns = 1 + selectedRuns;
    }

    onConfirm(
      "NO BALL",
      totalRuns,
      batterRuns
    );
  };

  const getResultText = () => {
    if (scoringType === "HIT_AND_RAN") {
      return (
        <>
          Total:{" "}
          <strong>1</strong>{" "}
          extra +{" "}
          <strong>
            {selectedRuns}
          </strong>{" "}
          batter runs ={" "}
          <strong>
            {1 + selectedRuns}
          </strong>{" "}
          runs
        </>
      );
    }

    if (scoringType === "BYE_RUNS") {
      return (
        <>
          Total:{" "}
          <strong>1</strong>{" "}
          extra +{" "}
          <strong>
            {selectedRuns}
          </strong>{" "}
          bye runs ={" "}
          <strong>
            {1 + selectedRuns}
          </strong>{" "}
          runs
        </>
      );
    }

    return null;
  };

  const runOptions =
  scoringType === "HIT_AND_RAN"
    ? [0, 1, 2, 3, 4, 5, 6]
    : [0, 1, 2, 3, 4, 5, 6];

  return (
    <>
      <div
        className="modal fade show d-block"
        tabIndex="-1"
        role="dialog"
        style={{
          backgroundColor: "rgba(0, 0, 0, 0.65)"
        }}
      >
        <div
          className="modal-dialog modal-dialog-centered"
          role="document"
        >
          <div className="modal-content">

            {/* Header */}
            <div className="modal-header">
              <h5 className="modal-title">
                NO BALL
              </h5>

              <button
                type="button"
                className="btn-close"
                onClick={onClose}
                disabled={loading}
              />
            </div>

            {/* Body */}
            <div className="modal-body">

              {!scoringType && (
                <>
                  <div className="text-center mb-3">
                    <h6 className="text-muted mb-1">
                      How did the runs occur?
                    </h6>
                  </div>

                  <div className="d-flex gap-2">

                    {/* Run Out */}
                    <button
                      type="button"
                      className="btn btn-danger flex-fill"
                      disabled={loading}
                      onClick={() => {
                        // Run Out functionality will be implemented later.
                      }}
                    >
                      RUN OUT
                    </button>

                    {/* Hit & Ran */}
                    <button
                      type="button"
                      className="btn btn-outline-primary flex-fill"
                      onClick={() =>
                        handleScoringType("HIT_AND_RAN")
                      }
                      disabled={loading}
                    >
                      HIT & RAN
                    </button>

                    {/* Bye Runs */}
                    <button
                      type="button"
                      className="btn btn-outline-primary flex-fill"
                      onClick={() =>
                        handleScoringType("BYE_RUNS")
                      }
                      disabled={loading}
                    >
                      BYE RUNS
                    </button>

                  </div>
                </>
              )}

              {scoringType && (
                <>
                  <div className="text-center mb-3">
                    <h6 className="text-muted mb-1">
                      {scoringType === "HIT_AND_RAN"
                        ? "Batter runs"
                        : "Bye runs"}
                    </h6>

                    <p className="small text-muted mb-0">
                      Select the runs scored.
                    </p>
                  </div>

                  <div className="d-flex flex-wrap justify-content-center gap-2">

                    {runOptions.map((runs) => (
                      <button
                        key={runs}
                        type="button"
                        className={`btn ${
                          selectedRuns === runs
                            ? "btn-primary"
                            : "btn-outline-primary"
                        }`}
                        style={{
                          minWidth: "60px"
                        }}
                        onClick={() =>
                          setSelectedRuns(runs)
                        }
                        disabled={loading}
                      >
                        {runs}
                      </button>
                    ))}

                  </div>

                  {selectedRuns !== null && (
                    <div className="alert alert-info text-center mt-4 mb-0">
                      <div className="fs-5">
                        {getResultText()}
                      </div>
                    </div>
                  )}

                  <div className="text-center mt-3">
                    <button
                      type="button"
                      className="btn btn-link btn-sm"
                      onClick={() => {
                        setScoringType(null);
                        setSelectedRuns(null);
                      }}
                      disabled={loading}
                    >
                      ← Back
                    </button>
                  </div>
                </>
              )}

            </div>

            {/* Footer */}
            <div className="modal-footer">

              <button
                type="button"
                className="btn btn-secondary"
                onClick={onClose}
                disabled={loading}
              >
                Cancel
              </button>

              <button
                type="button"
                className="btn btn-primary"
                onClick={handleConfirm}
                disabled={
                  loading ||
                  !scoringType ||
                  selectedRuns === null
                }
              >
                {loading
                  ? "Scoring..."
                  : `Add ${
                      selectedRuns === null
                        ? ""
                        : 1 + selectedRuns
                    } Runs`}
              </button>

            </div>

          </div>
        </div>
      </div>
    </>
  );
};

export default NoBallScoringModal;
