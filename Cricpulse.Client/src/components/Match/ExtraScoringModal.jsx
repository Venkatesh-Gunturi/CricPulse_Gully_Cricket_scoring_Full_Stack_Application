import { useEffect, useState } from "react";

const ExtraScoringModal = ({
  extraType,
  onClose,
  onConfirm,
  loading
}) => {
  const [selectedRuns, setSelectedRuns] = useState(null);

  useEffect(() => {
    setSelectedRuns(null);
  }, [extraType]);

  if (!extraType) {
    return null;
  }

  const isNoBall = extraType === "NO BALL";

  const title = extraType;

  const getRunLabel = () => {
    if (extraType === "WIDE") {
      return "Total wide runs";
    }

    if (extraType === "NO BALL") {
      return "Total no-ball runs";
    }

    if (extraType === "BYE") {
      return "Total bye runs";
    }

    return "Total leg-bye runs";
  };

  const calculateTotalRuns = () => {
    if (selectedRuns === null) {
      return 0;
    }

    return selectedRuns;
  };

  const getBatterRuns = () => {
    if (!isNoBall || selectedRuns === null) {
      return 0;
    }

    return selectedRuns - 1;
  };

  const getWideCompletedRuns = () => {
    if (selectedRuns === null) {
      return 0;
    }

    return selectedRuns - 1;
  };

  const handleConfirm = () => {
    if (selectedRuns === null) {
      return;
    }

    const totalRuns = calculateTotalRuns();

    const batterRuns = isNoBall
      ? getBatterRuns()
      : 0;

    onConfirm(
      extraType,
      totalRuns,
      batterRuns
    );
  };

  const runOptions = isNoBall
    ? [1, 2, 3, 4, 5, 6, 7]
    : [1, 2, 3, 4, 5, 6];

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

            <div className="modal-header">
              <h5 className="modal-title">
                {title}
              </h5>

              <button
                type="button"
                className="btn-close"
                onClick={onClose}
                disabled={loading}
              />
            </div>

            <div className="modal-body">

              <div className="text-center mb-3">
                <h6 className="text-muted mb-1">
                  {getRunLabel()}
                </h6>

                <p className="small text-muted mb-0">
                  {isNoBall
                    ? "Select the total runs produced by the no-ball."
                    : "Select the total runs produced by this extra."}
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

                {/* Run Out - functionality will be added later */}
                <button
                  type="button"
                  className="btn btn-danger"
                  style={{
                    minWidth: "100px"
                  }}
                  disabled={loading}
                  onClick={() => {
                    // Run Out functionality will be implemented later.
                  }}
                >
                  RUN OUT
                </button>

              </div>

              {selectedRuns !== null && (
                <div className="alert alert-info text-center mt-4 mb-0">

                  {extraType === "WIDE" ? (
                    <div className="fs-5">
                      Total:{" "}
                      <strong>1</strong>{" "}
                      extra +{" "}
                      <strong>
                        {getWideCompletedRuns()}
                      </strong>{" "}
                      runs ={" "}
                      <strong>
                        {calculateTotalRuns()}
                      </strong>{" "}
                      runs
                    </div>
                  ) : isNoBall ? (
                    <>
                      <div>
                        No-ball: <strong>1</strong>
                      </div>

                      <div>
                        Batter runs:{" "}
                        <strong>
                          {getBatterRuns()}
                        </strong>
                      </div>

                      <hr />

                      <div className="fs-5">
                        Total:{" "}
                        <strong>
                          {calculateTotalRuns()}
                        </strong>{" "}
                        runs
                      </div>
                    </>
                  ) : (
                    <div className="fs-5">
                      Total:{" "}
                      <strong>
                        {calculateTotalRuns()}
                      </strong>{" "}
                      runs
                    </div>
                  )}

                </div>
              )}

            </div>

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
                  selectedRuns === null
                }
              >
                {loading
                  ? "Scoring..."
                  : `Add ${
                      selectedRuns === null
                        ? ""
                        : calculateTotalRuns()
                    } Runs`}
              </button>

            </div>

          </div>
        </div>
      </div>
    </>
  );
};

export default ExtraScoringModal;
