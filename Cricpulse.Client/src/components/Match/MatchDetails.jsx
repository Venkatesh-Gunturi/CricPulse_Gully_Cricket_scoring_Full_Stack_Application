const MatchInfo = ({ match, liveMatch, onClose }) => {
  const players = liveMatch?.players || match?.players || [];

  const team1Players = players.filter(
    (player) =>
      player.team === "Team1" ||
      player.team === match?.team1Name ||
      player.teamName === match?.team1Name
  );

  const team2Players = players.filter(
    (player) =>
      player.team === "Team2" ||
      player.team === match?.team2Name ||
      player.teamName === match?.team2Name
  );

  const playerName = (player) =>
    player.displayName ||
    player.playerName ||
    player.name ||
    player.mobileNumber ||
    "Unknown Player";

  const logo = (value) => {
    if (!value) return null;

    if (String(value).startsWith("http")) {
      return (
        <img
          src={value}
          alt=""
          style={{
            width: 64,
            height: 64,
            objectFit: "contain"
          }}
        />
      );
    }

    return (
      <div
        className="rounded-circle border d-flex align-items-center justify-content-center"
        style={{
          width: 64,
          height: 64
        }}
      >
        {value}
      </div>
    );
  };

  const formatDate = (value) => {
    if (!value) return "Not available";

    return new Date(value).toLocaleDateString();
  };

  const formatDateTime = (value) => {
    if (!value) return "Not available";

    return new Date(value).toLocaleString();
  };

  const matchStatus =
    liveMatch?.status ||
    match?.status ||
    "Not available";

  const startedAt =
    liveMatch?.startedAt ||
    match?.startedAt;

  const createdAt =
    liveMatch?.createdAt ||
    match?.createdAt;

  const tossWinner =
    liveMatch?.tossWinnerTeam ||
    match?.tossWinnerTeam;

  const tossDecision =
    liveMatch?.tossDecision ||
    match?.tossDecision;

  const battingFirst =
    liveMatch?.battingFirstTeam ||
    match?.battingFirstTeam;

  return (
    <div className="container py-4">
      {/* Header */}
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2 className="mb-1">Match Info</h2>
          <small className="text-muted">
            Complete match and playing XI information
          </small>
        </div>

        <button
          className="btn btn-secondary"
          onClick={onClose}
        >
          ← Back to Match
        </button>
      </div>

      {/* Teams */}
      <div className="card mb-4">
        <div className="card-body">
          <div className="row align-items-center text-center">
            <div className="col-md-5">
              {logo(match?.team1Logo)}

              <h3 className="mt-2">
                {match?.team1Name || "Team 1"}
              </h3>

              <span className="text-muted">
                {team1Players.length} players
              </span>
            </div>

            <div className="col-md-2">
              <h4 className="text-muted">VS</h4>
            </div>

            <div className="col-md-5">
              {logo(match?.team2Logo)}

              <h3 className="mt-2">
                {match?.team2Name || "Team 2"}
              </h3>

              <span className="text-muted">
                {team2Players.length} players
              </span>
            </div>
          </div>
        </div>
      </div>

      {/* Match Info */}
      <div className="card mb-4">
        <div className="card-header">
          <h4 className="mb-0">Match Info</h4>
        </div>

        <div className="card-body">
          <div className="row g-3">
            <div className="col-md-4">
              <strong>Players per team</strong>
              <div>
                {match?.playersPerTeam ?? "Not available"}
              </div>
            </div>

            <div className="col-md-4">
              <strong>Overs</strong>
              <div>
                {match?.overs ?? "Not available"}
              </div>
            </div>

            <div className="col-md-4">
              <strong>Match date</strong>
              <div>
                {formatDate(match?.matchDate)}
              </div>
            </div>

            <div className="col-md-4">
              <strong>Match time</strong>
              <div>
                {match?.matchTime || "Not available"}
              </div>
            </div>

            <div className="col-md-4">
              <strong>Venue</strong>
              <div>
                {match?.venueName || "Not available"}
              </div>
            </div>

            <div className="col-md-4">
              <strong>Address</strong>
              <div>
                {match?.address || "Not available"}
              </div>
            </div>

            <div className="col-md-4">
              <strong>State</strong>
              <div>
                {match?.state || "Not available"}
              </div>
            </div>

            <div className="col-md-4">
              <strong>Latitude</strong>
              <div>
                {match?.latitude ?? "Not available"}
              </div>
            </div>

            <div className="col-md-4">
              <strong>Longitude</strong>
              <div>
                {match?.longitude ?? "Not available"}
              </div>
            </div>

            <div className="col-md-4">
              <strong>Status</strong>
              <div>
                {matchStatus}
              </div>
            </div>

            <div className="col-md-4">
              <strong>Started at</strong>
              <div>
                {formatDateTime(startedAt)}
              </div>
            </div>

            <div className="col-md-4">
              <strong>Created at</strong>
              <div>
                {formatDateTime(createdAt)}
              </div>
            </div>

            <div className="col-md-12">
              <strong>Live stream</strong>

              <div>
                {match?.liveStreamUrl ? (
                  <a
                    href={match.liveStreamUrl}
                    target="_blank"
                    rel="noreferrer"
                  >
                    {match.liveStreamUrl}
                  </a>
                ) : (
                  "Not available"
                )}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Toss Information */}
      <div className="card mb-4">
        <div className="card-header">
          <h4 className="mb-0">Toss</h4>
        </div>

        <div className="card-body">
          <div className="row g-3">
            <div className="col-md-4">
              <strong>Toss winner</strong>

              <div>
                {tossWinner || "Not recorded"}
              </div>
            </div>

            <div className="col-md-4">
              <strong>Decision</strong>

              <div>
                {tossDecision || "Not recorded"}
              </div>
            </div>

            <div className="col-md-4">
              <strong>Batting first</strong>

              <div>
                {battingFirst || "Not recorded"}
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Team 1 Players */}
      <div className="row g-4">
        <div className="col-md-6">
          <div className="card h-100">
            <div className="card-header">
              <h4 className="mb-0">
                {match?.team1Name || "Team 1"}
              </h4>
            </div>

            <div className="list-group list-group-flush">
              {team1Players.length > 0 ? (
                team1Players.map((player, index) => (
                  <div
                    key={
                      player.matchPlayerId ??
                      player.playerId ??
                      index
                    }
                    className="list-group-item d-flex justify-content-between align-items-center"
                  >
                    <span>
                      {playerName(player)}
                    </span>

                    {player.isCaptain && (
                      <span className="badge bg-warning text-dark">
                        Captain
                      </span>
                    )}
                  </div>
                ))
              ) : (
                <div className="list-group-item text-muted">
                  No players available.
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Team 2 Players */}
        <div className="col-md-6">
          <div className="card h-100">
            <div className="card-header">
              <h4 className="mb-0">
                {match?.team2Name || "Team 2"}
              </h4>
            </div>

            <div className="list-group list-group-flush">
              {team2Players.length > 0 ? (
                team2Players.map((player, index) => (
                  <div
                    key={
                      player.matchPlayerId ??
                      player.playerId ??
                      index
                    }
                    className="list-group-item d-flex justify-content-between align-items-center"
                  >
                    <span>
                      {playerName(player)}
                    </span>

                    {player.isCaptain && (
                      <span className="badge bg-warning text-dark">
                        Captain
                      </span>
                    )}
                  </div>
                ))
              ) : (
                <div className="list-group-item text-muted">
                  No players available.
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default MatchInfo;