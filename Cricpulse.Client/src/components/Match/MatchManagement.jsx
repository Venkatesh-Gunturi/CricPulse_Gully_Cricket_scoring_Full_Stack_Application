import { useState } from "react";
import {
  updateMatch,
  startMatch,
  cancelMatch
} from "../../services/matchService";


const MatchManagement = ({ match, onMatchUpdated }) => {
  const [editing, setEditing] = useState(false);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

 const [formData, setFormData] = useState({
  team1Name: match.team1Name,
  team1Logo: match.team1Logo,
  team2Name: match.team2Name,
  team2Logo: match.team2Logo,
  playersPerTeam: match.playersPerTeam,
  overs: match.overs,
  matchDate: match.matchDate?.split("T")[0] || "",
  matchTime: match.matchTime,
  venueName: match.venueName,
  address: match.address,
  liveStreamUrl: match.liveStreamUrl || ""
});

  const handleChange = (event) => {
    const { name, value } = event.target;

    setFormData((previousData) => ({
      ...previousData,
      [name]: value
    }));
  };

  const handleUpdate = async () => {
    try {
      setLoading(true);
      setError("");
      setMessage("");

      const updatedMatch = await updateMatch(
  match.id,
  {
    ...formData,
    playersPerTeam: Number(formData.playersPerTeam),
    overs: Number(formData.overs)
  }
);

      setEditing(false);
      setMessage("Match updated successfully.");

      onMatchUpdated(updatedMatch);
    } catch (error) {
      console.error("Failed to update match:", error);
      setError(
        error.response?.data || "Failed to update match."
      );
    } finally {
      setLoading(false);
    }
  };

  const handleStart = async () => {
    const confirmed = window.confirm(
      "Please make sure you are at the match venue. Your current GPS location will become the exact match location. Start the match?"
    );

    if (!confirmed) {
      return;
    }

    try {
      setLoading(true);
      setError("");
      setMessage("");

      const coordinates = await getCurrentLocation();

      const updatedMatch = await startMatch(
        match.id,
        coordinates.latitude,
        coordinates.longitude
      );

      setMessage("Match started successfully.");

      onMatchUpdated(updatedMatch);
    } catch (error) {
      console.error("Failed to start match:", error);

      setError(
        error.response?.data ||
          "Unable to start match. Please allow location access."
      );
    } finally {
      setLoading(false);
    }
  };

  const handleCancel = async () => {
    const confirmed = window.confirm(
      "Are you sure you want to cancel this match?"
    );

    if (!confirmed) {
      return;
    }

    try {
      setLoading(true);
      setError("");
      setMessage("");

      const result = await cancelMatch(match.id);

      setMessage(
        result.message || "Match cancelled successfully."
      );

      onMatchUpdated({
        ...match,
        status: "Cancelled"
      });
    } catch (error) {
      console.error("Failed to cancel match:", error);

      setError(
        error.response?.data ||
          "Failed to cancel match."
      );
    } finally {
      setLoading(false);
    }
  };

  if (editing) {
    return (
      <div className="card mt-3">
        <div className="card-body">
          <h5>Edit Match</h5>

          <div className="mb-3">
            <label className="form-label">
              Team 1 Name
            </label>

            <input
              type="text"
              name="team1Name"
              className="form-control"
              value={formData.team1Name}
              onChange={handleChange}
            />
          </div>

          <div className="mb-3">
            <label className="form-label">
              Team 2 Name
            </label>

            <input
              type="text"
              name="team2Name"
              className="form-control"
              value={formData.team2Name}
              onChange={handleChange}
            />
          </div>

          <div className="row">
            <div className="col-md-6 mb-3">
              <label className="form-label">
                Players per Team
              </label>

              <input
                type="number"
                name="playersPerTeam"
                className="form-control"
                value={formData.playersPerTeam}
                onChange={handleChange}
              />
            </div>

            <div className="col-md-6 mb-3">
              <label className="form-label">
                Overs
              </label>

              <input
                type="number"
                name="overs"
                className="form-control"
                value={formData.overs}
                onChange={handleChange}
              />
            </div>
          </div>

          <div className="row">
            <div className="col-md-6 mb-3">
              <label className="form-label">
                Match Date
              </label>

              <input
                type="date"
                name="matchDate"
                className="form-control"
                value={formData.matchDate}
                onChange={handleChange}
              />
            </div>

            <div className="col-md-6 mb-3">
              <label className="form-label">
                Match Time
              </label>

              <input
                type="time"
                name="matchTime"
                className="form-control"
                value={formData.matchTime}
                onChange={handleChange}
              />
            </div>
          </div>

          <div className="mb-3">
            <label className="form-label">
              Venue Name
            </label>

            <input
              type="text"
              name="venueName"
              className="form-control"
              value={formData.venueName}
              onChange={handleChange}
            />
          </div>

          <div className="mb-3">
            <label className="form-label">
              Address
            </label>

            <textarea
              name="address"
              className="form-control"
              value={formData.address}
              onChange={handleChange}
            />
          </div>

          <div className="mb-3">
            <label className="form-label">
              Match Location
            </label>

            <div className="alert alert-light border mb-0">
              <strong>📍 Location is locked</strong>

              <p className="mb-0 mt-1 text-muted">
                The exact match location will be captured
                automatically when you start the match.
              </p>
            </div>
        </div>

          <div className="mb-3">
            <label className="form-label">
              Live Stream URL
            </label>

            <input
              type="url"
              name="liveStreamUrl"
              className="form-control"
              value={formData.liveStreamUrl}
              onChange={handleChange}
            />
          </div>

          <button
            type="button"
            className="btn btn-primary me-2"
            onClick={handleUpdate}
            disabled={loading}
          >
            {loading ? "Saving..." : "Save Changes"}
          </button>

          <button
            type="button"
            className="btn btn-secondary"
            onClick={() => setEditing(false)}
            disabled={loading}
          >
            Cancel
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="mt-3">
      {message && (
        <div className="alert alert-success">
          {message}
        </div>
      )}

      {error && (
        <div className="alert alert-danger">
          {error}
        </div>
      )}

      {match.status === "Scheduled" && (
        <>
          <button
            type="button"
            className="btn btn-primary me-2"
            onClick={() => setEditing(true)}
            disabled={loading}
          >
            ✏️ Edit Match
          </button>

          <button
            type="button"
            className="btn btn-success me-2"
            onClick={handleStart}
            disabled={loading}
          >
            {loading
              ? "Starting..."
              : "▶️ Start Match"}
          </button>

          <button
            type="button"
            className="btn btn-danger"
            onClick={handleCancel}
            disabled={loading}
          >
            ❌ Cancel Match
          </button>
        </>
      )}

      {match.status === "Live" && (
        <button
          type="button"
          className="btn btn-danger"
          onClick={handleCancel}
          disabled={loading}
        >
          ❌ Cancel Match
        </button>
      )}
    </div>
  );
};

export default MatchManagement;