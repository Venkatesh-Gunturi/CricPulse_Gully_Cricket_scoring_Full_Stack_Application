import { useEffect, useState } from "react";
import {
  getAllMatches,
  getNearbyMatches,
  getMatchesByState
} from "../../services/matchService";
import "./MatchList.css";

const MatchList = ({
  status,
  mode = "all",
  location = null,
  state = ""
}) => {
  const [matches, setMatches] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    const loadMatches = async () => {
      try {
        setLoading(true);
        setError("");

        let data = [];

        if (mode === "state" && state) {
          data = await getMatchesByState(state);
        } else if (mode === "nearby" && location) {
          data = await getNearbyMatches(
            location.latitude,
            location.longitude
          );
        } else {
          data = await getAllMatches();
        }

        const filteredMatches = Array.isArray(data)
          ? data.filter((match) => match.status === status)
          : [];

        setMatches(filteredMatches);
      } catch (err) {
        console.error("Failed to load matches:", err);
        setError("Failed to load matches.");
        setMatches([]);
      } finally {
        setLoading(false);
      }
    };

    loadMatches();
  }, [status, mode, location, state]);

  if (loading) {
    return (
      <div className="cp-match-list-state">
        <div className="cp-match-spinner" />
        <p>Loading matches...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="cp-match-list-state cp-match-list-error">
        <div className="cp-match-state-icon">!</div>
        <h3>Couldn't load matches</h3>
        <p>{error}</p>
      </div>
    );
  }

  if (matches.length === 0) {
    return (
      <div className="cp-match-list-state">
        <div className="cp-match-state-icon">
          🏏
        </div>

        <h3>No matches found</h3>

        <p>
          There are no matches in this category right now.
        </p>
      </div>
    );
  }

  return (
    <div className="cp-match-list">
      {matches.map((match) => (
        <MatchCard
          key={match.id}
          match={match}
        />
      ))}
    </div>
  );
};

const MatchCard = ({ match }) => {
  const isLive = match.status === "Live";
  const isCompleted = match.status === "Completed";
  const isCancelled = match.status === "Cancelled";
  const isScheduled = match.status === "Scheduled";

  const formattedDate = match.matchDate
    ? new Date(match.matchDate).toLocaleDateString(
        "en-IN",
        {
          day: "2-digit",
          month: "short",
          year: "numeric"
        }
      )
    : "Date unavailable";

  return (
    <article
      className={`cp-match-card ${
        isLive ? "is-live" : ""
      }`}
    >
      <div className="cp-match-card-top">
        <div className="cp-match-venue">
          <span className="cp-venue-icon">⌖</span>

          <span>
            {match.venueName || "Venue unavailable"}
          </span>
        </div>

        <StatusBadge status={match.status} />
      </div>

      <div className="cp-match-teams">
        <div className="cp-team">
          <span className="cp-team-label">
            TEAM A
          </span>

          <strong>
            {match.team1Name || "Team 1"}
          </strong>

          {isLive && (
            <span className="cp-team-score">
              {match.team1Score ??
                match.team1Runs ??
                "—"}
            </span>
          )}
        </div>

        <div className="cp-vs">
          <span>VS</span>
        </div>

        <div className="cp-team cp-team-right">
          <span className="cp-team-label">
            TEAM B
          </span>

          <strong>
            {match.team2Name || "Team 2"}
          </strong>

          {isLive && (
            <span className="cp-team-score">
              {match.team2Score ??
                match.team2Runs ??
                "—"}
            </span>
          )}
        </div>
      </div>

      <div className="cp-match-meta">
        <div className="cp-match-meta-item">
          <span className="cp-meta-label">
            DATE
          </span>

          <span>{formattedDate}</span>
        </div>

        <div className="cp-match-meta-item">
          <span className="cp-meta-label">
            TIME
          </span>

          <span>
            {match.matchTime || "Time unavailable"}
          </span>
        </div>

        <div className="cp-match-meta-item">
          <span className="cp-meta-label">
            OVERS
          </span>

          <span>
            {match.overs ?? "—"}
          </span>
        </div>

        <div className="cp-match-meta-item">
          <span className="cp-meta-label">
            PLAYERS
          </span>

          <span>
            {match.playersPerTeam ?? "—"} / team
          </span>
        </div>
      </div>

      <div className="cp-match-card-bottom">
        <div className="cp-match-location">
          <span>📍</span>

          <span>
            {match.address || "Address unavailable"}
          </span>
        </div>

        {match.distanceInKm != null &&
          !isCancelled &&
          !isCompleted && (
            <span className="cp-distance">
              {Number(match.distanceInKm).toFixed(1)} km away
            </span>
          )}
      </div>

      {isCancelled && (
        <div className="cp-cancelled-reason">
          <span>Cancellation reason</span>

          <strong>
            {match.cancellationReason ||
              match.cancelReason ||
              "No reason provided"}
          </strong>
        </div>
      )}

      {isCompleted && (
        <div className="cp-completed-footer">
          <span>Match completed</span>
        </div>
      )}

      {isScheduled && (
        <div className="cp-scheduled-footer">
          <span>Upcoming match</span>
        </div>
      )}

      {isLive && (
        <div className="cp-live-footer">
          <span className="cp-live-pulse" />
          <span>Match is currently in progress</span>
        </div>
      )}
    </article>
  );
};

const StatusBadge = ({ status }) => {
  const config = {
    Scheduled: {
      label: "UPCOMING",
      className: "scheduled"
    },
    Live: {
      label: "LIVE",
      className: "live"
    },
    Completed: {
      label: "FINISHED",
      className: "completed"
    },
    Cancelled: {
      label: "CANCELLED",
      className: "cancelled"
    }
  };

  const current = config[status] || {
    label: status || "UNKNOWN",
    className: "unknown"
  };

  return (
    <span
      className={`cp-match-status ${current.className}`}
    >
      {status === "Live" && (
        <span className="cp-status-live-dot" />
      )}

      {current.label}
    </span>
  );
};

export default MatchList;