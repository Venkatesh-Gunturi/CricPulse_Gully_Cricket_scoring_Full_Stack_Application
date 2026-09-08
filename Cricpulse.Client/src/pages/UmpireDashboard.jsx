import { useEffect, useState } from "react";
import { getMyMatches } from "../services/matchService";
import MatchManagement from "../components/Match/MatchManagement";

const UmpireDashboard = ({ onCreateMatch }) => {
  const [matches, setMatches] = useState([]);
  const [selectedStatus, setSelectedStatus] = useState("Scheduled");
  const [selectedMatch, setSelectedMatch] = useState(null);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const loadMatches = async () => {
    try {
      setLoading(true);
      setError("");

      const data = await getMyMatches();

      setMatches(data);
    } catch (error) {
      console.error("Failed to load umpire matches:", error);

      setError("Failed to load your matches.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadMatches();
  }, []);

  const filteredMatches = matches.filter(
    (match) => match.status === selectedStatus
  );

  const handleMatchUpdated = (updatedMatch) => {
    setMatches((previousMatches) =>
      previousMatches.map((match) =>
        match.id === updatedMatch.id
          ? updatedMatch
          : match
      )
    );

    setSelectedMatch(updatedMatch);
  };

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
        <p>Loading your matches...</p>
      </div>
    );
  }

  return (
    <div className="container mt-4">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <div>
          <h2>Umpire Dashboard</h2>

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

      {error && (
        <div className="alert alert-danger">
          {error}
        </div>
      )}

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

      <h4 className="mb-3">
        {getStatusTitle()}
      </h4>

      {filteredMatches.length === 0 && (
        <div className="alert alert-light border">
          No {selectedStatus.toLowerCase()} matches.
        </div>
      )}

      {filteredMatches.map((match) => (
        <div
          key={match.id}
          className="card mb-3"
        >
          <div className="card-body">
            <div className="d-flex justify-content-between align-items-start">
              <div>
                <h5 className="card-title">
                  {match.team1Name} vs {match.team2Name}
                </h5>

                <p className="text-muted mb-1">
                  {new Date(
                    match.matchDate
                  ).toLocaleDateString()}{" "}
                  • {match.matchTime}
                </p>

                <p className="mb-1">
                  <strong>Venue:</strong>{" "}
                  {match.venueName}
                </p>

                <p className="mb-1">
                  <strong>State:</strong>{" "}
                  {match.state}
                </p>

                <p className="mb-0">
                  <strong>Status:</strong>{" "}
                  {match.status}
                </p>
              </div>

              {match.status === "Scheduled" && (
                <button
                  className="btn btn-outline-primary"
                  onClick={() =>
                    setSelectedMatch(match)
                  }
                >
                  Manage
                </button>
              )}

              {match.status === "Live" && (
                <button
                  className="btn btn-outline-danger"
                  onClick={() =>
                    setSelectedMatch(match)
                  }
                >
                  Manage
                </button>
              )}
            </div>

            {selectedMatch?.id === match.id && (
              <MatchManagement
                match={selectedMatch}
                onMatchUpdated={handleMatchUpdated}
              />
            )}
          </div>
        </div>
      ))}
    </div>
  );
};

export default UmpireDashboard;