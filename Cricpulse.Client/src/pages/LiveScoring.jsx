import { useEffect, useMemo, useRef, useState } from "react";

import NoBallScoringModal from "../components/Match/NoBallScoringModal";
import ExtraScoringModal from "../components/Match/ExtraScoringModal";
import MatchInfo from "../components/Match/MatchDetails";
import WicketScoringModal from "../components/Match/WicketScoringModal";

import {
  getLiveMatch,
  scoreRuns,
  scoreExtra,
  scoreWicket,
  cancelMatch,
  completeMatch,
  undoLastScore,
  changeBowler,
  scoreExtraRunOut
} from "../services/matchService";

const LiveScoring = ({
  match,
  onBack,
  onFirstInningsCompleted,
  onMatchCompleted
}) => {
  const [liveMatch, setLiveMatch] = useState(null);
  const [showMenu, setShowMenu] = useState(false);
  const [showMatchInfo, setShowMatchInfo] = useState(false);
  const [loading, setLoading] = useState(true);
  const [scoring, setScoring] = useState(false);
  const [error, setError] = useState("");
  const [selectedExtra, setSelectedExtra] = useState(null);
  const [showNoBallModal, setShowNoBallModal] = useState(false);
  const [showWicketModal, setShowWicketModal] = useState(false);
  const [selectedWicketType, setSelectedWicketType] = useState("BOWLED");
  const [completionSeconds, setCompletionSeconds] = useState(0);
  const [completionActionLoading, setCompletionActionLoading] = useState(false);

  // Only one undo is allowed until a new ball is scored.
  const [undoUsed, setUndoUsed] = useState(false);
  const [undoMessage, setUndoMessage] = useState("");

  const [showBowlerModal, setShowBowlerModal] = useState(false);
  const [selectedBowlerId, setSelectedBowlerId] = useState("");

  const completionStartedRef = useRef(false);
  const menuRef = useRef(null);

  const refreshLiveMatch = async () => {
    const result = await getLiveMatch(match.id);
    setLiveMatch(result);
    return result;
  };

  useEffect(() => {
    const load = async () => {
      try {
        setLoading(true);
        setError("");
        await refreshLiveMatch();
      } catch (err) {
        const data = err.response?.data;

        setError(
          typeof data === "string"
            ? data
            : data?.message || "Unable to load live match."
        );
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [match.id]);

  useEffect(() => {
    const closeMenu = (event) => {
      if (
        menuRef.current &&
        !menuRef.current.contains(event.target)
      ) {
        setShowMenu(false);
      }
    };

    document.addEventListener("mousedown", closeMenu);

    return () =>
      document.removeEventListener("mousedown", closeMenu);
  }, []);

  const getPlayer = (matchPlayerId) => {
    if (!matchPlayerId) return null;

    return (match.players || []).find(
      (player) =>
        Number(player.matchPlayerId ?? player.id) ===
        Number(matchPlayerId)
    );
  };

  const getPlayerName = (player) =>
    player?.displayName ||
    player?.playerName ||
    player?.name ||
    player?.mobileNumber ||
    "Unknown Player";

  const striker = useMemo(
    () => getPlayer(liveMatch?.strikerMatchPlayerId),
    [liveMatch?.strikerMatchPlayerId, match.players]
  );

  const nonStriker = useMemo(
    () => getPlayer(liveMatch?.nonStrikerMatchPlayerId),
    [liveMatch?.nonStrikerMatchPlayerId, match.players]
  );

  const bowler = useMemo(
    () => getPlayer(liveMatch?.currentBowlerMatchPlayerId),
    [liveMatch?.currentBowlerMatchPlayerId, match.players]
  );

  const isSecondInnings =
    Number(liveMatch?.inningsNumber) === 2;

  const inningsCompleted =
    liveMatch?.inningsStatus === "Completed";

  const balls = liveMatch?.balls || [];

  const isWaitingForBowler =
    !inningsCompleted &&
    liveMatch?.status === "Live" &&
    Number(liveMatch?.currentBowlerMatchPlayerId ?? 0) === 0;

 const bowlingTeamPlayers = useMemo(() => {
  const players =
    liveMatch?.players?.length > 0
      ? liveMatch.players
      : match?.players || [];

  const bowlingTeam = String(
    liveMatch?.bowlingTeam || ""
  )
    .trim()
    .toLowerCase();

  if (!bowlingTeam) {
    return [];
  }

  const team1Name = String(
    liveMatch?.team1Name ||
      match?.team1Name ||
      ""
  )
    .trim()
    .toLowerCase();

  const team2Name = String(
    liveMatch?.team2Name ||
      match?.team2Name ||
      ""
  )
    .trim()
    .toLowerCase();

  return players.filter((player) => {
    const playerTeam = String(
      player?.team || ""
    )
      .trim()
      .toLowerCase();

    if (!playerTeam) {
      return false;
    }

    // Backend stores MatchPlayer.Team as Team1 / Team2.
    if (
      playerTeam === "team1" &&
      team1Name === bowlingTeam
    ) {
      return true;
    }

    if (
      playerTeam === "team2" &&
      team2Name === bowlingTeam
    ) {
      return true;
    }

    // Also support an API response that already
    // contains the actual team name.
    return playerTeam === bowlingTeam;
  });
}, [
  liveMatch?.players,
  liveMatch?.bowlingTeam,
  liveMatch?.team1Name,
  liveMatch?.team2Name,
  match?.players,
  match?.team1Name,
  match?.team2Name
]);

  /*
   * Current over is based on legal deliveries.
   *
   * Example:
   * 0 legal balls  -> 0.0
   * 1 legal ball   -> 0.1
   * 5 legal balls  -> 0.5
   * 6 legal balls  -> 1.0
   * 7 legal balls  -> 1.1
   */
  const currentOver = useMemo(() => {
    const legalBalls = liveMatch?.legalBalls ?? 0;

    return `${Math.floor(legalBalls / 6)}.${legalBalls % 6}`;
  }, [liveMatch?.legalBalls]);

  const strikerStats = useMemo(() => {
    const id = liveMatch?.strikerMatchPlayerId;

    const playerBalls = balls.filter(
      (ball) =>
        Number(ball.strikerMatchPlayerId) === Number(id)
    );

    return {
      runs: playerBalls.reduce(
        (sum, ball) =>
          sum +
          (ball.runs ?? 0) -
          (ball.extraRuns ?? 0),
        0
      ),

      balls: playerBalls.filter(
        (ball) => ball.isLegalDelivery
      ).length
    };
  }, [
    balls,
    liveMatch?.strikerMatchPlayerId
  ]);

  const nonStrikerStats = useMemo(() => {
    const id = liveMatch?.nonStrikerMatchPlayerId;

    const playerBalls = balls.filter(
      (ball) =>
        Number(ball.strikerMatchPlayerId) === Number(id)
    );

    return {
      runs: playerBalls.reduce(
        (sum, ball) =>
          sum +
          (ball.runs ?? 0) -
          (ball.extraRuns ?? 0),
        0
      ),

      balls: playerBalls.filter(
        (ball) => ball.isLegalDelivery
      ).length
    };
  }, [
    balls,
    liveMatch?.nonStrikerMatchPlayerId
  ]);

  const bowlerStats = useMemo(() => {
    const id = liveMatch?.currentBowlerMatchPlayerId;

    if (!id) {
      return {
        overs: "0.0",
        runsConceded: 0,
        wickets: 0
      };
    }

    const playerBalls = balls.filter(
      (ball) =>
        Number(ball.bowlerMatchPlayerId) === Number(id)
    );

    const legal = playerBalls.filter(
      (ball) => ball.isLegalDelivery
    ).length;

    const conceded = playerBalls.reduce(
      (sum, ball) => {
        const type = (
          ball.extraType || ""
        ).toUpperCase();

        if (
          type === "BYE" ||
          type === "LEG BYE"
        ) {
          return sum;
        }

        return sum + (ball.runs ?? 0);
      },
      0
    );

    const wickets = playerBalls.filter(
      (ball) =>
        [
          "BOWLED",
          "CAUGHT",
          "LBW",
          "STUMPED",
          "HIT WICKET"
        ].includes(
          (ball.wicketType || "").toUpperCase()
        )
    ).length;

    return {
      overs: `${Math.floor(legal / 6)}.${legal % 6}`,
      runsConceded: conceded,
      wickets
    };
  }, [
    balls,
    liveMatch?.currentBowlerMatchPlayerId
  ]);

  /*
   * The current-over display must follow the current
   * legal-ball position, not simply the latest recorded
   * ball. This also works after an over has completed.
   */
  const currentOverBalls = useMemo(() => {
    if (!balls.length) return [];

    const legalBalls = liveMatch?.legalBalls ?? 0;
    const currentOverNumber = Math.floor(
      legalBalls / 6
    );

    return balls.filter(
      (ball) =>
        Number(ball.overNumber) ===
        currentOverNumber
    );
  }, [
    balls,
    liveMatch?.legalBalls
  ]);

  const firstInningsTotalRuns =
    Number(
      match.firstInningsTotalRuns ??
        match.firstInningsScore ??
        (isSecondInnings
          ? undefined
          : liveMatch?.firstInningsTotalRuns)
    );

  const targetRuns =
    isSecondInnings &&
    Number.isFinite(firstInningsTotalRuns)
      ? firstInningsTotalRuns + 1
      : null;

  const chaseStats = useMemo(() => {
    if (
      !isSecondInnings ||
      targetRuns === null
    ) {
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

    const requiredRuns = Math.max(
      targetRuns - currentRuns,
      0
    );

    const requiredRunRate =
      remainingBalls > 0
        ? requiredRuns / (remainingBalls / 6)
        : 0;

    return {
      requiredRuns,
      remainingBalls,
      requiredRunRate
    };
  }, [
    isSecondInnings,
    targetRuns,
    liveMatch?.totalRuns,
    liveMatch?.legalBalls,
    match.overs
  ]);

  const currentRunRate = useMemo(() => {
    const ballsBowled =
      liveMatch?.legalBalls ?? 0;

    if (!ballsBowled) return 0;

    return (
      (liveMatch?.totalRuns ?? 0) /
      (ballsBowled / 6)
    );
  }, [
    liveMatch?.totalRuns,
    liveMatch?.legalBalls
  ]);

  useEffect(() => {
    if (
      !inningsCompleted ||
      isSecondInnings ||
      !onFirstInningsCompleted ||
      !liveMatch
    ) {
      return;
    }

    onFirstInningsCompleted({
      battingTeam: liveMatch.battingTeam,
      totalRuns: liveMatch.totalRuns ?? 0,
      wickets: liveMatch.wickets ?? 0,
      overs: match.overs
    });
  }, [
    inningsCompleted,
    isSecondInnings,
    liveMatch?.battingTeam,
    liveMatch?.totalRuns,
    liveMatch?.wickets,
    match.overs,
    onFirstInningsCompleted
  ]);

  useEffect(() => {
    if (
      liveMatch?.status !==
      "PendingCompletion"
    ) {
      completionStartedRef.current = false;
      setCompletionSeconds(0);
      return;
    }

    if (completionStartedRef.current) return;

    completionStartedRef.current = true;

    const deadline = liveMatch.completionDeadline
      ? new Date(
          liveMatch.completionDeadline
        ).getTime()
      : Date.now() + 10000;

    const tick = () => {
      const remaining = Math.max(
        0,
        Math.ceil(
          (deadline - Date.now()) / 1000
        )
      );

      setCompletionSeconds(remaining);
    };

    tick();

    const timer = setInterval(tick, 250);

    return () => clearInterval(timer);
  }, [
    liveMatch?.status,
    liveMatch?.completionDeadline
  ]);

  const handleScoreRuns = async (runs) => {
    if (
      !liveMatch?.inningsId ||
      scoring ||
      inningsCompleted ||
      isWaitingForBowler
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

      setUndoUsed(false);
      setUndoMessage("");

      await refreshLiveMatch();
    } catch (err) {
      const data = err.response?.data;

      setError(
        typeof data === "string"
          ? data
          : data?.message ||
              "Unable to record runs."
      );
    } finally {
      setScoring(false);
    }
  };

  const handleScoreExtra = async (
    extraType,
    runs,
    batterRuns = 0
  ) => {
    if (
      !liveMatch?.inningsId ||
      scoring ||
      inningsCompleted ||
      isWaitingForBowler
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

      setUndoUsed(false);
      setUndoMessage("");

      await refreshLiveMatch();

      setSelectedExtra(null);
      setShowNoBallModal(false);
    } catch (err) {
      const data = err.response?.data;

      setError(
        typeof data === "string"
          ? data
          : data?.message ||
              "Unable to record extra."
      );
    } finally {
      setScoring(false);
    }
  };

  const handleScoreExtraRunOut = async ({
    extraType,
    totalRuns,
    batterRuns = 0,
    runsCompleted = 0,
    dismissedMatchPlayerId,
    didBattersCross = false,
    newBatterMatchPlayerId = null
  }) => {
    if (
      !liveMatch?.inningsId ||
      scoring ||
      inningsCompleted ||
      isWaitingForBowler
    ) {
      return;
    }

    if (!dismissedMatchPlayerId) {
      setError("Select the dismissed batter.");
      return;
    }

    try {
      setScoring(true);
      setError("");

      await scoreExtraRunOut(
        liveMatch.inningsId,
        extraType,
        totalRuns,
        batterRuns,
        dismissedMatchPlayerId,
        runsCompleted,
        didBattersCross,
        newBatterMatchPlayerId
      );

      setUndoUsed(false);
      setUndoMessage("");

      await refreshLiveMatch();

      setSelectedExtra(null);
      setShowNoBallModal(false);
    } catch (err) {
      const data = err.response?.data;

      setError(
        typeof data === "string"
          ? data
          : data?.message ||
              "Unable to record extra and run out."
      );
    } finally {
      setScoring(false);
    }
  };

  const handleScoreWicket = async (
    wicketData
  ) => {
    if (
      !liveMatch?.inningsId ||
      scoring ||
      inningsCompleted ||
      isWaitingForBowler
    ) {
      return;
    }

    try {
      setScoring(true);
      setError("");

    await scoreWicket({
  inningsId: liveMatch.inningsId,
  wicketType: wicketData.wicketType,
  dismissedMatchPlayerId: wicketData.dismissedMatchPlayerId,
  caughtByMatchPlayerId: wicketData.caughtByMatchPlayerId,
  stumpedByMatchPlayerId: wicketData.stumpedByMatchPlayerId,
  runsCompleted: Number(wicketData.runsCompleted || 0),
  didBattersCross: Boolean(wicketData.didBattersCross),
  newBatterMatchPlayerId: wicketData.newBatterMatchPlayerId
});

// A new ball was successfully scored,
// so the next undo is allowed.
setUndoUsed(false);
setUndoMessage("");

await refreshLiveMatch();

      setShowWicketModal(false);
    } catch (err) {
      const data = err.response?.data;

      setError(
        typeof data === "string"
          ? data
          : data?.message ||
              "Unable to record wicket."
      );
    } finally {
      setScoring(false);
    }
  };

  const handleChangeBowler = async () => {
    if (
      !liveMatch?.inningsId ||
      !selectedBowlerId ||
      scoring ||
      inningsCompleted
    ) {
      return;
    }

    try {
      setScoring(true);
      setError("");

      await changeBowler(
        liveMatch.inningsId,
        Number(selectedBowlerId)
      );

      setSelectedBowlerId("");
      setShowBowlerModal(false);

      await refreshLiveMatch();
    } catch (err) {
      const data = err.response?.data;

      setError(
        typeof data === "string"
          ? data
          : data?.message ||
              "Unable to change bowler."
      );
    } finally {
      setScoring(false);
    }
  };

  const handleUndoLastBall = async () => {
    if (
      undoUsed ||
      !liveMatch?.inningsId ||
      !liveMatch?.balls?.length ||
      scoring ||
      completionActionLoading
    ) {
      return;
    }

    try {
      setCompletionActionLoading(true);
      setError("");
      setUndoMessage("");

      const lastBall = [...liveMatch.balls]
        .sort((a, b) => a.id - b.id)
        .at(-1);

      if (!lastBall?.id) {
        setError("No ball is available to undo.");
        return;
      }

      await undoLastScore(
        liveMatch.inningsId,
        lastBall.id
      );

      // Only one undo is allowed until another ball is scored.
      setUndoUsed(true);
      setUndoMessage(
        "Last ball undone successfully. You can only undo one ball at a time. To undo, you have to score a new ball."
      );

      completionStartedRef.current = false;

      await refreshLiveMatch();

      setCompletionSeconds(0);
    } catch (err) {
      const data = err.response?.data;

      setError(
        typeof data === "string"
          ? data
          : data?.message ||
              "Unable to undo the last ball."
      );
    } finally {
      setCompletionActionLoading(false);
    }
  };

  const handleAcceptCompletion = async () => {
    if (
      !match?.id ||
      completionActionLoading
    ) {
      return;
    }

    try {
      setCompletionActionLoading(true);
      setError("");

      const snapshot = {
        ...liveMatch
      };

      await completeMatch(match.id);

      onMatchCompleted?.(snapshot);
    } catch (err) {
      const data = err.response?.data;

      setError(
        typeof data === "string"
          ? data
          : data?.message ||
              "Unable to complete match."
      );
    } finally {
      setCompletionActionLoading(false);
    }
  };

  const handleCancelMatch = async () => {
    if (!match?.id || scoring) return;

    if (
      !window.confirm(
        "Are you sure you want to cancel this match?"
      )
    ) {
      return;
    }

    try {
      setScoring(true);
      setError("");

      await cancelMatch(match.id);

      setShowMenu(false);
      onBack();
    } catch (err) {
      const data = err.response?.data;

      setError(
        typeof data === "string"
          ? data
          : data?.message ||
              "Unable to cancel match."
      );
    } finally {
      setScoring(false);
    }
  };

  const formatBallNotation = (ball) => {
    if (ball.notation) {
      return ball.notation;
    }

    const wicket =
      (ball.wicketType || "").toUpperCase();

    if (wicket) {
      return "W";
    }

    const extra =
      (ball.extraType || "").toUpperCase();

    const runs = ball.runs ?? 0;

    if (extra === "WIDE") {
      return `WD${runs}`;
    }

    if (extra === "NO BALL") {
      return `NB${runs}`;
    }

    if (extra === "BYE") {
      return `B${runs}`;
    }

    if (extra === "LEG BYE") {
      return `LB${runs}`;
    }

    return String(runs);
  };

  if (loading) {
    return (
      <div className="container py-5 text-center">
        <h4>Loading live match...</h4>
      </div>
    );
  }

  if (!liveMatch) {
    return (
      <div className="container py-5">
        <div className="alert alert-danger">
          {error ||
            "Live match unavailable."}
        </div>

        <button
          className="btn btn-secondary"
          onClick={onBack}
        >
          ← Back to Dashboard
        </button>
      </div>
    );
  }

  if (showMatchInfo) {
    return (
      <MatchInfo
        match={match}
        liveMatch={liveMatch}
        onClose={() =>
          setShowMatchInfo(false)
        }
      />
    );
  }

  return (
    <div className="container-fluid p-0">

      <nav className="navbar navbar-dark bg-dark">
        <div className="container-fluid">

          <span className="navbar-brand mb-0 h1">
            {liveMatch.team1Name} VS{" "}
            {liveMatch.team2Name}
          </span>

          <div
            className="position-relative"
            ref={menuRef}
          >
            <button
              type="button"
              className="btn btn-outline-light"
              onClick={() =>
                setShowMenu(
                  (value) => !value
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
                  minWidth: "220px",
                  zIndex: 1000
                }}
              >

                <button
                  type="button"
                  className="btn btn-link text-dark text-decoration-none w-100 text-start"
                  onClick={() => {
                    setShowMenu(false);
                    setShowMatchInfo(true);
                  }}
                >
                  ℹ️ Match Info
                </button>

                <hr className="my-1" />

                <button
                  type="button"
                  className="btn btn-link text-danger text-decoration-none w-100 text-start"
                  onClick={handleCancelMatch}
                  disabled={scoring}
                >
                  ✕ Cancel Match
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

        {undoMessage && (
          <div className="alert alert-info">
            {undoMessage}
          </div>
        )}

        <div className="card mb-4">
          <div className="card-body">

            <div className="row align-items-center">

              <div className="col-md-5 text-center">

                <h2 className="mb-3">
                  {liveMatch.battingTeam}
                </h2>

                <h1 className="display-4 fw-bold mb-2">
                  {liveMatch.totalRuns ?? 0}/
                  {liveMatch.wickets ?? 0}
                </h1>

                <h5 className="text-muted">
                  {currentOver} overs
                </h5>

              </div>

              <div className="col-md-7 text-center">

                {!isSecondInnings ? (
                  <>
                    <h4>Current Run Rate</h4>

                    <h2>
                      CRR -{" "}
                      {currentRunRate.toFixed(2)}
                    </h2>
                  </>
                ) : (
                  <>
                    <h5 className="text-muted">
                      {targetRuns !== null
                        ? `Required ${chaseStats.requiredRuns} runs in ${chaseStats.remainingBalls} balls`
                        : "Target unavailable"}
                    </h5>

                    <div className="row">

                      <div className="col-6">
                        <small className="text-muted">
                          CRR
                        </small>

                        <h3>
                          {currentRunRate.toFixed(2)}
                        </h3>
                      </div>

                      <div className="col-6">
                        <small className="text-muted">
                          RRR
                        </small>

                        <h3>
                          {chaseStats?.requiredRunRate.toFixed(2)}
                        </h3>
                      </div>

                    </div>
                  </>
                )}

              </div>

            </div>

          </div>
        </div>

        <div className="row g-3 mb-4">

          <div className="col-md-4">
            <div className="card h-100">
              <div className="card-body">

                <h5>Striker</h5>

                <h4>
                  {getPlayerName(striker)}
                </h4>

                <p className="mb-0">
                  {strikerStats.runs} (
                  {strikerStats.balls})
                </p>

              </div>
            </div>
          </div>

          <div className="col-md-4">
            <div className="card h-100">
              <div className="card-body">

                <h5>Non-Striker</h5>

                <h4>
                  {getPlayerName(nonStriker)}
                </h4>

                <p className="mb-0">
                  {nonStrikerStats.runs} (
                  {nonStrikerStats.balls})
                </p>

              </div>
            </div>
          </div>

          <div className="col-md-4">
            <div className="card h-100">
              <div className="card-body">

                <h5>
                  {inningsCompleted
                    ? "Innings Completed"
                    : isWaitingForBowler
                    ? "New Bowler Required"
                    : "Bowler"}
                </h5>

                <h4>
                  {inningsCompleted
                    ? "Waiting for next innings"
                    : isWaitingForBowler
                    ? "Select a bowler"
                    : getPlayerName(bowler)}
                </h4>

                <p className="mb-0">
                  {inningsCompleted
                    ? "First innings finished"
                    : isWaitingForBowler
                    ? "Over completed"
                    : `${bowlerStats.overs} - ${bowlerStats.runsConceded} - ${bowlerStats.wickets}`}
                </p>

              </div>
            </div>
          </div>

        </div>

        {isWaitingForBowler && (
          <div className="card border-warning mb-4">

            <div className="card-body text-center">

              <h4 className="mb-3">
                Over Completed
              </h4>

              <p className="text-muted">
                Select the bowler for the next over.
              </p>

              <div className="row justify-content-center">

                <div className="col-md-6">

                  <select
                    className="form-select mb-3"
                    value={selectedBowlerId}
                    onChange={(event) =>
                      setSelectedBowlerId(
                        event.target.value
                      )
                    }
                    disabled={scoring}
                  >
                    <option value="">
                      Select Bowler
                    </option>

                    {bowlingTeamPlayers.map(
                      (player) => {
                        const playerId =
                          player.matchPlayerId ??
                          player.id;

                        return (
                          <option
                            key={playerId}
                            value={playerId}
                          >
                            {getPlayerName(player)}
                          </option>
                        );
                      }
                    )}
                  </select>

                  <button
                    type="button"
                    className="btn btn-primary"
                    disabled={
                      !selectedBowlerId ||
                      scoring
                    }
                    onClick={
                      handleChangeBowler
                    }
                  >
                    {scoring
                      ? "SELECTING..."
                      : "START NEXT OVER"}
                  </button>

                </div>

              </div>

            </div>

          </div>
        )}

        <div className="card mb-4">

          <div className="card-body">

            <h4>
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
                      {formatBallNotation(ball)}
                    </span>
                  )
                )
              )}

            </div>

          </div>

        </div>

        {liveMatch.status ===
          "PendingCompletion" && (
          <div className="alert alert-success text-center">

            <h4 className="mb-2">
              Match Complete
            </h4>

            <p className="mb-2">
              Confirm completion in{" "}
              <strong>
                {completionSeconds}
              </strong>{" "}
              seconds.
            </p>

            <div className="d-flex justify-content-center gap-2">

              <button
                className="btn btn-warning"
                disabled={
                  undoUsed ||
                  completionActionLoading
                }
                onClick={
                  handleUndoLastBall
                }
              >
                {undoUsed
                  ? "UNDO USED"
                  : "UNDO LAST BALL"}
              </button>

              <button
                className="btn btn-success"
                disabled={
                  completionActionLoading
                }
                onClick={
                  handleAcceptCompletion
                }
              >
                ACCEPT & COMPLETE
              </button>

            </div>

          </div>
        )}

        <div className="row g-3">

          <div className="col-md-4">
            <div className="card">
              <div className="card-body">

                <h4>RUNS</h4>

                <div className="d-grid gap-2">

                  {[0, 1, 2, 3, 4, 6].map(
                    (runs) => (
                      <button
                        key={runs}
                        type="button"
                        className="btn btn-primary"
                        disabled={
                          scoring ||
                          inningsCompleted ||
                          isWaitingForBowler ||
                          liveMatch.status ===
                            "PendingCompletion"
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

          <div className="col-md-4">
            <div className="card">
              <div className="card-body">

                <h4>EXTRAS</h4>

                <div className="d-grid gap-2">

                  {[
                    [
                      "WIDE",
                      () =>
                        setSelectedExtra(
                          "WIDE"
                        )
                    ],
                    [
                      "NO BALL",
                      () =>
                        setShowNoBallModal(
                          true
                        )
                    ],
                    [
                      "BYE",
                      () =>
                        setSelectedExtra(
                          "BYE"
                        )
                    ],
                    [
                      "LEG BYE",
                      () =>
                        setSelectedExtra(
                          "LEG BYE"
                        )
                    ]
                  ].map(
                    ([label, action]) => (
                      <button
                        key={label}
                        type="button"
                        className="btn btn-warning"
                        disabled={
                          scoring ||
                          inningsCompleted ||
                          isWaitingForBowler ||
                          liveMatch.status ===
                            "PendingCompletion"
                        }
                        onClick={action}
                      >
                        {label}
                      </button>
                    )
                  )}

                </div>

              </div>
            </div>
          </div>

          <div className="col-md-4">
            <div className="card">
              <div className="card-body">

                <h4>WICKETS</h4>

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
                        disabled={
                          scoring ||
                          inningsCompleted ||
                          isWaitingForBowler ||
                          liveMatch.status ===
                            "PendingCompletion"
                        }
                        onClick={() => {
                          setSelectedWicketType(wicket);
                          setShowWicketModal(true);
                        }}
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

        {liveMatch.status !==
          "PendingCompletion" &&
          balls.length > 0 && (
            <div className="text-center mt-4">

              <button
                type="button"
                className="btn btn-outline-secondary"
                disabled={
                  undoUsed ||
                  scoring ||
                  completionActionLoading
                }
                onClick={
                  handleUndoLastBall
                }
              >
                {completionActionLoading
                  ? "PROCESSING..."
                  : undoUsed
                  ? "UNDO USED"
                  : "UNDO LAST BALL"}
              </button>

            </div>
          )}

        {selectedExtra && (
          <ExtraScoringModal
            match={match}
            liveMatch={liveMatch}
            extraType={selectedExtra}
            loading={scoring}
            onClose={() =>
              !scoring &&
              setSelectedExtra(null)
            }
            onConfirm={
              handleScoreExtra
            }
            onRunOut={
              handleScoreExtraRunOut
            }
          />
        )}

        {showNoBallModal && (
          <NoBallScoringModal
            match={match}
            liveMatch={liveMatch}
            loading={scoring}
            onClose={() =>
              !scoring &&
              setShowNoBallModal(false)
            }
            onConfirm={
              handleScoreExtra
            }
            onRunOut={
              handleScoreExtraRunOut
            }
          />
        )}

        {showWicketModal && (
          <WicketScoringModal
            match={match}
            liveMatch={liveMatch}
            initialWicketType={selectedWicketType}
            loading={scoring}
            onClose={() =>
              !scoring &&
              setShowWicketModal(false)
            }
            onConfirm={
              handleScoreWicket
            }
          />
        )}

      </div>
    </div>
  );
};

export default LiveScoring;