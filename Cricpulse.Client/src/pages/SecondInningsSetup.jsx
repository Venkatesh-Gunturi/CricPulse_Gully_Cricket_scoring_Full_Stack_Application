import { useMemo, useState } from "react";
import { startInnings } from "../services/matchService";

const SecondInningsSetup = ({
  match,
  onInningsStarted
}) => {
  const [strikerId, setStrikerId] = useState("");
  const [nonStrikerId, setNonStrikerId] = useState("");
  const [bowlerId, setBowlerId] = useState("");

  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const battingTeamName =
    match.firstInningsBowlingTeam;

  const bowlingTeamName =
    match.firstInningsBattingTeam;

  const getTeamCode = (teamName) => {
    if (!teamName) {
      return null;
    }

    if (
      teamName === match?.team1Name
    ) {
      return "Team1";
    }

    if (
      teamName === match?.team2Name
    ) {
      return "Team2";
    }

    return null;
  };

  const battingTeamCode =
    getTeamCode(battingTeamName);

  const bowlingTeamCode =
    getTeamCode(bowlingTeamName);

  const battingPlayers = useMemo(() => {
    if (!battingTeamCode) {
      return [];
    }

    return (match?.players || []).filter(
      (player) =>
        player?.team === battingTeamCode
    );
  }, [
    match?.players,
    battingTeamCode
  ]);

  const bowlingPlayers = useMemo(() => {
    if (!bowlingTeamCode) {
      return [];
    }

    return (match?.players || []).filter(
      (player) =>
        player?.team === bowlingTeamCode
    );
  }, [
    match?.players,
    bowlingTeamCode
  ]);

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

  const handleStartInnings = async () => {
    setError("");

    if (!battingTeamName) {
      setError(
        "Batting team information is missing."
      );
      return;
    }

    if (!bowlingTeamName) {
      setError(
        "Bowling team information is missing."
      );
      return;
    }

    if (!battingTeamCode) {
      setError(
        `Unable to identify batting team "${battingTeamName}".`
      );
      return;
    }

    if (!bowlingTeamCode) {
      setError(
        `Unable to identify bowling team "${bowlingTeamName}".`
      );
      return;
    }

    if (!strikerId) {
      setError(
        "Please select a striker."
      );
      return;
    }

    if (!nonStrikerId) {
      setError(
        "Please select a non-striker."
      );
      return;
    }

    if (!bowlerId) {
      setError(
        "Please select a bowler."
      );
      return;
    }

    if (
      String(strikerId) ===
      String(nonStrikerId)
    ) {
      setError(
        "Striker and non-striker must be different."
      );
      return;
    }

    const selectedStriker =
      battingPlayers.find(
        (player) =>
          String(player.matchPlayerId) ===
          String(strikerId)
      );

    const selectedNonStriker =
      battingPlayers.find(
        (player) =>
          String(player.matchPlayerId) ===
          String(nonStrikerId)
      );

    const selectedBowler =
      bowlingPlayers.find(
        (player) =>
          String(player.matchPlayerId) ===
          String(bowlerId)
      );

    if (!selectedStriker) {
      setError(
        "Selected striker does not belong to the batting team."
      );
      return;
    }

    if (!selectedNonStriker) {
      setError(
        "Selected non-striker does not belong to the batting team."
      );
      return;
    }

    if (!selectedBowler) {
      setError(
        "Selected bowler does not belong to the bowling team."
      );
      return;
    }

    try {
      setLoading(true);

      const request = {
        matchId: match.id,
        inningsNumber: 2,
        strikerMatchPlayerId:
          Number(strikerId),
        nonStrikerMatchPlayerId:
          Number(nonStrikerId),
        bowlerMatchPlayerId:
          Number(bowlerId)
      };

      console.log(
        "Starting second innings:",
        request
      );

      console.log(
        "Batting team:",
        battingTeamName,
        battingTeamCode
      );

      console.log(
        "Bowling team:",
        bowlingTeamName,
        bowlingTeamCode
      );

      const result =
        await startInnings(
          request.matchId,
          request.inningsNumber,
          request.strikerMatchPlayerId,
          request.nonStrikerMatchPlayerId,
          request.bowlerMatchPlayerId
        );

      console.log(
        "Second innings started:",
        result
      );

      onInningsStarted(result);
    } catch (error) {
      console.error(
        "Failed to start second innings:",
        error
      );

      console.error(
        "Status:",
        error.response?.status
      );

      console.error(
        "Response:",
        error.response?.data
      );

      const responseData =
        error.response?.data;

      if (
        typeof responseData ===
        "string"
      ) {
        setError(responseData);
      } else if (
        responseData?.message
      ) {
        setError(responseData.message);
      } else if (
        responseData?.title
      ) {
        setError(responseData.title);
      } else {
        setError(
          `Unable to start second innings. Status: ${
            error.response?.status ||
            "unknown"
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
                  setError("");
                }}
                disabled={loading}
              >
                <option value="">
                  Select striker
                </option>

                {battingPlayers.map(
                  (player) => (
                    <option
                      key={
                        player.matchPlayerId
                      }
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

        <div className="col-md-4">
          <div className="card h-100">

            <div className="card-body">

              <h5>
                Non-Striker
              </h5>

              <select
                className="form-select mt-3"
                value={nonStrikerId}
                onChange={(event) => {
                  setNonStrikerId(
                    event.target.value
                  );
                  setError("");
                }}
                disabled={loading}
              >
                <option value="">
                  Select non-striker
                </option>

                {availableNonStrikers.map(
                  (player) => (
                    <option
                      key={
                        player.matchPlayerId
                      }
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

        <div className="col-md-4">
          <div className="card h-100">

            <div className="card-body">

              <h5>
                Bowler
              </h5>

              <select
                className="form-select mt-3"
                value={bowlerId}
                onChange={(event) => {
                  setBowlerId(
                    event.target.value
                  );
                  setError("");
                }}
                disabled={loading}
              >
                <option value="">
                  Select bowler
                </option>

                {bowlingPlayers.map(
                  (player) => (
                    <option
                      key={
                        player.matchPlayerId
                      }
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
          onClick={
            handleStartInnings
          }
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