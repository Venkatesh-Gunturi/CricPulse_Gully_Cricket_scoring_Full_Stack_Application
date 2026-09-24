import { useEffect, useMemo, useState } from "react";

const NoBallScoringModal = ({
  match,
  liveMatch,
  onClose,
  onConfirm,
  onRunOut,
  loading
}) => {
  const [scoringType, setScoringType] = useState(null);
  const [selectedRuns, setSelectedRuns] = useState(null);

  const [runOutDismissedPlayerId, setRunOutDismissedPlayerId] =
    useState("");
  const [runOutRunsCompleted, setRunOutRunsCompleted] = useState(0);
  const [runOutDidBattersCross, setRunOutDidBattersCross] =
    useState(false);
  const [runOutNewBatterId, setRunOutNewBatterId] = useState("");

  useEffect(() => {
    setScoringType(null);
    setSelectedRuns(null);
    setRunOutDismissedPlayerId("");
    setRunOutRunsCompleted(0);
    setRunOutDidBattersCross(false);
    setRunOutNewBatterId("");
  }, []);

  const allPlayers =
    liveMatch?.players ||
    match?.players ||
    [];

  const battingTeam = String(
    liveMatch?.battingTeam ||
      liveMatch?.battingTeamName ||
      match?.battingTeam ||
      match?.battingTeamName ||
      ""
  )
    .trim()
    .toLowerCase();

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

  const activeBattingPlayers = useMemo(() => {
    if (!allPlayers.length) {
      return [];
    }

    if (!battingTeam) {
      return allPlayers.filter(
        (player) => !player?.isDismissed
      );
    }

    return allPlayers.filter((player) => {
      const team = String(
        player?.team || ""
      )
        .trim()
        .toLowerCase();

      return (
        !player?.isDismissed &&
        (
          team === battingTeam ||
          (team === "team1" &&
            team1Name === battingTeam) ||
          (team === "team2" &&
            team2Name === battingTeam)
        )
      );
    });
  }, [
    allPlayers,
    battingTeam,
    team1Name,
    team2Name
  ]);

  const getPlayerId = (player) =>
    Number(
      player?.matchPlayerId ??
        player?.matchPlayerID ??
        player?.id ??
        player?.playerId ??
        0
    );

  const strikerId = Number(
    liveMatch?.strikerMatchPlayerId ??
      liveMatch?.strikerId ??
      match?.strikerMatchPlayerId ??
      match?.strikerId ??
      0
  );

  const nonStrikerId = Number(
    liveMatch?.nonStrikerMatchPlayerId ??
      liveMatch?.nonStrikerId ??
      match?.nonStrikerMatchPlayerId ??
      match?.nonStrikerId ??
      0
  );

  const currentBatters = useMemo(() => {
    if (!strikerId && !nonStrikerId) {
      return [];
    }

    return allPlayers.filter((player) => {
      const playerId =
        getPlayerId(player);

      return (
        playerId === strikerId ||
        playerId === nonStrikerId
      );
    });
  }, [
    allPlayers,
    strikerId,
    nonStrikerId
  ]);

  const getPlayerName = (player) =>
    player?.displayName ||
    player?.playerName ||
    player?.name ||
    player?.mobileNumber ||
    "Unknown Player";

  const handleScoringType = (type) => {
    setScoringType(type);
    setSelectedRuns(null);

    if (type !== "RUN_OUT") {
      setRunOutDismissedPlayerId("");
      setRunOutRunsCompleted(0);
      setRunOutDidBattersCross(false);
      setRunOutNewBatterId("");
    }
  };

  const handleConfirm = () => {
    if (
      !scoringType ||
      selectedRuns === null
    ) {
      return;
    }

    let totalRuns = 0;
    let batterRuns = 0;

    if (scoringType === "HIT_AND_RAN") {
      batterRuns =
        Number(selectedRuns);

      totalRuns =
        1 + batterRuns;
    }

    if (scoringType === "BYE_RUNS") {
      batterRuns = 0;

      totalRuns =
        1 +
        Number(selectedRuns);
    }

    onConfirm(
      "NO BALL",
      totalRuns,
      batterRuns
    );
  };

  /*
   * No Ball + Run Out is completely
   * independent of the normal scoring buttons.
   */
  const handleRunOut = () => {
    if (
      !runOutDismissedPlayerId ||
      !liveMatch?.inningsId ||
      typeof onRunOut !== "function"
    ) {
      return;
    }

    const completedRuns =
      Number(runOutRunsCompleted) || 0;

    const totalRuns =
      1 + completedRuns;

    onRunOut({
      extraType: "NO BALL",
      totalRuns,
      batterRuns: 0,
      runsCompleted:
        completedRuns,
      dismissedMatchPlayerId:
        Number(
          runOutDismissedPlayerId
        ),
      didBattersCross:
        runOutDidBattersCross,
      newBatterMatchPlayerId:
        runOutNewBatterId
          ? Number(
              runOutNewBatterId
            )
          : null
    });
  };

  /*
   * Normal No Ball scoring:
   * 0–6.
   */
  const runOptions = [
    0,
    1,
    2,
    3,
    4,
    5,
    6
  ];

  /*
   * Separate options used ONLY
   * inside the Run Out screen.
   */
  const completedRunOptions = [
    0,
    1,
    2,
    3,
    4,
    5,
    6
  ];

  const isRunOutReady =
    Boolean(
      runOutDismissedPlayerId
    ) &&
    Number(
      runOutRunsCompleted
    ) >= 0;

  const getResultText = () => {
    if (
      selectedRuns === null
    ) {
      return null;
    }

    if (
      scoringType ===
      "HIT_AND_RAN"
    ) {
      return (
        <>
          Total:{" "}
          <strong>1</strong>{" "}
          extra +{" "}
          <strong>
            {selectedRuns}
          </strong>{" "}
          batter runs ={" "}
          <strong>
            {1 +
              Number(
                selectedRuns
              )}
          </strong>{" "}
          runs
        </>
      );
    }

    if (
      scoringType ===
      "BYE_RUNS"
    ) {
      return (
        <>
          Total:{" "}
          <strong>1</strong>{" "}
          extra +{" "}
          <strong>
            {selectedRuns}
          </strong>{" "}
          bye runs ={" "}
          <strong>
            {1 +
              Number(
                selectedRuns
              )}
          </strong>{" "}
          runs
        </>
      );
    }

    return null;
  };

  return (
    <div
      className="modal fade show d-block"
      tabIndex="-1"
      role="dialog"
      style={{
        backgroundColor:
          "rgba(0, 0, 0, 0.65)"
      }}
    >
      <div
        className="modal-dialog modal-dialog-centered"
        role="document"
      >
        <div className="modal-content">

          <div className="modal-header">
            <h5 className="modal-title">
              NO BALL
            </h5>

            <button
              type="button"
              className="btn-close"
              onClick={onClose}
              disabled={loading}
            />
          </div>

          <div className="modal-body">

            {!scoringType && (
              <>
                <div className="text-center mb-3">
                  <h6 className="text-muted mb-1">
                    How did the runs occur?
                  </h6>
                </div>

                <div className="d-flex gap-2">

                  <button
                    type="button"
                    className="btn btn-danger flex-fill"
                    disabled={loading}
                    onClick={() =>
                      handleScoringType(
                        "RUN_OUT"
                      )
                    }
                  >
                    RUN OUT
                  </button>

                  <button
                    type="button"
                    className="btn btn-outline-primary flex-fill"
                    onClick={() =>
                      handleScoringType(
                        "HIT_AND_RAN"
                      )
                    }
                    disabled={loading}
                  >
                    HIT & RAN
                  </button>

                  <button
                    type="button"
                    className="btn btn-outline-primary flex-fill"
                    onClick={() =>
                      handleScoringType(
                        "BYE_RUNS"
                      )
                    }
                    disabled={loading}
                  >
                    BYE RUNS
                  </button>

                </div>
              </>
            )}

            {scoringType ===
              "RUN_OUT" && (
              <>
                <div className="text-center mb-3">
                  <h6 className="text-muted mb-1">
                    NO BALL + RUN OUT
                  </h6>

                  <p className="small text-muted mb-0">
                    Enter the runs completed
                    before the run out.
                    The no-ball penalty is
                    automatically included.
                  </p>
                </div>

                <div className="mb-3">
                  <label className="form-label fw-semibold">
                    Who is out?
                  </label>

                  <select
                    className="form-select"
                    value={
                      runOutDismissedPlayerId
                    }
                    onChange={(event) => {
                      setRunOutDismissedPlayerId(
                        event.target.value
                      );

                      setRunOutNewBatterId(
                        ""
                      );
                    }}
                    disabled={loading}
                  >
                    <option value="">
                      Select dismissed batter
                    </option>

                    {currentBatters.map(
                      (player) => {
                        const playerId =
                          getPlayerId(
                            player
                          );

                        return (
                          <option
                            key={playerId}
                            value={
                              playerId
                            }
                          >
                            {getPlayerName(
                              player
                            )}
                            {playerId ===
                            strikerId
                              ? " (Striker)"
                              : playerId ===
                                  nonStrikerId
                                ? " (Non-Striker)"
                                : ""}
                          </option>
                        );
                      }
                    )}
                  </select>
                </div>

                <div className="mb-3">
                  <label className="form-label fw-semibold">
                    Runs completed before run out
                  </label>

                  <div className="d-flex flex-wrap gap-2">
                    {completedRunOptions.map(
                      (runs) => (
                        <button
                          key={runs}
                          type="button"
                          className={`btn ${
                            Number(
                              runOutRunsCompleted
                            ) === runs
                              ? "btn-primary"
                              : "btn-outline-primary"
                          }`}
                          style={{
                            minWidth:
                              "55px"
                          }}
                          onClick={() =>
                            setRunOutRunsCompleted(
                              runs
                            )
                          }
                          disabled={loading}
                        >
                          {runs}
                        </button>
                      )
                    )}
                  </div>
                </div>

                <div className="form-check mb-3">
                  <input
                    id="noBallRunOutCrossed"
                    type="checkbox"
                    className="form-check-input"
                    checked={
                      runOutDidBattersCross
                    }
                    onChange={(event) =>
                      setRunOutDidBattersCross(
                        event.target.checked
                      )
                    }
                    disabled={loading}
                  />

                  <label
                    htmlFor="noBallRunOutCrossed"
                    className="form-check-label"
                  >
                    Did the batters cross?
                  </label>
                </div>

                <div className="mb-3">
                  <label className="form-label fw-semibold">
                    Incoming batter
                  </label>

                  <select
                    className="form-select"
                    value={
                      runOutNewBatterId
                    }
                    onChange={(event) =>
                      setRunOutNewBatterId(
                        event.target.value
                      )
                    }
                    disabled={loading}
                  >
                    <option value="">
                      Select incoming batter
                    </option>

                    {activeBattingPlayers
                      .filter((player) => {
                        const playerId =
                          getPlayerId(
                            player
                          );

                        return (
                          playerId !==
                            Number(
                              runOutDismissedPlayerId
                            ) &&
                          playerId !==
                            strikerId &&
                          playerId !==
                            nonStrikerId
                        );
                      })
                      .map((player) => {
                        const playerId =
                          getPlayerId(
                            player
                          );

                        return (
                          <option
                            key={playerId}
                            value={
                              playerId
                            }
                          >
                            {getPlayerName(
                              player
                            )}
                          </option>
                        );
                      })}
                  </select>
                </div>

                <div className="alert alert-warning mb-0">
                  <div>
                    No-ball penalty:{" "}
                    <strong>
                      1
                    </strong>
                  </div>

                  <div>
                    Completed runs:{" "}
                    <strong>
                      {
                        runOutRunsCompleted
                      }
                    </strong>
                  </div>

                  <div className="mt-1">
                    Total:{" "}
                    <strong>
                      {1 +
                        Number(
                          runOutRunsCompleted
                        )}
                    </strong>
                  </div>
                </div>

                <div className="text-center mt-3">
                  <button
                    type="button"
                    className="btn btn-link btn-sm"
                    onClick={() => {
                      setScoringType(
                        null
                      );
                      setSelectedRuns(
                        null
                      );
                      setRunOutDismissedPlayerId(
                        ""
                      );
                      setRunOutRunsCompleted(
                        0
                      );
                      setRunOutDidBattersCross(
                        false
                      );
                      setRunOutNewBatterId(
                        ""
                      );
                    }}
                    disabled={loading}
                  >
                    ← Back
                  </button>
                </div>
              </>
            )}

            {scoringType &&
              scoringType !==
                "RUN_OUT" && (
              <>
                <div className="text-center mb-3">
                  <h6 className="text-muted mb-1">
                    {scoringType ===
                    "HIT_AND_RAN"
                      ? "Batter runs"
                      : "Bye runs"}
                  </h6>

                  <p className="small text-muted mb-0">
                    Select the runs scored.
                  </p>
                </div>

                <div className="d-flex flex-wrap justify-content-center gap-2">
                  {runOptions.map(
                    (runs) => (
                      <button
                        key={runs}
                        type="button"
                        className={`btn ${
                          selectedRuns ===
                          runs
                            ? "btn-primary"
                            : "btn-outline-primary"
                        }`}
                        style={{
                          minWidth:
                            "60px"
                        }}
                        onClick={() =>
                          setSelectedRuns(
                            runs
                          )
                        }
                        disabled={loading}
                      >
                        {runs}
                      </button>
                    )
                  )}
                </div>

                {selectedRuns !==
                  null && (
                  <div className="alert alert-info text-center mt-4 mb-0">
                    <div className="fs-5">
                      {getResultText()}
                    </div>
                  </div>
                )}

                <div className="text-center mt-3">
                  <button
                    type="button"
                    className="btn btn-link btn-sm"
                    onClick={() => {
                      setScoringType(
                        null
                      );
                      setSelectedRuns(
                        null
                      );
                    }}
                    disabled={loading}
                  >
                    ← Back
                  </button>
                </div>
              </>
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

            {scoringType ===
            "RUN_OUT" ? (
              <button
                type="button"
                className="btn btn-danger"
                onClick={
                  handleRunOut
                }
                disabled={
                  loading ||
                  !isRunOutReady
                }
              >
                {loading
                  ? "Scoring..."
                  : "CONFIRM RUN OUT"}
              </button>
            ) : (
              <button
                type="button"
                className="btn btn-primary"
                onClick={
                  handleConfirm
                }
                disabled={
                  loading ||
                  !scoringType ||
                  selectedRuns ===
                    null
                }
              >
                {loading
                  ? "Scoring..."
                  : `Add ${
                      selectedRuns ===
                      null
                        ? ""
                        : 1 +
                          Number(
                            selectedRuns
                          )
                    } Runs`}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default NoBallScoringModal;