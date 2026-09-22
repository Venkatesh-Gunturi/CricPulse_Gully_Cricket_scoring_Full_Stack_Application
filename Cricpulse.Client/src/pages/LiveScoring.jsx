import {
  useEffect,
  useMemo,
  useRef,
  useState
} from "react";

import NoBallScoringModal from "../components/Match/NoBallScoringModal";
import ExtraScoringModal from "../components/Match/ExtraScoringModal";

import {
  getLiveMatch,
  scoreRuns,
  scoreExtra,
  cancelMatch
} from "../services/matchService";

const LiveScoring = ({
  match,
  firstInningsTotalRuns,
  onBack,
  onFirstInningsCompleted
}) => {
  const [liveMatch, setLiveMatch] = useState(null);
  const [showMenu, setShowMenu] = useState(false);
  const [loading, setLoading] = useState(true);
  const [scoring, setScoring] = useState(false);
  const [error, setError] = useState("");
  const [selectedExtra, setSelectedExtra] = useState(null);
const [showNoBallModal, setShowNoBallModal] = useState(false);
  // Reference for the three-dot menu.
  // Used to detect clicks outside the menu.
  const menuRef = useRef(null);

  // Purpose:
  // Close the three-dot menu when the user clicks anywhere
  // outside the menu.
  useEffect(() => {
    const handleOutsideClick = (event) => {
      if (
        menuRef.current &&
        !menuRef.current.contains(event.target)
      ) {
        setShowMenu(false);
      }
    };

    document.addEventListener(
      "mousedown",
      handleOutsideClick
    );

    return () => {
      document.removeEventListener(
        "mousedown",
        handleOutsideClick
      );
    };
  }, []);

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
      overs: `${Math.floor(
        legalBalls / 6
      )}.${legalBalls % 6}`,

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
  // Determine whether the current innings is the second innings.
  const isSecondInnings =
    Number(liveMatch?.inningsNumber) === 2;

  // Purpose:
  // Calculate the current run rate from the actual legal balls
  // and runs recorded by the backend.
  const currentRunRate = useMemo(() => {
    const runs =
      liveMatch?.totalRuns ?? 0;

    const legalBalls =
      liveMatch?.legalBalls ?? 0;

    if (legalBalls === 0) {
      return 0;
    }

    return runs / (legalBalls / 6);
  }, [
    liveMatch?.totalRuns,
    liveMatch?.legalBalls
  ]);

  // Purpose:
  // Calculate the second-innings target directly from the persisted
  // first-innings score returned by the live-match API.
  const targetRuns = useMemo(() => {
    if (!isSecondInnings) {
      return null;
    }

    if (
      liveMatch?.firstInningsTotalRuns === null ||
      liveMatch?.firstInningsTotalRuns === undefined
    ) {
      return null;
    }

    return (
      Number(
        liveMatch.firstInningsTotalRuns
      ) + 1
    );
  }, [
    isSecondInnings,
    liveMatch?.firstInningsTotalRuns
  ]);

  // Purpose:
  // Calculate required runs, remaining legal balls, and
  // required run rate for the second innings chase.
  const chaseStats = useMemo(() => {
    if (!isSecondInnings) {
      return null;
    }

    const currentRuns =
      liveMatch?.totalRuns ?? 0;

    const legalBalls =
      liveMatch?.legalBalls ?? 0;

    const totalBalls =
      (match.overs ?? 0) * 6;

    const remainingBalls = Math.max(
      totalBalls - legalBalls,
      0
    );

    const requiredRuns =
      targetRuns !== null
        ? Math.max(
            targetRuns - currentRuns,
            0
          )
        : 0;

    const requiredRunRate =
      remainingBalls > 0
        ? requiredRuns /
          (remainingBalls / 6)
        : 0;

    return {
      requiredRuns,
      remainingBalls,
      requiredRunRate
    };
  }, [
    isSecondInnings,
    liveMatch?.totalRuns,
    liveMatch?.legalBalls,
    match.overs,
    targetRuns
  ]);

  // Purpose:
  // Notify the parent application when the first innings has completed
  // so the application can display the next stage of the match flow.
  useEffect(() => {
    if (
      !inningsCompleted ||
      !onFirstInningsCompleted ||
      !liveMatch
    ) {
      return;
    }

    onFirstInningsCompleted({
      battingTeam:
        liveMatch.battingTeam,

      totalRuns:
        liveMatch.totalRuns ?? 0,

      wickets:
        liveMatch.wickets ?? 0,

      overs:
        match.overs
    });
  }, [
    inningsCompleted,
    liveMatch?.battingTeam,
    liveMatch?.totalRuns,
    liveMatch?.wickets,
    match.overs,
    onFirstInningsCompleted
  ]);

  // Purpose:
  // Record a normal bat run and reload the latest live state
  // so all score information immediately reflects the delivery.
  const handleScoreRuns = async (runs) => {
    if (
      !liveMatch?.inningsId ||
      scoring ||
      inningsCompleted
    ) {
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

  // Purpose:
  // Record an extra delivery and reload the latest live state.
  const handleScoreExtra = async (
    extraType,
    runs,
    batterRuns = 0
  ) => {
    if (
      !liveMatch?.inningsId ||
      scoring ||
      inningsCompleted
    ) {
      return;
    }

    try {
      setScoring(true);
      setError("");

      await scoreExtra(
        liveMatch.inningsId,
        extraType,
        runs,
        batterRuns
      );

      const updatedMatch =
        await getLiveMatch(match.id);

      setLiveMatch(updatedMatch);

      setSelectedExtra(null);
      setShowNoBallModal(false);
    } catch (error) {
      console.error(
        "Failed to score extra:",
        error
      );

      const responseData =
        error.response?.data;

      if (typeof responseData === "string") {
        setError(responseData);
      } else if (responseData?.message) {
        setError(responseData.message);
      } else {
        setError("Unable to record extra.");
      }
    } finally {
      setScoring(false);
    }
  };

  // Purpose:
  // Cancel the current match after confirmation.
  // After successful cancellation, return to the dashboard.
  const handleCancelMatch = async () => {
    if (
      !match?.id ||
      scoring
    ) {
      return;
    }

    const confirmed = window.confirm(
      "Are you sure you want to cancel this match?"
    );

    if (!confirmed) {
      return;
    }

    try {
      setScoring(true);
      setError("");

      await cancelMatch(match.id);

      setShowMenu(false);

      onBack();
    } catch (error) {
      console.error(
        "Failed to cancel match:",
        error
      );

      const responseData =
        error.response?.data;

      if (typeof responseData === "string") {
        setError(responseData);
      } else if (responseData?.message) {
        setError(responseData.message);
      } else {
        setError("Unable to cancel match.");
      }
    } finally {
      setScoring(false);
    }
  };

  if (loading) {
    return (
      <div className="container py-5 text-center">
        <h4>
          Loading live match...
        </h4>
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

          {/* Three-dot menu */}
          <div
            className="position-relative"
            ref={menuRef}
          >

            <button
              type="button"
              className="btn btn-outline-light"
              onClick={() =>
                setShowMenu(
                  (current) => !current
                )
              }
              disabled={scoring}
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
                  onClick={handleCancelMatch}
                  disabled={scoring}
                >
                  Cancel Match
                </button>

              </div>
            )}

          </div>

        </div>
      </nav>

      <div className="container py-4">

        {/* Error */}
        {error && (
          <div className="alert alert-danger">
            {error}
          </div>
        )}

        {/* Score Header */}
        <div className="card mb-4">

          <div className="card-body">

            <div className="row align-items-center">

              {/* Score Box */}
              <div className="col-md-5 text-center">

                <h2 className="mb-3">
                  {liveMatch.battingTeam}
                </h2>

                <h1 className="display-4 fw-bold mb-2">
                  {liveMatch.totalRuns ?? 0}
                  {" / "}
                  {liveMatch.wickets ?? 0}
                </h1>

                <h5 className="text-muted">
                  {currentOver} overs
                </h5>

              </div>

              {/* First innings statistics */}
              {!isSecondInnings && (
                <div className="col-md-7 text-center">

                  <h4 className="mb-3">
                    Current Run Rate
                  </h4>

                  <h2>
                    CRR -{" "}
                    {currentRunRate.toFixed(2)}
                  </h2>

                </div>
              )}

              {/* Second innings chase statistics */}
              {isSecondInnings && (
                <div className="col-md-7 text-center">

                  <h4 className="mb-3">
                    {targetRuns !== null
                      ? `Required ${chaseStats.requiredRuns} runs to win in ${chaseStats.remainingBalls} balls`
                      : "Target unavailable"}
                  </h4>

                  <div className="row">

                    <div className="col-6">

                      <h5 className="text-muted">
                        CRR
                      </h5>

                      <h3>
                        {currentRunRate.toFixed(2)}
                      </h3>

                    </div>

                    <div className="col-6">

                      <h5 className="text-muted">
                        RRR
                      </h5>

                      <h3>
                        {chaseStats.requiredRunRate.toFixed(
                          2
                        )}
                      </h3>

                    </div>

                  </div>

                </div>
              )}

            </div>

            {inningsCompleted && (
              <div className="alert alert-success mt-4 mb-0 text-center">

                <strong>
                  INNINGS COMPLETED
                </strong>

              </div>
            )}

          </div>
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

                {/* Same alignment as Runs and Wickets */}
                <div className="d-grid gap-2">

                  <button
                    type="button"
                    className="btn btn-warning"
                    disabled={
                      scoring ||
                      inningsCompleted
                    }
                    onClick={() =>
                      setSelectedExtra("WIDE")
                    }
                  >
                    WIDE
                  </button>

                 <button
                    type="button"
                    className="btn btn-warning"
                    disabled={scoring || inningsCompleted}
                    onClick={() => setShowNoBallModal(true)}
                  >
                    NO BALL
                  </button>

                  <button
                    type="button"
                    className="btn btn-warning"
                    disabled={
                      scoring ||
                      inningsCompleted
                    }
                    onClick={() =>
                      setSelectedExtra("BYE")
                    }
                  >
                    BYE
                  </button>

                  <button
                    type="button"
                    className="btn btn-warning"
                    disabled={
                      scoring ||
                      inningsCompleted
                    }
                    onClick={() =>
                      setSelectedExtra("LEG BYE")
                    }
                  >
                    LEG BYE
                  </button>

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
                  ].map(
                    (wicket) => (
                      <button
                        key={wicket}
                        type="button"
                        className="btn btn-danger"
                        disabled
                      >
                        {wicket}
                      </button>
                    )
                  )}

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

        {/* Extra Scoring Modal */}
        {selectedExtra && (
          <ExtraScoringModal
            extraType={selectedExtra}
            loading={scoring}
            onClose={() => {
              if (!scoring) {
                setSelectedExtra(null);
              }
            }}
            onConfirm={handleScoreExtra}
          />
        )}

        {showNoBallModal && (
          <NoBallScoringModal
            loading={scoring}
            onClose={() => {
              if (!scoring) {
                setShowNoBallModal(false);
              }
            }}
            onConfirm={handleScoreExtra}
          />
        )}
      </div>
    </div>
  );
};

export default LiveScoring;