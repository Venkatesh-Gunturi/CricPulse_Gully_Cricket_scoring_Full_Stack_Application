import { useEffect, useMemo, useState } from "react";
import { getLiveMatch } from "../services/matchService";

const LiveScoring = ({ match, onBack }) => {
  const [liveMatch, setLiveMatch] = useState(null);
  const [showMenu, setShowMenu] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Purpose:
  // Load the latest live scoring state when the live scoring screen opens.
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

        setError(
          error.response?.data ||
          "Unable to load live match."
        );
      } finally {
        setLoading(false);
      }
    };

    loadLiveMatch();
  }, [match.id]);

  // Purpose:
  // Find a MatchPlayer from the original match lineup using its
  // MatchPlayer ID returned by the live scoring API.
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
  // Resolve the selected striker into the player's display information.
  const striker = useMemo(() => {
    return getPlayer(
      liveMatch?.strikerMatchPlayerId
    );
  }, [
    liveMatch?.strikerMatchPlayerId,
    match.players
  ]);

  // Purpose:
  // Resolve the selected non-striker into the player's display information.
  const nonStriker = useMemo(() => {
    return getPlayer(
      liveMatch?.nonStrikerMatchPlayerId
    );
  }, [
    liveMatch?.nonStrikerMatchPlayerId,
    match.players
  ]);

  // Purpose:
  // Resolve the current bowler into the player's display information.
  const bowler = useMemo(() => {
    return getPlayer(
      liveMatch?.currentBowlerMatchPlayerId
    );
  }, [
    liveMatch?.currentBowlerMatchPlayerId,
    match.players
  ]);

  // Purpose:
  // Calculate the current over notation from the number of legal
  // deliveries bowled in the active innings.
  const currentOver = useMemo(() => {
    const legalBalls = liveMatch?.legalBalls ?? 0;

    const completedOvers = Math.floor(
      legalBalls / 6
    );

    const ballsInCurrentOver =
      legalBalls % 6;

    return `${completedOvers}.${ballsInCurrentOver}`;
  }, [liveMatch?.legalBalls]);

  // Purpose:
  // Get only the balls belonging to the current over so the live
  // screen can display the delivery sequence.
  const currentOverBalls = useMemo(() => {
    const balls = liveMatch?.balls || [];

    if (balls.length === 0) {
      return [];
    }

    const latestBall = balls[balls.length - 1];

    return balls.filter(
      (ball) =>
        ball.overNumber ===
        latestBall.overNumber
    );
  }, [liveMatch?.balls]);

  if (loading) {
    return (
      <div className="container py-5 text-center">
        <h4>Loading live match...</h4>
      </div>
    );
  }

  if (error) {
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

        </div>

        {/* Current Players */}
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
                  0 (0)
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
                  0 (0)
                </p>

              </div>
            </div>
          </div>

          {/* Bowler */}
          <div className="col-md-4">
            <div className="card h-100">
              <div className="card-body">

                <h5 className="card-title">
                  Bowler
                </h5>

                <h4>
                  {bowler?.displayName ||
                    "Unknown Player"}
                </h4>

                <p className="mb-0">
                  0 - 0
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
                      {ball.extraType ||
                      ball.wicketType
                        ? (
                          ball.wicketType ||
                          ball.extraType
                        )
                        : ball.runs}
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