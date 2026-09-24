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

  const battingTeamName =
    tossResult?.battingFirstTeam;

  const bowlingTeamName =
    tossResult?.bowlingFirstTeam;

  /*
   * Determine whether a player belongs to Team 1.
   *
   * MatchPlayer.Team is stored as "Team1" / "Team2".
   */
  const isTeam1Player = (player) => {
    return (
      player?.team === "Team1" ||
      player?.team === match?.team1Name
    );
  };

  /*
   * Convert an actual team name into the MatchPlayer
   * team identifier used by the backend.
   */
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

  /*
   * Players belonging to the batting team.
   */
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

  /*
   * Players belonging to the bowling team.
   */
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

  /*
   * Do not allow the striker to also be selected
   * as the non-striker.
   */
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
        "Please select the striker."
      );
      return;
    }

    if (!nonStrikerId) {
      setError(
        "Please select the non-striker."
      );
      return;
    }

    if (!bowlerId) {
      setError(
        "Please select the bowler."
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
        inningsNumber: 1,
        strikerMatchPlayerId:
          Number(strikerId),
        nonStrikerMatchPlayerId:
          Number(nonStrikerId),
        bowlerMatchPlayerId:
          Number(bowlerId)
      };

      console.log(
        "Starting first innings:",
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

      console.log(
        "Selected striker:",
        selectedStriker
      );

      console.log(
        "Selected non-striker:",
        selectedNonStriker
      );

      console.log(
        "Selected bowler:",
        selectedBowler
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
        "Start innings response:",
        result
      );

      onInningsStarted(result);
    } catch (error) {
      console.error(
        "Failed to start innings:",
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
        setError(
          responseData.message
        );
      } else if (
        responseData?.title
      ) {
        setError(
          responseData.title
        );
      } else {
        setError(
          `Unable to start innings. Status: ${
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
          {match.team1Name}
          &nbsp; VS &nbsp;
          {match.team2Name}
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
                const value =
                  event.target.value;

                setStrikerId(value);

                if (
                  value ===
                  nonStrikerId
                ) {
                  setNonStrikerId("");
                }

                setError("");
              }}
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

          <div className="mb-4">
            <label className="form-label fw-bold">
              Non-Striker
            </label>

            <select
              className="form-select"
              value={nonStrikerId}
              onChange={(event) => {
                setNonStrikerId(
                  event.target.value
                );
                setError("");
              }}
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
                setBowlerId(
                  event.target.value
                );
                setError("");
              }}
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

          {error && (
            <div className="alert alert-danger">
              {error}
            </div>
          )}

          <div className="text-center">
            <button
              type="button"
              className="btn btn-success btn-lg"
              onClick={
                handleStartInnings
              }
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