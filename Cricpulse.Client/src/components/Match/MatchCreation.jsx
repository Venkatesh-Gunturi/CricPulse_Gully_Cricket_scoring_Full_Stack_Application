import { useState } from "react";
import { createMatch } from "../../services/matchService";
import MatchPlayerAssignment from "./PlayerAssignment";
import { getCurrentLocation } from "../../services/LocationService";

const MatchCreation = () => {
  const [formData, setFormData] = useState({
    team1Name: "",
    team1Logo: "1",
    team2Name: "",
    team2Logo: "2",
    playersPerTeam: 11,
    overs: 20,
    matchDate: "",
    matchTime: "",
    venueName: "",
    address: "",
    latitude: "",
    longitude: "",
    liveStreamUrl: ""
  });

  const [team1Players, setTeam1Players] = useState([]);
  const [team2Players, setTeam2Players] = useState([]);

  const [locationLoading, setLocationLoading] = useState(false);
  const [locationError, setLocationError] = useState("");
  const [locationSelected, setLocationSelected] = useState(false);

  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  // Purpose: Update a match field when the umpire changes a value.
  const handleChange = (e) => {
    const { name, value } = e.target;

    setFormData((previousData) => ({
      ...previousData,
      [name]: value
    }));
  };

  // Purpose: Get the umpire's current coordinates for the match venue.
  const handleGetLocation = async () => {
    setLocationLoading(true);
    setLocationError("");

    try {
      const coordinates = await getCurrentLocation();

      setFormData((previousData) => ({
        ...previousData,
        latitude: coordinates.latitude,
        longitude: coordinates.longitude
      }));

      setLocationSelected(true);
    } catch (error) {
      console.error("Failed to get location:", error);

      setLocationError(
        "Unable to get your location. Please allow location access."
      );

      setLocationSelected(false);
    } finally {
      setLocationLoading(false);
    }
  };

  // Purpose: Validate the match form before sending the match to the backend.
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
      formData.latitude === "" ||
      formData.longitude === ""
    ) {
      return "Please select the match location.";
    }

    if (team1Players.length !== Number(formData.playersPerTeam)) {
      return `Team 1 must have exactly ${formData.playersPerTeam} verified players.`;
    }

    if (team2Players.length !== Number(formData.playersPerTeam)) {
      return `Team 2 must have exactly ${formData.playersPerTeam} verified players.`;
    }

    return null;
  };

  // Purpose: Create the match with its configuration and all verified player assignments.
  const handleSubmit = async () => {
    setLoading(true);
    setMessage("");
    setError("");

    try {
      const validationError = validateForm();

      if (validationError) {
        setError(validationError);
        return;
      }

      const players = [
        ...team1Players.map((player) => ({
          mobileNumber: player.mobileNumber,
          team: "Team1"
        })),

        ...team2Players.map((player) => ({
          mobileNumber: player.mobileNumber,
          team: "Team2"
        }))
      ];

      const matchData = {
        team1Name: formData.team1Name.trim(),
        team1Logo: formData.team1Logo,

        team2Name: formData.team2Name.trim(),
        team2Logo: formData.team2Logo,

        playersPerTeam: Number(formData.playersPerTeam),
        overs: Number(formData.overs),

        matchDate: formData.matchDate,
        matchTime: `${formData.matchTime}:00`,

        venueName: formData.venueName.trim(),
        address: formData.address.trim() || "",

        latitude: Number(formData.latitude),
        longitude: Number(formData.longitude),

        liveStreamUrl:
          formData.liveStreamUrl.trim() || null,

        players
      };

      const response = await createMatch(matchData);

      setMessage(
        `Match created successfully! Match ID: ${response.id}`
      );
    } catch (error) {
      console.error(error);

      setError(
        error.response?.data?.message ||
          error.response?.data ||
          "Failed to create match."
      );
    } finally {
      setLoading(false);
    }
  };

  const playersCount = Number(formData.playersPerTeam);

  return (
    <div
      style={{
        maxWidth: "1000px",
        margin: "0 auto",
        padding: "30px 20px"
      }}
    >
      <h2 style={{ textAlign: "center", marginBottom: "30px" }}>
        CREATE A MATCH
      </h2>

      {/* TEAM 1 */}
      <section style={{ marginBottom: "30px" }}>
        <h3>TEAM 1 DETAILS</h3>

        <div style={{ marginBottom: "15px" }}>
          <label>Team Name</label>

          <input
            type="text"
            name="team1Name"
            value={formData.team1Name}
            onChange={handleChange}
            placeholder="Enter Team 1 name"
            style={{ width: "100%" }}
          />
        </div>

        <div>
          <label>Team Logo</label>

          <select
            name="team1Logo"
            value={formData.team1Logo}
            onChange={handleChange}
            style={{ width: "100%" }}
          >
            {Array.from({ length: 10 }, (_, index) => index + 1).map((logo) => (
              <option key={logo} value={logo}>
                Logo {String(logo).padStart(2, "0")}
              </option>
            ))}
          </select>
        </div>
      </section>

      {/* TEAM 2 */}
      <section style={{ marginBottom: "30px" }}>
        <h3>TEAM 2 DETAILS</h3>

        <div style={{ marginBottom: "15px" }}>
          <label>Team Name</label>

          <input
            type="text"
            name="team2Name"
            value={formData.team2Name}
            onChange={handleChange}
            placeholder="Enter Team 2 name"
            style={{ width: "100%" }}
          />
        </div>

        <div>
          <label>Team Logo</label>

          <select
            name="team2Logo"
            value={formData.team1Logo}
            onChange={handleChange}
            style={{ width: "100%" }}
          >
            {Array.from({ length: 10 }, (_, index) => index + 1).map((logo) => (
              <option key={logo} value={logo}>
                Logo {String(logo).padStart(2, "0")}
              </option>
            ))}
          </select>
        </div>
      </section>

      {/* MATCH INFORMATION */}
      <section style={{ marginBottom: "30px" }}>
        <h3>MORE INFO ABOUT MATCH</h3>

        <div
          style={{
            display: "grid",
            gridTemplateColumns: "1fr 1fr",
            gap: "15px"
          }}
        >
          <div>
            <label>Match Date</label>

            <input
              type="date"
              name="matchDate"
              value={formData.matchDate}
              onChange={handleChange}
              style={{ width: "100%" }}
            />
          </div>

          <div>
            <label>Match Time</label>

            <input
              type="time"
              name="matchTime"
              value={formData.matchTime}
              onChange={handleChange}
              style={{ width: "100%" }}
            />
          </div>
        </div>

        <div style={{ marginTop: "15px" }}>
          <label>Venue</label>

          <input
            type="text"
            name="venueName"
            value={formData.venueName}
            onChange={handleChange}
            placeholder="Enter venue name"
            style={{ width: "100%" }}
          />
        </div>

        <div style={{ marginTop: "15px" }}>
          <label>Address</label>

          <textarea
            name="address"
            value={formData.address}
            onChange={handleChange}
            placeholder="Enter venue address"
            rows="3"
            style={{ width: "100%" }}
          />
        </div>

        <div style={{ marginTop: "15px" }}>
          <button
            type="button"
            onClick={handleGetLocation}
            disabled={locationLoading}
          >
            {locationLoading
              ? "Getting Location..."
              : "📍 Use My Current Location"}
          </button>

          {locationSelected && (
            <p style={{ color: "green" }}>
              ✓ Location selected
            </p>
          )}

          {locationError && (
            <p style={{ color: "red" }}>
              {locationError}
            </p>
          )}
        </div>
      </section>

      {/* MATCH TYPE */}
      <section style={{ marginBottom: "30px" }}>
        <h3>MATCH TYPE</h3>

        <div
          style={{
            display: "grid",
            gridTemplateColumns: "1fr 1fr",
            gap: "15px"
          }}
        >
          <div>
            <label>Overs</label>

            <select
              name="overs"
              value={formData.overs}
              onChange={handleChange}
              style={{ width: "100%" }}
            >
              {Array.from(
                { length: 90 },
                (_, index) => index + 1
              ).map((over) => (
                <option key={over} value={over}>
                  {over} {over === 1 ? "Over" : "Overs"}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label>Players Per Team</label>

            <select
              name="playersPerTeam"
              value={formData.playersPerTeam}
              onChange={handleChange}
              style={{ width: "100%" }}
            >
              {Array.from(
                { length: 8 },
                (_, index) => index + 4
              ).map((count) => (
                <option key={count} value={count}>
                  {count}
                </option>
              ))}
            </select>
          </div>
        </div>
      </section>

      {/* PLAYERS */}
      <section style={{ marginBottom: "30px" }}>
        <MatchPlayerAssignment
          playersPerTeam={playersCount}
          team1Players={team1Players}
          team2Players={team2Players}
          setTeam1Players={setTeam1Players}
          setTeam2Players={setTeam2Players}
        />
      </section>

      {/* LIVE STREAM */}
      <section style={{ marginBottom: "30px" }}>
        <h3>LIVE STREAM</h3>

        <label>Live Stream URL (Optional)</label>

        <input
          type="url"
          name="liveStreamUrl"
          value={formData.liveStreamUrl}
          onChange={handleChange}
          placeholder="https://..."
          style={{ width: "100%" }}
        />
      </section>

      {/* CREATE */}
      <section style={{ textAlign: "center" }}>
        <button
          type="button"
          onClick={handleSubmit}
          disabled={loading}
        >
          {loading ? "Creating Match..." : "CREATE MATCH"}
        </button>

        {message && (
          <p style={{ color: "green", marginTop: "15px" }}>
            {message}
          </p>
        )}

        {error && (
          <p style={{ color: "red", marginTop: "15px" }}>
            {error}
          </p>
        )}
      </section>
    </div>
  );
};

export default MatchCreation;