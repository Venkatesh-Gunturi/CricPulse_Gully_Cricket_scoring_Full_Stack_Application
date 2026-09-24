import { useEffect, useMemo, useState } from "react";

const ExtraScoringModal = ({
  extraType,
  match,
  liveMatch,
  onClose,
  onConfirm,
  onRunOut,
  loading
}) => {
  const [selectedRuns, setSelectedRuns] = useState(null);

  const [showRunOutForm, setShowRunOutForm] = useState(false);
  const [dismissedPlayerId, setDismissedPlayerId] = useState("");
  const [runsCompleted, setRunsCompleted] = useState(0);
  const [didBattersCross, setDidBattersCross] = useState(false);
  const [newBatterMatchPlayerId, setNewBatterMatchPlayerId] =
    useState("");

  useEffect(() => {
    setSelectedRuns(null);
    setShowRunOutForm(false);
    setDismissedPlayerId("");
    setRunsCompleted(0);
    setDidBattersCross(false);
    setNewBatterMatchPlayerId("");
  }, [extraType]);

  if (!extraType) {
    return null;
  }

  const isWide = extraType === "WIDE";
  const isBye =
    extraType === "BYE" ||
    extraType === "LEG BYE";

  const battingTeamPlayers = useMemo(() => {
    const battingTeam = String(
      liveMatch?.battingTeam || ""
    )
      .trim()
      .toLowerCase();

    const team1 = String(
      liveMatch?.team1Name ||
        match?.team1Name ||
        ""
    )
      .trim()
      .toLowerCase();

    const team2 = String(
      liveMatch?.team2Name ||
        match?.team2Name ||
        ""
    )
      .trim()
      .toLowerCase();

    const players =
      liveMatch?.players ||
      match?.players ||
      [];

    if (!battingTeam) {
      return players;
    }

    return players.filter((player) => {
      const team = String(
        player?.team || ""
      )
        .trim()
        .toLowerCase();

      return (
        team === battingTeam ||
        (team === "team1" &&
          team1 === battingTeam) ||
        (team === "team2" &&
          team2 === battingTeam)
      );
    });
  }, [
    liveMatch?.players,
    liveMatch?.battingTeam,
    liveMatch?.team1Name,
    liveMatch?.team2Name,
    match?.players,
    match?.team1Name,
    match?.team2Name
  ]);

  const getPlayerId = (player) =>
    Number(
      player?.matchPlayerId ??
        player?.matchPlayerID ??
        player?.id ??
        player?.playerId ??
        0
    );

  const activeBattingPlayers =
    battingTeamPlayers.filter(
      (player) => !player?.isDismissed
    );

  const allPlayers =
    liveMatch?.players ||
    match?.players ||
    [];

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

  const currentBatters = allPlayers.filter(
    (player) => {
      const playerId = getPlayerId(player);

      return (
        playerId === strikerId ||
        playerId === nonStrikerId
      );
    }
  );

  const getPlayerName = (player) =>
    player?.displayName ||
    player?.playerName ||
    player?.name ||
    player?.mobileNumber ||
    "Unknown Player";

  const getRunLabel = () => {
    if (isWide) {
      return "Wide runs";
    }

    if (extraType === "BYE") {
      return "Bye runs";
    }

    return "Leg-bye runs";
  };

  /*
   * Normal extra scoring.
   *
   * 0 is valid for BYE and LEG BYE.
   *
   * Wide still starts at 1 because a wide itself
   * always contributes at least one extra run.
   */
  const calculateTotalRuns = () => {
    if (selectedRuns === null) {
      return 0;
    }

    if (isWide) {
      return Number(selectedRuns);
    }

    return Number(selectedRuns);
  };

  const getWideCompletedRuns = () => {
    if (!isWide || selectedRuns === null) {
      return 0;
    }

    return Math.max(
      Number(selectedRuns) - 1,
      0
    );
  };

  const handleConfirm = () => {
    if (selectedRuns === null) {
      return;
    }

    const totalRuns =
      calculateTotalRuns();

    onConfirm(
      extraType,
      totalRuns,
      0
    );
  };

  /*
   * Run Out is completely independent of the
   * normal 0–6 scoring buttons.
   */
  const handleOpenRunOut = () => {
    if (
      typeof onRunOut !== "function"
    ) {
      return;
    }

    setShowRunOutForm(true);
    setDismissedPlayerId("");
    setRunsCompleted(0);
    setDidBattersCross(false);
    setNewBatterMatchPlayerId("");
  };

  const handleRunOut = () => {
    if (
      typeof onRunOut !== "function" ||
      !dismissedPlayerId
    ) {
      return;
    }

    const completedRuns =
      Number(runsCompleted) || 0;

    /*
     * Wide has a mandatory 1-run penalty.
     *
     * Bye / Leg Bye run-out:
     * completed runs are entered directly
     * in the Run Out form.
     */
    const totalRuns = isWide
      ? 1 + completedRuns
      : completedRuns;

    onRunOut({
      extraType,
      totalRuns,
      batterRuns: 0,
      runsCompleted: completedRuns,
      dismissedMatchPlayerId:
        Number(dismissedPlayerId),
      didBattersCross,
      newBatterMatchPlayerId:
        newBatterMatchPlayerId
          ? Number(
              newBatterMatchPlayerId
            )
          : null
    });
  };

  /*
   * Normal scoring buttons.
   *
   * BYE / LEG BYE:
   * 0–6
   *
   * WIDE:
   * 1–6
   */
  const runOptions = isWide
    ? [1, 2, 3, 4, 5, 6]
    : [0, 1, 2, 3, 4, 5, 6];

  /*
   * These buttons belong ONLY to the Run Out form.
   * They are not the normal scoring buttons.
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
              {showRunOutForm
                ? `${extraType} + RUN OUT`
                : extraType}
            </h5>

            <button
              type="button"
              className="btn-close"
              onClick={onClose}
              disabled={loading}
            />
          </div>

          <div className="modal-body">

            {!showRunOutForm ? (
              <>
                <div className="text-center mb-3">
                  <h6 className="text-muted mb-1">
                    {getRunLabel()}
                  </h6>

                  <p className="small text-muted mb-0">
                    {isWide
                      ? "Select the total wide runs."
                      : "Select the total runs scored from this extra."}
                  </p>
                </div>

                <div className="d-flex flex-wrap justify-content-center gap-2">
                  {runOptions.map((runs) => (
                    <button
                      key={runs}
                      type="button"
                      className={`btn ${
                        selectedRuns === runs
                          ? "btn-primary"
                          : "btn-outline-primary"
                      }`}
                      style={{
                        minWidth: "60px"
                      }}
                      onClick={() =>
                        setSelectedRuns(runs)
                      }
                      disabled={loading}
                    >
                      {runs}
                    </button>
                  ))}
                </div>

                {selectedRuns !== null && (
                  <div className="alert alert-info text-center mt-4 mb-0">
                    {isWide ? (
                      <div className="fs-5">
                        Total:{" "}
                        <strong>
                          {calculateTotalRuns()}
                        </strong>{" "}
                        wide runs
                      </div>
                    ) : (
                      <div className="fs-5">
                        Total:{" "}
                        <strong>
                          {calculateTotalRuns()}
                        </strong>{" "}
                        runs
                      </div>
                    )}
                  </div>
                )}

                <div className="mt-4">
                  <button
                    type="button"
                    className="btn btn-danger w-100"
                    disabled={
                      loading ||
                      typeof onRunOut !==
                        "function"
                    }
                    onClick={
                      handleOpenRunOut
                    }
                  >
                    {extraType} + RUN OUT
                  </button>
                </div>
              </>
            ) : (
              <>
                <div className="text-center mb-3">
                  <h6 className="text-muted mb-1">
                    {extraType} + RUN OUT
                  </h6>

                  <p className="small text-muted mb-0">
                    Enter the runs completed
                    before the run out.
                  </p>
                </div>

                <div className="mb-3">
                  <label className="form-label fw-semibold">
                    Who is out?
                  </label>

                  <select
                    className="form-select"
                    value={
                      dismissedPlayerId
                    }
                    onChange={(event) => {
                      setDismissedPlayerId(
                        event.target.value
                      );

                      setNewBatterMatchPlayerId(
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
                          getPlayerId(player);

                        return (
                          <option
                            key={playerId}
                            value={playerId}
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
                            runsCompleted ===
                            runs
                              ? "btn-primary"
                              : "btn-outline-primary"
                          }`}
                          style={{
                            minWidth: "55px"
                          }}
                          onClick={() =>
                            setRunsCompleted(
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
                    id="extraRunOutCrossed"
                    type="checkbox"
                    className="form-check-input"
                    checked={
                      didBattersCross
                    }
                    onChange={(event) =>
                      setDidBattersCross(
                        event.target.checked
                      )
                    }
                    disabled={loading}
                  />

                  <label
                    htmlFor="extraRunOutCrossed"
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
                      newBatterMatchPlayerId
                    }
                    onChange={(event) =>
                      setNewBatterMatchPlayerId(
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
                          getPlayerId(player);

                        return (
                          playerId !==
                            Number(
                              dismissedPlayerId
                            ) &&
                          playerId !==
                            strikerId &&
                          playerId !==
                            nonStrikerId
                        );
                      })
                      .map((player) => {
                        const playerId =
                          getPlayerId(player);

                        return (
                          <option
                            key={playerId}
                            value={playerId}
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
                    Extra penalty:{" "}
                    <strong>
                      {isWide ? 1 : 0}
                    </strong>
                  </div>

                  <div>
                    Completed runs:{" "}
                    <strong>
                      {runsCompleted}
                    </strong>
                  </div>

                  <div className="mt-1">
                    Total runs:{" "}
                    <strong>
                      {isWide
                        ? 1 +
                          Number(
                            runsCompleted
                          )
                        : Number(
                            runsCompleted
                          )}
                    </strong>
                  </div>
                </div>

                <div className="text-center mt-3">
                  <button
                    type="button"
                    className="btn btn-link btn-sm"
                    onClick={() =>
                      setShowRunOutForm(
                        false
                      )
                    }
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

            {showRunOutForm ? (
              <button
                type="button"
                className="btn btn-danger"
                onClick={handleRunOut}
                disabled={
                  loading ||
                  !dismissedPlayerId
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
                onClick={handleConfirm}
                disabled={
                  loading ||
                  selectedRuns === null
                }
              >
                {loading
                  ? "Scoring..."
                  : `Add ${calculateTotalRuns()} Runs`}
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default ExtraScoringModal;