import { useEffect, useMemo, useState } from "react";
import {
  getLiveMatch,
  scoreRuns
} from "../services/matchService";

const LiveScoring = ({ match, onBack }) => {
  const [liveMatch, setLiveMatch] = useState(null);
  const [showMenu, setShowMenu] = useState(false);
  const [loading, setLoading] = useState(true);
  const [scoring, setScoring] = useState(false);
  const [error, setError] = useState("");

  // Purpose:
  // Load the latest live match state when the live scoring screen opens.
  useEffect(() => {
    const loadLiveMatch = async () => {
      try {
        setLoading(true);
        setError("");

        const result = await getLiveMatch(match.id);

        setLiveMatch(result);
      } catch (error) {
        console.error(
          "Failed to load live match:",
          error
        );

        const responseData =
          error.response?.data;

        if (typeof responseData === "string") {
          setError(responseData);
        } else if (responseData?.message) {
          setError(responseData.message);
        } else {
          setError("Unable to load live match.");
        }
      } finally {
        setLoading(false);
      }
    };

    loadLiveMatch();
  }, [match.id]);

  // Purpose:
  // Find a match player from the original match lineup using
  // the MatchPlayer ID returned by the live scoring API.
  const getPlayer = (matchPlayerId) => {
    if (!matchPlayerId) {
      return null;
    }

    return (match.players || []).find(
      (player) =>
        Number(player.matchPlayerId) ===
        Number(matchPlayerId)
    );
  };

  // Purpose:
  // Resolve the current striker from the live innings state.
  const striker = useMemo(() => {
    return getPlayer(
      liveMatch?.strikerMatchPlayerId
    );
  }, [
    liveMatch?.strikerMatchPlayerId,
    match.players
  ]);

  // Purpose:
  // Resolve the current non-striker from the live innings state.
  const nonStriker = useMemo(() => {
    return getPlayer(
      liveMatch?.nonStrikerMatchPlayerId
    );
  }, [
    liveMatch?.nonStrikerMatchPlayerId,
    match.players
  ]);

  // Purpose:
  // Resolve the current bowler from the live innings state.
  const bowler = useMemo(() => {
    return getPlayer(
      liveMatch?.currentBowlerMatchPlayerId
    );
  }, [
    liveMatch?.currentBowlerMatchPlayerId,
    match.players
  ]);

  // Purpose:
  // Calculate the current striker's runs and legal balls faced.
  const strikerStats = useMemo(() => {
    const balls = liveMatch?.balls || [];
    const playerId =
      liveMatch?.strikerMatchPlayerId;

    const playerBalls = balls.filter(
      (ball) =>
        Number(ball.strikerMatchPlayerId) ===
        Number(playerId)
    );

    return {
      runs: playerBalls.reduce(
        (total, ball) =>
          total +
          (ball.runs ?? 0) -
          (ball.extraRuns ?? 0),
        0
      ),
      balls: playerBalls.filter(
        (ball) => ball.isLegalDelivery
      ).length
    };
  }, [
    liveMatch?.balls,
    liveMatch?.strikerMatchPlayerId
  ]);

  // Purpose:
  // Calculate the non-striker's runs and legal balls faced.
  const nonStrikerStats = useMemo(() => {
    const balls = liveMatch?.balls || [];
    const playerId =
      liveMatch?.nonStrikerMatchPlayerId;

    const playerBalls = balls.filter(
      (ball) =>
        Number(ball.strikerMatchPlayerId) ===
        Number(playerId)
    );

    return {
      runs: playerBalls.reduce(
        (total, ball) =>
          total +
          (ball.runs ?? 0) -
          (ball.extraRuns ?? 0),
        0
      ),
      balls: playerBalls.filter(
        (ball) => ball.isLegalDelivery
      ).length
    };
  }, [
    liveMatch?.balls,
    liveMatch?.nonStrikerMatchPlayerId
  ]);

  // Purpose:
  // Calculate the current bowler's overs, runs conceded,
  // and wickets from the recorded deliveries.
  const bowlerStats = useMemo(() => {
    const balls = liveMatch?.balls || [];
    const bowlerId =
      liveMatch?.currentBowlerMatchPlayerId;

    const bowlerBalls = balls.filter(
      (ball) =>
        Number(ball.bowlerMatchPlayerId) ===
        Number(bowlerId)
    );

    const legalBalls = bowlerBalls.filter(
      (ball) => ball.isLegalDelivery
    ).length;

    const runsConceded = bowlerBalls.reduce(
      (total, ball) => {
        // Byes and leg-byes are not charged to the bowler.
        if (
          ball.extraType === "BYE" ||
          ball.extraType === "LEG BYE"
        ) {
          return total;
        }

        return total + (ball.runs ?? 0);
      },
      0
    );

    const wickets = bowlerBalls.filter(
      (ball) =>
        ball.wicketType &&
        ball.wicketType !== "RUN OUT"
    ).length;

    return {
      overs: `${Math.floor(legalBalls / 6)}.${legalBalls % 6}`,
      runsConceded,
      wickets
    };
  }, [
    liveMatch?.balls,
    liveMatch?.currentBowlerMatchPlayerId
  ]);

  // Purpose:
  // Calculate the current over notation from the number of legal
  // deliveries recorded in the innings.
  const currentOver = useMemo(() => {
    const legalBalls =
      liveMatch?.legalBalls ?? 0;

    const completedOvers =
      Math.floor(legalBalls / 6);

    const ballsInCurrentOver =
      legalBalls % 6;

    return `${completedOvers}.${ballsInCurrentOver}`;
  }, [
    liveMatch?.legalBalls
  ]);

  // Purpose:
  // Get the deliveries belonging to the latest over so the
  // current-over display can show the actual ball sequence.
  const currentOverBalls = useMemo(() => {
    const balls = liveMatch?.balls || [];

    if (balls.length === 0) {
      return [];
    }

    const latestBall =
      balls[balls.length - 1];

    return balls.filter(
      (ball) =>
        ball.overNumber ===
        latestBall.overNumber
    );
  }, [
    liveMatch?.balls
  ]);

  // Purpose:
  // Determine whether the current innings has finished using
  // the authoritative innings status returned by the backend.
  const inningsCompleted =
    liveMatch?.inningsStatus === "Completed";

  // Purpose:
  // Record a normal bat run and reload the latest live state
  // so all score information immediately reflects the delivery.
  const handleScoreRuns = async (runs) => {
    if (!liveMatch?.inningsId || scoring) {
      return;
    }

    try {
      setScoring(true);
      setError("");

      await scoreRuns(
        liveMatch.inningsId,
        runs
      );

      const updatedMatch =
        await getLiveMatch(match.id);

      setLiveMatch(updatedMatch);
    } catch (error) {
      console.error(
        "Failed to score runs:",
        error
      );

      const responseData =
        error.response?.data;

      if (typeof responseData === "string") {
        setError(responseData);
      } else if (responseData?.message) {
        setError(responseData.message);
      } else {
        setError("Unable to record runs.");
      }
    } finally {
      setScoring(false);
    }
  };

  if (loading) {
    return (
      <div className="container py-5 text-center">
        <h4>Loading live match...</h4>
      </div>
    );
  }

  if (error && !liveMatch) {
    return (
      <div className="container py-5 text-center">
        <div className="alert alert-danger">
          {error}
        </div>

        <button
          type="button"
          className="btn btn-secondary"
          onClick={onBack}
        >
          ← Back to Dashboard
        </button>
      </div>
    );
  }

  if (!liveMatch) {
    return (
      <div className="container py-5 text-center">
        <div className="alert alert-warning">
          Live match data is unavailable.
        </div>

        <button
          type="button"
          className="btn btn-secondary"
          onClick={onBack}
        >
          ← Back to Dashboard
        </button>
      </div>
    );
  }

  return (
    <div className="container-fluid p-0">

      {/* Match Header */}
      <nav className="navbar navbar-dark bg-dark">
        <div className="container-fluid">

          <span className="navbar-brand mb-0 h1">
            {liveMatch.team1Name}
            &nbsp; VS &nbsp;
            {liveMatch.team2Name}
          </span>

          <div className="position-relative">

            <button
              type="button"
              className="btn btn-outline-light"
              onClick={() =>
                setShowMenu(
                  (current) => !current
                )
              }
            >
              ⋮
            </button>

            {showMenu && (
              <div
                className="position-absolute end-0 mt-2 bg-white border rounded shadow"
                style={{
                  minWidth: "180px",
                  zIndex: 1000
                }}
              >
                <button
                  type="button"
                  className="btn btn-link text-danger text-decoration-none w-100 text-start"
                >
                  Cancel Match
                </button>
              </div>
            )}

          </div>
        </div>
      </nav>

      <div className="container py-4">

        {error && (
          <div className="alert alert-danger">
            {error}
          </div>
        )}

        {/* Score */}
        <div className="text-center mb-4">

          <h2>
            {liveMatch.battingTeam}
          </h2>

          <h1>
            {liveMatch.totalRuns ?? 0}
            {" / "}
            {liveMatch.wickets ?? 0}
          </h1>

          <h5>
            {currentOver} overs
          </h5>

          {inningsCompleted && (
            <div className="alert alert-success mt-3">
              <strong>
                INNINGS COMPLETED
              </strong>
            </div>
          )}

        </div>

        {/* Active Players */}
        <div className="row g-3 mb-4">

          {/* Striker */}
          <div className="col-md-4">
            <div className="card h-100">
              <div className="card-body">

                <h5 className="card-title">
                  Striker
                </h5>

                <h4>
                  {striker?.displayName ||
                    "Unknown Player"}
                </h4>

                <p className="mb-0">
                  {strikerStats.runs}{" "}
                  ({strikerStats.balls})
                </p>

              </div>
            </div>
          </div>

          {/* Non-Striker */}
          <div className="col-md-4">
            <div className="card h-100">
              <div className="card-body">

                <h5 className="card-title">
                  Non-Striker
                </h5>

                <h4>
                  {nonStriker?.displayName ||
                    "Unknown Player"}
                </h4>

                <p className="mb-0">
                  {nonStrikerStats.runs}{" "}
                  ({nonStrikerStats.balls})
                </p>

              </div>
            </div>
          </div>

          {/* Bowler */}
          <div className="col-md-4">
            <div className="card h-100">
              <div className="card-body">

                <h5 className="card-title">
                  {inningsCompleted
                    ? "Innings Completed"
                    : "Bowler"}
                </h5>

                <h4>
                  {inningsCompleted
                    ? "Waiting for next innings"
                    : bowler?.displayName ||
                      "No Bowler Selected"}
                </h4>

                <p className="mb-0">
                  {inningsCompleted
                    ? "First innings finished"
                    : `${bowlerStats.overs} - ${bowlerStats.runsConceded} - ${bowlerStats.wickets}`}
                </p>

              </div>
            </div>
          </div>

        </div>

        {/* Current Over */}
        <div className="card mb-4">

          <div className="card-body">

            <h4 className="card-title">
              CURRENT OVER ({currentOver})
            </h4>

            <div className="d-flex gap-2 flex-wrap">

              {currentOverBalls.length === 0 ? (
                <span className="text-muted">
                  No deliveries yet
                </span>
              ) : (
                currentOverBalls.map(
                  (ball) => (
                    <span
                      key={ball.id}
                      className="badge bg-secondary fs-6 p-2"
                    >
                      {ball.wicketType ||
                        ball.extraType ||
                        ball.runs}
                    </span>
                  )
                )
              )}

            </div>

          </div>

        </div>

        {/* Scoring Controls */}
        <div className="row g-3">

          {/* Runs */}
          <div className="col-md-4">
            <div className="card">

              <div className="card-body">

                <h4>
                  RUNS
                </h4>

                <div className="d-grid gap-2">

                  {[0, 1, 2, 3, 4, 6].map(
                    (runs) => (
                      <button
                        key={runs}
                        type="button"
                        className="btn btn-primary"
                        disabled={
                          scoring ||
                          inningsCompleted
                        }
                        onClick={() =>
                          handleScoreRuns(runs)
                        }
                      >
                        {runs}
                      </button>
                    )
                  )}

                </div>

              </div>

            </div>
          </div>

          {/* Extras */}
          <div className="col-md-4">
            <div className="card">

              <div className="card-body">

                <h4>
                  EXTRAS
                </h4>

                <div className="d-grid gap-2">

                  {[
                    "WIDE",
                    "NO BALL",
                    "BYE",
                    "LEG BYE"
                  ].map((extra) => (
                    <button
                      key={extra}
                      type="button"
                      className="btn btn-warning"
                      disabled
                    >
                      {extra}
                    </button>
                  ))}

                </div>

              </div>

            </div>
          </div>

          {/* Wickets */}
          <div className="col-md-4">
            <div className="card">

              <div className="card-body">

                <h4>
                  WICKETS
                </h4>

                <div className="d-grid gap-2">

                  {[
                    "BOWLED",
                    "CAUGHT",
                    "RUN OUT",
                    "LBW",
                    "STUMPED",
                    "HIT WICKET"
                  ].map((wicket) => (
                    <button
                      key={wicket}
                      type="button"
                      className="btn btn-danger"
                      disabled
                    >
                      {wicket}
                    </button>
                  ))}

                </div>

              </div>

            </div>
          </div>

        </div>

        {/* Undo */}
        <div className="text-center mt-4">

          <button
            type="button"
            className="btn btn-secondary"
            disabled
          >
            UNDO LAST BALL
          </button>

        </div>

        {/* Back */}
        <div className="text-center mt-3">

          <button
            type="button"
            className="btn btn-link"
            onClick={onBack}
          >
            ← Back to Dashboard
          </button>

        </div>

      </div>
    </div>
  );
};

export default LiveScoring;