import { useMemo, useState } from "react";
import { startInnings } from "../services/matchService";

const InningsSetup = ({
  match,
  tossResult,
  onInningsStarted
}) => {
  const [strikerId, setStrikerId] = useState("");
  const [nonStrikerId, setNonStrikerId] = useState("");
  const [bowlerId, setBowlerId] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const battingTeamName = tossResult.battingFirstTeam;
  const bowlingTeamName = tossResult.bowlingFirstTeam;

  // Purpose:
  // Convert the actual batting team name into the Team1/Team2 code
  // used by MatchPlayer records.
  const battingTeamCode =
    battingTeamName === match.team1Name
      ? "Team1"
      : "Team2";

  // Purpose:
  // Convert the actual bowling team name into the Team1/Team2 code
  // used by MatchPlayer records.
  const bowlingTeamCode =
    bowlingTeamName === match.team1Name
      ? "Team1"
      : "Team2";

  // Purpose:
  // Get all players belonging to the team batting this innings.
  const battingPlayers = useMemo(() => {
    return (match.players || []).filter(
      (player) => player.team === battingTeamCode
    );
  }, [match.players, battingTeamCode]);

  // Purpose:
  // Get all players belonging to the team bowling this innings.
  const bowlingPlayers = useMemo(() => {
    return (match.players || []).filter(
      (player) => player.team === bowlingTeamCode
    );
  }, [match.players, bowlingTeamCode]);

  // Purpose:
  // Prevent the selected striker from appearing as an available
  // non-striker.
  const availableNonStrikers = useMemo(() => {
    return battingPlayers.filter(
      (player) =>
       String(player.matchPlayerId) !== String(strikerId)
    );
  }, [battingPlayers, strikerId]);

 // Purpose:
// Validate the selected players, call the backend to start the innings,
// and expose the actual API error so failures can be diagnosed correctly.
const handleStartInnings = async () => {
  setError("");

  if (!strikerId) {
    setError("Please select the striker.");
    return;
  }

  if (!nonStrikerId) {
    setError("Please select the non-striker.");
    return;
  }

  if (!bowlerId) {
    setError("Please select the bowler.");
    return;
  }

  if (strikerId === nonStrikerId) {
    setError("Striker and non-striker must be different.");
    return;
  }

  try {
    setLoading(true);

    console.log("Starting innings with:", {
      matchId: match.id,
      strikerMatchPlayerId: Number(strikerId),
      nonStrikerMatchPlayerId: Number(nonStrikerId),
      bowlerMatchPlayerId: Number(bowlerId)
    });

    const result = await startInnings(
      match.id,
      Number(strikerId),
      Number(nonStrikerId),
      Number(bowlerId)
    );

    console.log("Start innings response:", result);

    onInningsStarted(result);
  } catch (error) {
    console.error("Failed to start innings:", error);
    console.error("Status:", error.response?.status);
    console.error("Response:", error.response?.data);

    const responseData = error.response?.data;

    if (typeof responseData === "string") {
      setError(responseData);
    } else if (responseData?.message) {
      setError(responseData.message);
    } else if (responseData?.title) {
      setError(responseData.title);
    } else {
      setError(
        `Unable to start innings. Status: ${
          error.response?.status || "unknown"
        }`
      );
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

        <h4 className="mt-3">
          FIRST INNINGS
        </h4>
      </div>

      <div
        className="card mx-auto"
        style={{ maxWidth: "650px" }}
      >
        <div className="card-body">

          <div className="mb-4 text-center">
            <small className="text-muted">
              BATTING TEAM
            </small>

            <h3 className="mt-1">
              {battingTeamName}
            </h3>
          </div>

          <div className="mb-3">
            <label className="form-label fw-bold">
              Striker
            </label>

            <select
              className="form-select"
              value={strikerId}
              onChange={(event) => {
                setStrikerId(event.target.value);

                if (
                  event.target.value === nonStrikerId
                ) {
                  setNonStrikerId("");
                }

                setError("");
              }}
            >
              <option value="">
                Select striker
              </option>

              {battingPlayers.map((player) => (
                <option
                  key={player.matchPlayerId}
                 value={player.matchPlayerId}
                >
                  {player.displayName}
                </option>
              ))}
            </select>
          </div>

          <div className="mb-4">
            <label className="form-label fw-bold">
              Non-Striker
            </label>

            <select
              className="form-select"
              value={nonStrikerId}
              onChange={(event) => {
                setNonStrikerId(event.target.value);
                setError("");
              }}
            >
              <option value="">
                Select non-striker
              </option>

              {availableNonStrikers.map((player) => (
                <option
                   key={player.matchPlayerId}
                   value={player.matchPlayerId}
                >
                  {player.displayName}
                </option>
              ))}
            </select>
          </div>

          <hr />

          <div className="mb-4 text-center">
            <small className="text-muted">
              BOWLING TEAM
            </small>

            <h3 className="mt-1">
              {bowlingTeamName}
            </h3>
          </div>

          <div className="mb-4">
            <label className="form-label fw-bold">
              Bowler
            </label>

            <select
              className="form-select"
              value={bowlerId}
              onChange={(event) => {
                setBowlerId(event.target.value);
                setError("");
              }}
            >
              <option value="">
                Select bowler
              </option>

              {bowlingPlayers.map((player) => (
                <option
                  key={player.matchPlayerId}
                  value={player.matchPlayerId}
                >
                  {player.displayName}
                </option>
              ))}
            </select>
          </div>

          {error && (
            <div className="alert alert-danger">
              {error}
            </div>
          )}

          <div className="text-center">
            <button
              type="button"
              className="btn btn-success btn-lg"
              onClick={handleStartInnings}
              disabled={loading}
            >
              {loading
                ? "Starting..."
                : "START INNINGS"}
            </button>
          </div>

        </div>
      </div>
    </div>
  );
};

export default InningsSetup;