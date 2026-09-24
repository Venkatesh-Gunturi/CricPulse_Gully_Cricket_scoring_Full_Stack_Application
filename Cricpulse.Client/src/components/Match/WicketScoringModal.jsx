import { useEffect, useMemo, useState } from "react";

const WICKET_TYPES = [
  "BOWLED",
  "CAUGHT",
  "RUN OUT",
  "LBW",
  "STUMPED",
  "HIT WICKET"
];

const WicketScoringModal = ({
  match,
  liveMatch,
  initialWicketType = "BOWLED",
  loading,
  onClose,
  onConfirm
}) => {
  const [wicketType, setWicketType] = useState(initialWicketType);
  const [dismissedPlayerId, setDismissedPlayerId] = useState("");
  const [caughtBy, setCaughtBy] = useState("");
  const [stumpedBy, setStumpedBy] = useState("");
  const [runsCompleted, setRunsCompleted] = useState(0);
  const [didBattersCross, setDidBattersCross] = useState(false);
  const [newBatterId, setNewBatterId] = useState("");

  /*
   * Prefer live-match players because liveMatch is the
   * authoritative scoring state.
   */
  const players = liveMatch?.players || match?.players || [];

  const battingTeam = liveMatch?.battingTeam;
  const bowlingTeam = liveMatch?.bowlingTeam;

  const playerId = (player) =>
    Number(player?.matchPlayerId ?? player?.id ?? player?.playerId);

  const playerName = (player) =>
    player?.displayName ||
    player?.playerName ||
    player?.name ||
    player?.mobileNumber ||
    "Unknown Player";

  const getTeam = (player) => {
    return player?.team || player?.Team || player?.teamName || player?.TeamName || "";
  };

  const belongsToTeam = (player, expectedTeam) => {
    const actual = String(getTeam(player) || "").trim().toLowerCase();
    const expected = String(expectedTeam || "").trim().toLowerCase();

    if (!actual || !expected) {
      return false;
    }

    if (actual === expected) {
      return true;
    }

    const team1 = String(
      match?.team1Name ?? match?.Team1Name ?? ""
    ).trim().toLowerCase();
    const team2 = String(
      match?.team2Name ?? match?.Team2Name ?? ""
    ).trim().toLowerCase();

    if (actual === "team1" || actual === "team 1") {
      return expected === team1 || expected === "team1" || expected === "team 1";
    }

    if (actual === "team2" || actual === "team 2") {
      return expected === team2 || expected === "team2" || expected === "team 2";
    }

    return false;
  };

  const battingPlayers = useMemo(
    () =>
      players.filter(
        (player) => belongsToTeam(player, battingTeam)
      ),
    [
      players,
      battingTeam,
      match?.team1Name,
      match?.team2Name
    ]
  );

  const fieldingPlayers = useMemo(
    () =>
      players.filter(
        (player) => belongsToTeam(player, bowlingTeam)
      ),
    [
      players,
      bowlingTeam,
      match?.team1Name,
      match?.team2Name
    ]
  );

  const strikerId = Number(
    liveMatch?.strikerMatchPlayerId || 0
  );

  const nonStrikerId = Number(
    liveMatch?.nonStrikerMatchPlayerId || 0
  );

  const activeBatters = [
    strikerId,
    nonStrikerId
  ].filter((id) => id > 0);

  /*
   * A player who has already been dismissed in this innings
   * must never be available as an incoming batter again.
   */
  const dismissedPlayerIds = useMemo(() => {
    const ids = new Set();

    (liveMatch?.balls || []).forEach((ball) => {
      const id = Number(
        ball?.dismissedMatchPlayerId ??
        ball?.DismissedMatchPlayerId ??
        0
      );

      if (id > 0) {
        ids.add(id);
      }
    });

    return ids;
  }, [liveMatch?.balls]);

  /*
   * Only players from the batting team who have not already
   * batted and have not already been dismissed can become
   * the incoming batter.
   */
  const eligibleIncomingBatters = battingPlayers.filter(
    (player) => {
      const id = playerId(player);

      return (
        id > 0 &&
        !activeBatters.includes(id) &&
        !dismissedPlayerIds.has(id) &&
        id !== Number(dismissedPlayerId)
      );
    }
  );

  const requiresFielder =
    wicketType === "CAUGHT" ||
    wicketType === "STUMPED";

  const isRunOut =
    wicketType === "RUN OUT";

  const isFreeHit =
    Boolean(liveMatch?.isFreeHit);

  /*
   * On a free hit, only Run Out is selectable because
   * that is the supported dismissal allowed by the
   * current scoring implementation.
   */
  const restrictedWicketTypes = isFreeHit
    ? WICKET_TYPES.filter(
        (type) => type !== "RUN OUT"
      )
    : [];

  /*
   * Determine whether this wicket ends the innings.
   * The backend remains authoritative; this is only
   * used to control the UI.
   */
  const inningsWillEndAfterThisWicket = useMemo(() => {
    const playersPerTeam =
      Number(match?.playersPerTeam || 0);

    const maximumWickets =
      Math.max(playersPerTeam - 1, 1);

    const currentWickets =
      Number(liveMatch?.wickets || 0);

    const endsByWickets =
      currentWickets + 1 >= maximumWickets;

    const currentLegalBalls =
      Number(liveMatch?.legalBalls || 0);

    const totalMatchBalls =
      Number(match?.overs || 0) * 6;

    const endsByOvers =
      totalMatchBalls > 0 &&
      currentLegalBalls + 1 >= totalMatchBalls;

    /*
     * A run out can include successfully completed runs,
     * so those runs are included when checking whether
     * the chasing team reaches the target.
     */
    const endsByTarget =
      Number(liveMatch?.inningsNumber) === 2 &&
      liveMatch?.firstInningsTotalRuns !== null &&
      liveMatch?.firstInningsTotalRuns !== undefined &&
      Number(liveMatch?.totalRuns || 0) +
        Number(runsCompleted || 0) >
        Number(liveMatch.firstInningsTotalRuns);

    return (
      endsByWickets ||
      endsByOvers ||
      endsByTarget
    );
  }, [
    match?.playersPerTeam,
    match?.overs,
    liveMatch?.wickets,
    liveMatch?.legalBalls,
    liveMatch?.inningsNumber,
    liveMatch?.firstInningsTotalRuns,
    liveMatch?.totalRuns,
    runsCompleted
  ]);

  /*
   * Reset the modal whenever a new scoring state is opened.
   */
  useEffect(() => {
    if (!liveMatch) {
      return;
    }

    setWicketType(
      isFreeHit
        ? "RUN OUT"
        : WICKET_TYPES.includes(initialWicketType)
          ? initialWicketType
          : "BOWLED"
    );

    setDismissedPlayerId(
      liveMatch.strikerMatchPlayerId
        ? String(liveMatch.strikerMatchPlayerId)
        : ""
    );

    setCaughtBy("");
    setStumpedBy("");
    setRunsCompleted(0);
    setDidBattersCross(false);
    setNewBatterId("");
  }, [
    liveMatch?.inningsId,
    liveMatch?.balls?.length,
    isFreeHit,
    initialWicketType
  ]);

  const handleWicketTypeChange = (event) => {
    const value = event.target.value;

    if (
      isFreeHit &&
      value !== "RUN OUT"
    ) {
      return;
    }

    setWicketType(value);

    if (value !== "CAUGHT") {
      setCaughtBy("");
    }

    if (value !== "STUMPED") {
      setStumpedBy("");
    }

    if (value !== "RUN OUT") {
      setDismissedPlayerId(
        strikerId > 0 ? String(strikerId) : ""
      );
      setRunsCompleted(0);
      setDidBattersCross(false);
    }
  };

  const handleRunsChange = (event) => {
    const value = Number(event.target.value);

    if (!Number.isFinite(value)) {
      setRunsCompleted(0);
      return;
    }

    setRunsCompleted(
      Math.max(0, Math.floor(value))
    );
  };

  const submit = () => {
    const dismissedId =
      Number(dismissedPlayerId);

    if (!dismissedId) {
      return;
    }

    if (
      requiresFielder &&
      !Number(
        wicketType === "CAUGHT"
          ? caughtBy
          : stumpedBy
      )
    ) {
      return;
    }

    if (
      isRunOut &&
      runsCompleted < 0
    ) {
      return;
    }

    if (
      !isRunOut &&
      runsCompleted !== 0
    ) {
      return;
    }

    if (
      isFreeHit &&
      wicketType !== "RUN OUT"
    ) {
      return;
    }

    /*
     * If the innings continues, an incoming batter must
     * be selected whenever an eligible player exists.
     */
    if (
      !inningsWillEndAfterThisWicket &&
      eligibleIncomingBatters.length > 0 &&
      !Number(newBatterId)
    ) {
      return;
    }

    const payload = {
      wicketType,

      dismissedMatchPlayerId:
        dismissedId,

      caughtByMatchPlayerId:
        wicketType === "CAUGHT"
          ? Number(caughtBy)
          : null,

      stumpedByMatchPlayerId:
        wicketType === "STUMPED"
          ? Number(stumpedBy)
          : null,

      runsCompleted:
        isRunOut
          ? Number(runsCompleted)
          : 0,

      didBattersCross:
        isRunOut
          ? Boolean(didBattersCross)
          : false,

      newBatterMatchPlayerId:
        inningsWillEndAfterThisWicket
          ? null
          : Number(newBatterId)
    };

    onConfirm(payload);
  };

  const selectedFielderId =
    wicketType === "CAUGHT"
      ? caughtBy
      : stumpedBy;

  const incomingBatterRequired =
    !inningsWillEndAfterThisWicket;

  const invalid =
    !dismissedPlayerId ||

    (
      requiresFielder &&
      !Number(selectedFielderId)
    ) ||

    (
      isRunOut &&
      runsCompleted < 0
    ) ||

    (
      !isRunOut &&
      runsCompleted !== 0
    ) ||

    (
      isFreeHit &&
      wicketType !== "RUN OUT"
    ) ||

    (
      incomingBatterRequired &&
      eligibleIncomingBatters.length > 0 &&
      !Number(newBatterId)
    );

  return (
    <div
      className="modal d-block"
      tabIndex="-1"
      style={{
        background: "rgba(0,0,0,.55)"
      }}
    >
      <div className="modal-dialog modal-dialog-centered">
        <div className="modal-content">

          {/* Header */}
          <div className="modal-header">
            <h5 className="modal-title">
              Record Wicket
            </h5>

            <button
              type="button"
              className="btn-close"
              onClick={onClose}
              disabled={loading}
            />
          </div>

          {/* Body */}
          <div className="modal-body">

            {/* Free Hit */}
            {isFreeHit && (
              <div className="alert alert-warning">
                <strong>FREE HIT</strong>
                <br />
                Only Run Out can be recorded from
                the available wicket types for this
                delivery.
              </div>
            )}

            {/* Wicket Type */}
            <label className="form-label">
              Wicket type
            </label>

            <select
              className="form-select mb-3"
              value={wicketType}
              onChange={handleWicketTypeChange}
              disabled={loading}
            >
              {WICKET_TYPES.map((type) => (
                <option
                  key={type}
                  value={type}
                  disabled={
                    restrictedWicketTypes.includes(type)
                  }
                >
                  {type}
                </option>
              ))}
            </select>

            {/* Dismissed Batter */}
            <label className="form-label">
              Who is out?
            </label>

            <select
              className="form-select mb-3"
              value={dismissedPlayerId}
              onChange={(event) =>
                isRunOut &&
                setDismissedPlayerId(
                  event.target.value
                )
              }
              disabled={loading || !isRunOut}
            >
              <option value="">
                Select batter
              </option>

              {[
                liveMatch?.strikerMatchPlayerId,
                liveMatch?.nonStrikerMatchPlayerId
              ]
                .filter(Boolean)
                .map((id) => {
                  const player =
                    players.find(
                      (item) =>
                        playerId(item) ===
                        Number(id)
                    );

                  if (!player) {
                    return null;
                  }

                  return (
                    <option
                      key={id}
                      value={id}
                    >
                      {playerName(player)}
                    </option>
                  );
                })}
            </select>

            {/* Caught / Stumped Fielder */}
            {requiresFielder && (
              <>
                <label className="form-label">
                  {wicketType === "CAUGHT"
                    ? "Caught by"
                    : "Stumped by"}
                </label>

                <select
                  className="form-select mb-3"
                  value={selectedFielderId}
                  onChange={(event) => {
                    if (
                      wicketType === "CAUGHT"
                    ) {
                      setCaughtBy(
                        event.target.value
                      );
                    } else {
                      setStumpedBy(
                        event.target.value
                      );
                    }
                  }}
                  disabled={loading}
                >
                  <option value="">
                    Select fielding player
                  </option>

                  {fieldingPlayers.map(
                    (player) => (
                      <option
                        key={playerId(player)}
                        value={playerId(player)}
                      >
                        {playerName(player)}
                      </option>
                    )
                  )}
                </select>
              </>
            )}

            {/* Run Out */}
            {isRunOut && (
              <>
                <label className="form-label">
                  Runs successfully completed
                </label>

                <input
                  type="number"
                  min="0"
                  step="1"
                  className="form-control mb-3"
                  value={runsCompleted}
                  onChange={handleRunsChange}
                  disabled={loading}
                />

                <label className="form-label">
                  Did the batters cross?
                </label>

                <div className="d-flex gap-3 mb-3">

                  <label>
                    <input
                      type="radio"
                      name="didBattersCross"
                      checked={
                        didBattersCross === true
                      }
                      onChange={() =>
                        setDidBattersCross(true)
                      }
                      disabled={loading}
                    />{" "}
                    Yes
                  </label>

                  <label>
                    <input
                      type="radio"
                      name="didBattersCross"
                      checked={
                        didBattersCross === false
                      }
                      onChange={() =>
                        setDidBattersCross(false)
                      }
                      disabled={loading}
                    />{" "}
                    No
                  </label>

                </div>
              </>
            )}

            {/* Incoming Batter */}
            {incomingBatterRequired &&
              eligibleIncomingBatters.length > 0 && (
                <>
                  <label className="form-label">
                    Incoming batter
                  </label>

                  <select
                    className="form-select"
                    value={newBatterId}
                    onChange={(event) =>
                      setNewBatterId(
                        event.target.value
                      )
                    }
                    disabled={loading}
                  >
                    <option value="">
                      Select incoming batter
                    </option>

                    {eligibleIncomingBatters.map(
                      (player) => (
                        <option
                          key={playerId(player)}
                          value={playerId(player)}
                        >
                          {playerName(player)}
                        </option>
                      )
                    )}
                  </select>
                </>
              )}

            {/* Innings End */}
            {inningsWillEndAfterThisWicket && (
              <div className="alert alert-info mt-3 mb-0">
                This wicket will end the innings.
                No incoming batter is required.
              </div>
            )}

          </div>

          {/* Footer */}
          <div className="modal-footer">

            <button
              type="button"
              className="btn btn-secondary"
              onClick={onClose}
              disabled={loading}
            >
              Cancel
            </button>

            <button
              type="button"
              className="btn btn-danger"
              onClick={submit}
              disabled={
                loading ||
                invalid
              }
            >
              {loading
                ? "Saving..."
                : "Record Wicket"}
            </button>

          </div>

        </div>
      </div>
    </div>
  );
};

export default WicketScoringModal;