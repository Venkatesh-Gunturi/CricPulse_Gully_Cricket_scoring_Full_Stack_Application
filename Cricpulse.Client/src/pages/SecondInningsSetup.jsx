import { useMemo, useState } from "react";
import { startInnings } from "../services/matchService";

const SecondInningsSetup = ({ match, onInningsStarted }) => {
  const [strikerId, setStrikerId] = useState("");
  const [nonStrikerId, setNonStrikerId] = useState("");
  const [bowlerId, setBowlerId] = useState("");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  // Purpose:
  // Determine the second innings batting team from the team that
  // bowled during the completed first innings.
  const battingTeamName =
    match.firstInningsBowlingTeam;

  // Purpose:
  // Determine the second innings bowling team from the team that
  // batted during the completed first innings.
  const bowlingTeamName =
    match.firstInningsBattingTeam;

  // Purpose:
  // Convert the batting team name into the Team1/Team2 code
  // stored on MatchPlayer records.
  const battingTeamCode =
    battingTeamName === match.team1Name
      ? "Team1"
      : "Team2";

  // Purpose:
  // Convert the bowling team name into the Team1/Team2 code
  // stored on MatchPlayer records.
  const bowlingTeamCode =
    bowlingTeamName === match.team1Name
      ? "Team1"
      : "Team2";

  // Purpose:
  // Get all players belonging to the second innings batting team.
  const battingPlayers = useMemo(() => {
    return (match.players || []).filter(
      (player) =>
        player.team === battingTeamCode
    );
  }, [
    match.players,
    battingTeamCode
  ]);

  // Purpose:
  // Get all players belonging to the second innings bowling team.
  const bowlingPlayers = useMemo(() => {
    return (match.players || []).filter(
      (player) =>
        player.team === bowlingTeamCode
    );
  }, [
    match.players,
    bowlingTeamCode
  ]);

  // Purpose:
  // Prevent the selected striker from being selected again
  // as the non-striker.
  const availableNonStrikers = useMemo(() => {
    return battingPlayers.filter(
      (player) =>
        String(player.matchPlayerId) !==
        String(strikerId)
    );
  }, [
    battingPlayers,
    strikerId
  ]);

  // Purpose:
  // Validate the selected players and start the second innings
  // through the backend before returning to live scoring.
  const handleStartInnings = async () => {
    setError("");

    if (!strikerId) {
      setError("Please select a striker.");
      return;
    }

    if (!nonStrikerId) {
      setError("Please select a non-striker.");
      return;
    }

    if (!bowlerId) {
      setError("Please select a bowler.");
      return;
    }

    if (strikerId === nonStrikerId) {
      setError(
        "Striker and non-striker must be different."
      );
      return;
    }

    try {
      setLoading(true);

      await startInnings(
        match.id,
        Number(strikerId),
        Number(nonStrikerId),
        Number(bowlerId)
      );

      onInningsStarted();
    } catch (error) {
      console.error(
        "Failed to start second innings:",
        error
      );

      setError(
        error.response?.data ||
        "Unable to start second innings."
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container py-4">

      <div className="text-center mb-4">

        <h2>
          SECOND INNINGS
        </h2>

        <h4 className="mt-3">
          {battingTeamName}
        </h4>

        <p className="text-muted">
          Batting
        </p>

      </div>

      {error && (
        <div className="alert alert-danger text-center">
          {error}
        </div>
      )}

      <div className="row g-4">

        {/* Striker */}
        <div className="col-md-4">
          <div className="card h-100">

            <div className="card-body">

              <h5>
                Striker
              </h5>

              <select
                className="form-select mt-3"
                value={strikerId}
                onChange={(event) => {
                  setStrikerId(
                    event.target.value
                  );

                  setNonStrikerId("");
                }}
                disabled={loading}
              >
                <option value="">
                  Select striker
                </option>

                {battingPlayers.map(
                  (player) => (
                    <option
                      key={player.matchPlayerId}
                      value={
                        player.matchPlayerId
                      }
                    >
                      {player.displayName}
                    </option>
                  )
                )}

              </select>

            </div>

          </div>
        </div>

        {/* Non-Striker */}
        <div className="col-md-4">
          <div className="card h-100">

            <div className="card-body">

              <h5>
                Non-Striker
              </h5>

              <select
                className="form-select mt-3"
                value={nonStrikerId}
                onChange={(event) =>
                  setNonStrikerId(
                    event.target.value
                  )
                }
                disabled={loading}
              >
                <option value="">
                  Select non-striker
                </option>

                {availableNonStrikers.map(
                  (player) => (
                    <option
                      key={player.matchPlayerId}
                      value={
                        player.matchPlayerId
                      }
                    >
                      {player.displayName}
                    </option>
                  )
                )}

              </select>

            </div>

          </div>
        </div>

        {/* Bowler */}
        <div className="col-md-4">
          <div className="card h-100">

            <div className="card-body">

              <h5>
                Bowler
              </h5>

              <select
                className="form-select mt-3"
                value={bowlerId}
                onChange={(event) =>
                  setBowlerId(
                    event.target.value
                  )
                }
                disabled={loading}
              >
                <option value="">
                  Select bowler
                </option>

                {bowlingPlayers.map(
                  (player) => (
                    <option
                      key={player.matchPlayerId}
                      value={
                        player.matchPlayerId
                      }
                    >
                      {player.displayName}
                    </option>
                  )
                )}

              </select>

            </div>

          </div>
        </div>

      </div>

      <div className="text-center mt-4">

        <button
          type="button"
          className="btn btn-primary px-5"
          onClick={handleStartInnings}
          disabled={loading}
        >
          {loading
            ? "STARTING..."
            : "START SECOND INNINGS"}
        </button>

      </div>

    </div>
  );
};

export default SecondInningsSetup;