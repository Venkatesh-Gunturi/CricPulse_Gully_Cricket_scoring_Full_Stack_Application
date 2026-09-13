import { useEffect, useState } from "react";

import {
  updateMatch,
  startMatch,
  cancelMatch
} from "../../services/matchService";

import {
  getCurrentLocation
} from "../../services/LocationService";

import MatchPlayerAssignment from "./PlayerAssignment";

const MatchManagement = ({
  match,
  onMatchUpdated
}) => {
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
    matchDate:
      match.matchDate?.split("T")[0] || "",
    matchTime:
      match.matchTime?.substring(0, 5) || "",
    venueName: match.venueName || "",
    address: match.address || "",
    liveStreamUrl:
      match.liveStreamUrl || ""
  });

  const [team1Players, setTeam1Players] =
    useState([]);

  const [team2Players, setTeam2Players] =
    useState([]);

    const [initialTeam1Players, setInitialTeam1Players] =
  useState([]);

  const [initialTeam2Players, setInitialTeam2Players] =
  useState([]);

 // Purpose:
// Load the existing match lineup into both the editable player state
// and the initial lineup state used by MatchPlayerAssignment.
useEffect(() => {
  const players = match.players || [];

  const team1 = players
    .filter((player) => player.team === "Team1")
    .map((player) => ({
      mobileNumber: player.mobileNumber,
      playerId: player.playerId,
      displayName: player.displayName,
      verified: true
    }));

  const team2 = players
    .filter((player) => player.team === "Team2")
    .map((player) => ({
      mobileNumber: player.mobileNumber,
      playerId: player.playerId,
      displayName: player.displayName,
      verified: true
    }));

  setInitialTeam1Players(team1);
  setInitialTeam2Players(team2);

  setTeam1Players(team1);
  setTeam2Players(team2);
}, [
  match.id,
  match.players
]);

  // Purpose:
  // Update a match form field.
  const handleChange = (event) => {
    const {
      name,
      value
    } = event.target;

    setFormData((previousData) => ({
      ...previousData,
      [name]: value
    }));

    setError("");
  };

  // Purpose:
  // Validate all editable match information before sending the update.
  const validateForm = () => {
    if (!formData.team1Name.trim()) {
      return "Team 1 name is required.";
    }

    if (!formData.team2Name.trim()) {
      return "Team 2 name is required.";
    }

    if (
      formData.team1Name.trim().toLowerCase() ===
      formData.team2Name.trim().toLowerCase()
    ) {
      return "Team 1 and Team 2 must have different names.";
    }

    const team1Logo =
      Number(formData.team1Logo);

    if (
      !Number.isInteger(team1Logo) ||
      team1Logo < 1 ||
      team1Logo > 10
    ) {
      return "Team 1 logo must be between 1 and 10.";
    }

    const team2Logo =
      Number(formData.team2Logo);

    if (
      !Number.isInteger(team2Logo) ||
      team2Logo < 1 ||
      team2Logo > 10
    ) {
      return "Team 2 logo must be between 1 and 10.";
    }

    const playersPerTeam =
      Number(formData.playersPerTeam);

    if (
      !Number.isInteger(playersPerTeam) ||
      playersPerTeam < 4 ||
      playersPerTeam > 11
    ) {
      return "Players per team must be between 4 and 11.";
    }

    const overs =
      Number(formData.overs);

    if (
      !Number.isInteger(overs) ||
      overs < 1 ||
      overs > 90
    ) {
      return "Overs must be between 1 and 90.";
    }

    if (!formData.matchDate) {
      return "Match date is required.";
    }

    if (!formData.matchTime) {
      return "Match time is required.";
    }

    if (!formData.venueName.trim()) {
      return "Venue name is required.";
    }

    if (
      team1Players.length !== playersPerTeam
    ) {
      return `Team 1 must have exactly ${playersPerTeam} verified players.`;
    }

    if (
      team2Players.length !== playersPerTeam
    ) {
      return `Team 2 must have exactly ${playersPerTeam} verified players.`;
    }

    const allPlayers = [
      ...team1Players,
      ...team2Players
    ];

    const mobileNumbers =
      allPlayers.map((player) =>
        player.mobileNumber.trim()
      );

    if (
      mobileNumbers.some(
        (mobile) =>
          !/^[0-9]{10}$/.test(mobile)
      )
    ) {
      return "Every player mobile number must contain exactly 10 digits.";
    }

    if (
      mobileNumbers.length !==
      new Set(mobileNumbers).size
    ) {
      return "A player cannot be assigned more than once.";
    }

    if (
      allPlayers.some(
        (player) => !player.playerId
      )
    ) {
      return "Every assigned player must be verified.";
    }

    if (
      formData.liveStreamUrl.trim()
    ) {
      try {
        const url = new URL(
          formData.liveStreamUrl.trim()
        );

        const allowedHosts = [
          "youtube.com",
          "www.youtube.com",
          "youtu.be",
          "m.youtube.com"
        ];

        if (
          !allowedHosts.includes(
            url.hostname.toLowerCase()
          )
        ) {
          return "Live stream URL must be a valid YouTube URL.";
        }
      } catch {
        return "Live stream URL must be a valid YouTube URL.";
      }
    }

    return "";
  };

  // Purpose:
  // Validate and save all editable match details and player assignments.
  const handleUpdate = async () => {
    const validationError =
      validateForm();

    if (validationError) {
      setError(validationError);
      setMessage("");
      return;
    }

    try {
      setLoading(true);
      setError("");
      setMessage("");

      const players = [
        ...team1Players.map((player) => ({
          mobileNumber:
            player.mobileNumber.trim(),
          team: "Team1"
        })),

        ...team2Players.map((player) => ({
          mobileNumber:
            player.mobileNumber.trim(),
          team: "Team2"
        }))
      ];

      const updateData = {
        team1Name:
          formData.team1Name.trim(),

        team1Logo:
          String(formData.team1Logo),

        team2Name:
          formData.team2Name.trim(),

        team2Logo:
          String(formData.team2Logo),

        playersPerTeam:
          Number(formData.playersPerTeam),

        overs:
          Number(formData.overs),

        matchDate:
          formData.matchDate,

        matchTime:
          formData.matchTime.length === 5
            ? `${formData.matchTime}:00`
            : formData.matchTime,

        venueName:
          formData.venueName.trim(),

        address:
          formData.address.trim(),

        liveStreamUrl:
          formData.liveStreamUrl.trim() || null,

        players
      };

      const updatedMatch =
        await updateMatch(
          match.id,
          updateData
        );

      setEditing(false);
      setMessage(
        "Match updated successfully."
      );

      onMatchUpdated(updatedMatch);
    } catch (error) {
      console.error(
        "Failed to update match:",
        error
      );

      const responseData =
        error.response?.data;

      if (
        typeof responseData === "string"
      ) {
        setError(responseData);
      } else if (
        responseData?.message
      ) {
        setError(
          responseData.message
        );
      } else {
        setError(
          "Failed to update match."
        );
      }
    } finally {
      setLoading(false);
    }
  };

  // Purpose:
  // Start the scheduled match using the umpire's current physical location.
  const handleStart = async () => {
    const confirmed =
      window.confirm(
        "Please make sure you are at the match venue. Your current GPS location will become the exact match location. Start the match?"
      );

    if (!confirmed) {
      return;
    }

    try {
      setLoading(true);
      setError("");
      setMessage("");

      const coordinates =
        await getCurrentLocation();

      const updatedMatch =
        await startMatch(
          match.id,
          coordinates.latitude,
          coordinates.longitude
        );

      setMessage(
        "Match started successfully."
      );

      onMatchUpdated(updatedMatch);
    } catch (error) {
      console.error(
        "Failed to start match:",
        error
      );

      const responseData =
        error.response?.data;

      if (
        typeof responseData === "string"
      ) {
        setError(responseData);
      } else if (
        responseData?.message
      ) {
        setError(
          responseData.message
        );
      } else {
        setError(
          "Unable to start match. Please allow location access."
        );
      }
    } finally {
      setLoading(false);
    }
  };

  // Purpose:
  // Cancel the scheduled or live match after umpire confirmation.
  const handleCancel = async () => {
    const confirmed =
      window.confirm(
        "Are you sure you want to cancel this match?"
      );

    if (!confirmed) {
      return;
    }

    try {
      setLoading(true);
      setError("");
      setMessage("");

      const result =
        await cancelMatch(
          match.id
        );

      setMessage(
        result.message ||
          "Match cancelled successfully."
      );

      onMatchUpdated({
        ...match,
        status: "Cancelled"
      });
    } catch (error) {
      console.error(
        "Failed to cancel match:",
        error
      );

      const responseData =
        error.response?.data;

      if (
        typeof responseData === "string"
      ) {
        setError(responseData);
      } else if (
        responseData?.message
      ) {
        setError(
          responseData.message
        );
      } else {
        setError(
          "Failed to cancel match."
        );
      }
    } finally {
      setLoading(false);
    }
  };

  // Purpose:
  // Create the editable match form.
  if (editing) {
    return (
      <div className="card mt-3">
        <div className="card-body">
          <h5 className="mb-4">
            Edit Match
          </h5>

          <div className="row">
            <div className="col-md-6 mb-3">
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

            <div className="col-md-6 mb-3">
              <label className="form-label">
                Team 1 Logo
              </label>

              <select
                name="team1Logo"
                className="form-select"
                value={formData.team1Logo}
                onChange={handleChange}
              >
                {Array.from(
                  { length: 10 },
                  (_, index) => {
                    const logoId =
                      index + 1;

                    return (
                      <option
                        key={logoId}
                        value={logoId}
                      >
                        Logo {logoId}
                      </option>
                    );
                  }
                )}
              </select>
            </div>
          </div>

          <div className="row">
            <div className="col-md-6 mb-3">
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

            <div className="col-md-6 mb-3">
              <label className="form-label">
                Team 2 Logo
              </label>

              <select
                name="team2Logo"
                className="form-select"
                value={formData.team2Logo}
                onChange={handleChange}
              >
                {Array.from(
                  { length: 10 },
                  (_, index) => {
                    const logoId =
                      index + 1;

                    return (
                      <option
                        key={logoId}
                        value={logoId}
                      >
                        Logo {logoId}
                      </option>
                    );
                  }
                )}
              </select>
            </div>
          </div>

          <div className="row">
            <div className="col-md-6 mb-3">
              <label className="form-label">
                Players per Team
              </label>

              <select
                name="playersPerTeam"
                className="form-select"
                value={formData.playersPerTeam}
                onChange={handleChange}
              >
                {Array.from(
                  { length: 8 },
                  (_, index) => {
                    const count =
                      index + 4;

                    return (
                      <option
                        key={count}
                        value={count}
                      >
                        {count}
                      </option>
                    );
                  }
                )}
              </select>
            </div>

            <div className="col-md-6 mb-3">
              <label className="form-label">
                Overs
              </label>

              <select
                name="overs"
                className="form-select"
                value={formData.overs}
                onChange={handleChange}
              >
                {Array.from(
                  { length: 90 },
                  (_, index) => {
                    const overs =
                      index + 1;

                    return (
                      <option
                        key={overs}
                        value={overs}
                      >
                        {overs} overs
                      </option>
                    );
                  }
                )}
              </select>
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
              <strong>
                📍 Location is locked
              </strong>

              <p className="mb-0 mt-1 text-muted">
                The exact match location will
                be captured automatically when
                you start the match.
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
              placeholder="https://youtube.com/..."
              value={formData.liveStreamUrl}
              onChange={handleChange}
            />
          </div>

          <hr />

          <MatchPlayerAssignment
            playersPerTeam={
              Number(formData.playersPerTeam)
            }
            team1Players={team1Players}
            team2Players={team2Players}
            setTeam1Players={
              setTeam1Players
            }
            setTeam2Players={
              setTeam2Players
            }
            initialTeam1Players={
              initialTeam1Players
            }

            initialTeam2Players={
              initialTeam2Players
            }
          />

          {error && (
            <div className="alert alert-danger mt-3">
              {error}
            </div>
          )}

          <div className="mt-4">
            <button
              type="button"
              className="btn btn-primary me-2"
              onClick={handleUpdate}
              disabled={loading}
            >
              {loading
                ? "Saving..."
                : "Save Changes"}
            </button>

            <button
              type="button"
              className="btn btn-secondary"
              onClick={() =>
                setEditing(false)
              }
              disabled={loading}
            >
              Cancel
            </button>
          </div>
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
            onClick={() => {
              setError("");
              setMessage("");
              setEditing(true);
            }}
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