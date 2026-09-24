import { useEffect, useState } from "react";
import { getMyMatches } from "../services/matchService";
import MatchManagement from "../components/Match/MatchManagement";

const UmpireDashboard = ({
  onCreateMatch,
  onContinueScoring
}) => {
  const [matches, setMatches] = useState([]);
  const [selectedStatus, setSelectedStatus] =
    useState("Scheduled");
  const [selectedMatch, setSelectedMatch] =
    useState(null);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Purpose:
  // Load all matches belonging to the currently logged-in umpire.
  const loadMatches = async () => {
    try {
      setLoading(true);
      setError("");

      const data = await getMyMatches();

      setMatches(data ?? []);
    } catch (error) {
      console.error(
        "Failed to load umpire matches:",
        error
      );

      setError(
        "Failed to load your matches."
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadMatches();
  }, []);

  const filteredMatches = matches.filter(
    (match) =>
      match.status === selectedStatus
  );

  // Purpose:
  // Update the dashboard immediately after a match
  // is edited, started, or cancelled.
  const handleMatchUpdated = (
    updatedMatch
  ) => {
    setMatches((previousMatches) =>
      previousMatches.map((match) =>
        match.id === updatedMatch.id
          ? updatedMatch
          : match
      )
    );

    setSelectedMatch(updatedMatch);
  };

  // Purpose:
  // Return the appropriate heading for the
  // selected match category.
  const getStatusTitle = () => {
    switch (selectedStatus) {
      case "Scheduled":
        return "My Upcoming Matches";

      case "Live":
        return "My Live Matches";

      case "Completed":
        return "My Finished Matches";

      case "Cancelled":
        return "My Cancelled Matches";

      default:
        return "My Matches";
    }
  };

  if (loading) {
    return (
      <div className="container mt-5">
        <p>
          Loading your matches...
        </p>
      </div>
    );
  }

  return (
    <div className="container mt-4">

      {/* Dashboard Header */}
      <div className="d-flex justify-content-between align-items-center mb-4">

        <div>
          <h2>
            Umpire Dashboard
          </h2>

          <p className="text-muted mb-0">
            Manage your matches and scoring.
          </p>
        </div>

        <button
          className="btn btn-primary"
          onClick={onCreateMatch}
        >
          + Create Match
        </button>

      </div>

      {/* Error */}
      {error && (
        <div className="alert alert-danger">
          {error}
        </div>
      )}

      {/* Status Filters */}
      <div className="d-flex gap-2 flex-wrap mb-4">

        <button
          className={
            selectedStatus === "Scheduled"
              ? "btn btn-primary"
              : "btn btn-outline-primary"
          }
          onClick={() => {
            setSelectedStatus("Scheduled");
            setSelectedMatch(null);
          }}
        >
          Upcoming
        </button>

        <button
          className={
            selectedStatus === "Live"
              ? "btn btn-danger"
              : "btn btn-outline-danger"
          }
          onClick={() => {
            setSelectedStatus("Live");
            setSelectedMatch(null);
          }}
        >
          Live
        </button>

        <button
          className={
            selectedStatus === "Completed"
              ? "btn btn-secondary"
              : "btn btn-outline-secondary"
          }
          onClick={() => {
            setSelectedStatus("Completed");
            setSelectedMatch(null);
          }}
        >
          Finished
        </button>

        <button
          className={
            selectedStatus === "Cancelled"
              ? "btn btn-dark"
              : "btn btn-outline-dark"
          }
          onClick={() => {
            setSelectedStatus("Cancelled");
            setSelectedMatch(null);
          }}
        >
          Cancelled
        </button>

      </div>

      {/* Current Category */}
      <h4 className="mb-3">
        {getStatusTitle()}
      </h4>

      {/* Empty State */}
      {filteredMatches.length === 0 && (
        <div className="alert alert-light border">
          No{" "}
          {selectedStatus.toLowerCase()}{" "}
          matches.
        </div>
      )}

      {/* Match List */}
      {filteredMatches.map((match) => (
        <div
          key={match.id}
          className="card mb-3"
        >
          <div className="card-body">

            <div className="d-flex justify-content-between align-items-start">

              {/* Match Information */}
              <div>

                <h5 className="card-title">
                  {match.team1Name} vs{" "}
                  {match.team2Name}
                </h5>

                <p className="text-muted mb-1">
                  {new Date(
                    match.matchDate
                  ).toLocaleDateString()}{" "}
                  • {match.matchTime}
                </p>

                <p className="mb-1">
                  <strong>
                    Venue:
                  </strong>{" "}
                  {match.venueName}
                </p>

                <p className="mb-1">
                  <strong>
                    State:
                  </strong>{" "}
                  {match.state}
                </p>

                <p className="mb-0">
                  <strong>
                    Status:
                  </strong>{" "}
                  {match.status}
                </p>

              </div>

              {/* Scheduled Match */}
              {match.status ===
                "Scheduled" && (
                <button
                  className="btn btn-outline-primary"
                  onClick={() => {
                    setSelectedMatch(
                      selectedMatch?.id ===
                        match.id
                        ? null
                        : match
                    );
                  }}
                >
                  {selectedMatch?.id ===
                  match.id
                    ? "Close"
                    : "Manage"}
                </button>
              )}

              {/* Live Match */}
              {match.status ===
                "Live" && (
                <button
                  className="btn btn-danger"
                  onClick={() =>
                    onContinueScoring(
                      match
                    )
                  }
                >
                  ▶️ Continue Scoring
                </button>
              )}

              {/* Completed Match */}
              {match.status ===
                "Completed" && (
                <span className="badge bg-secondary fs-6">
                  Completed
                </span>
              )}

              {/* Cancelled Match */}
              {match.status ===
                "Cancelled" && (
                <span className="badge bg-dark fs-6">
                  Cancelled
                </span>
              )}

            </div>

            {/* Scheduled Match Management */}
            {selectedMatch?.id ===
              match.id &&
              match.status ===
                "Scheduled" && (
                <MatchManagement
                  match={selectedMatch}
                  onMatchUpdated={
                    handleMatchUpdated
                  }
                />
              )}

          </div>
        </div>
      ))}

    </div>
  );
};

export default UmpireDashboard;

