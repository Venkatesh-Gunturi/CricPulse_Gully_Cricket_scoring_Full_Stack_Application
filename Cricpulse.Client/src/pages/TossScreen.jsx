import { useState } from "react";
import { recordToss } from "../services/matchService";

const TossScreen = ({ match, onTossComplete }) => {
  const [tossWinner, setTossWinner] = useState(null);
  const [tossConfirmed, setTossConfirmed] = useState(false);
  const [tossDecision, setTossDecision] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  // Purpose:
  // Randomly select one of the two teams as the toss winner and allow the
  // umpire to confirm or redraw the result before making the toss decision.
  const handleToss = () => {
    const winner =
      Math.random() < 0.5
        ? match.team1Name
        : match.team2Name;

    setTossWinner(winner);
    setTossConfirmed(false);
    setTossDecision(null);
    setError("");
  };

  // Purpose:
  // Confirm the randomly generated toss result so the winner can choose
  // whether to bat or bowl first.
  const handleConfirmToss = () => {
    if (!tossWinner) {
      return;
    }

    setTossConfirmed(true);
    setError("");
  };

  // Purpose:
  // Persist the toss winner's decision and determine the batting and bowling
  // teams before moving to innings setup.
  const handleDecision = async (decision) => {
    if (!tossWinner) {
      return;
    }

    try {
      setLoading(true);
      setError("");

      const result = await recordToss(
        match.id,
        tossWinner,
        decision
      );

      const battingFirstTeam =
        decision === "BAT"
          ? tossWinner
          : tossWinner === match.team1Name
            ? match.team2Name
            : match.team1Name;

      const bowlingFirstTeam =
        battingFirstTeam === match.team1Name
          ? match.team2Name
          : match.team1Name;

      setTossDecision(decision);

      onTossComplete({
        tossWinnerTeam: tossWinner,
        tossDecision: decision,
        battingFirstTeam,
        bowlingFirstTeam,
        ...result
      });
    } catch (error) {
      console.error(
        "Failed to record toss:",
        error
      );

      const responseData =
        error.response?.data;

      if (typeof responseData === "string") {
        setError(responseData);
      } else if (responseData?.message) {
        setError(responseData.message);
      } else if (responseData?.title) {
        setError(responseData.title);
      } else {
        setError("Unable to record toss.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container py-4">

      <div className="text-center mb-4">
        <h2>
          {match.team1Name} &nbsp; VS &nbsp; {match.team2Name}
        </h2>
      </div>

      <div
        className="card mx-auto"
        style={{ maxWidth: "600px" }}
      >
        <div className="card-header text-center">
          <h4 className="mb-0">
            TOSS
          </h4>
        </div>

        <div className="card-body text-center">

          {!tossWinner && (
            <>
              <div
                className="rounded-circle border mx-auto mb-4 d-flex align-items-center justify-content-center"
                style={{
                  width: "140px",
                  height: "140px",
                  fontSize: "22px",
                  fontWeight: "bold"
                }}
              >
                🪙
              </div>

              <button
                type="button"
                className="btn btn-primary btn-lg"
                onClick={handleToss}
                disabled={loading}
              >
                TOSS COIN
              </button>
            </>
          )}

          {tossWinner && !tossConfirmed && (
            <>
              <div
                className="rounded-circle border mx-auto mb-4 d-flex align-items-center justify-content-center"
                style={{
                  width: "140px",
                  height: "140px",
                  fontSize: "18px",
                  fontWeight: "bold"
                }}
              >
                {tossWinner}
              </div>

              <h4 className="mb-4">
                {tossWinner} won the toss
              </h4>

              <div className="d-flex justify-content-center gap-3">

                <button
                  type="button"
                  className="btn btn-success"
                  onClick={handleConfirmToss}
                  disabled={loading}
                >
                  CONFIRM
                </button>

                <button
                  type="button"
                  className="btn btn-outline-secondary"
                  onClick={handleToss}
                  disabled={loading}
                >
                  RE-DRAW
                </button>

              </div>
            </>
          )}

          {tossWinner &&
            tossConfirmed &&
            !tossDecision && (
              <>
                <h4 className="mb-2">
                  {tossWinner} won the toss
                </h4>

                <p className="text-muted mb-4">
                  What do they choose?
                </p>

                <div className="d-flex justify-content-center gap-3">

                  <button
                    type="button"
                    className="btn btn-primary btn-lg"
                    onClick={() =>
                      handleDecision("BAT")
                    }
                    disabled={loading}
                  >
                    {loading ? "Saving..." : "🏏 BAT"}
                  </button>

                  <button
                    type="button"
                    className="btn btn-dark btn-lg"
                    onClick={() =>
                      handleDecision("BOWL")
                    }
                    disabled={loading}
                  >
                    {loading ? "Saving..." : "⚾ BOWL"}
                  </button>

                </div>
              </>
          )}

          {tossWinner &&
            tossConfirmed &&
            tossDecision && (
              <div>

                <h4>
                  {tossWinner} chose to{" "}
                  {tossDecision}
                </h4>

                <p className="mt-3 mb-0">
                  Batting first:{" "}
                  <strong>
                    {tossDecision === "BAT"
                      ? tossWinner
                      : tossWinner === match.team1Name
                        ? match.team2Name
                        : match.team1Name}
                  </strong>
                </p>

              </div>
          )}

          {error && (
            <div className="alert alert-danger mt-4 mb-0">
              {error}
            </div>
          )}

        </div>
      </div>
    </div>
  );
};

export default TossScreen;