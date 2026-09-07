import { useState } from "react";
import { createMatch } from "../../services/matchService";
import { getCurrentLocation } from "../../services/LocationService";

const MatchCreation = () => {
  const [locationLoading, setLocationLoading] = useState(false);
const [locationError, setLocationError] = useState("");
const [locationSelected, setLocationSelected] = useState(false);
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState("");
    const [error, setError] = useState(""); 
  const [formData, setFormData] = useState({
    team1Name: "",
    team1Logo: "logo_01",
    team2Name: "",
    team2Logo: "logo_02",
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

//Match location handler
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

  const handleChange = (e) => {
    const { name, value } = e.target;

    setFormData((prev) => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSubmit = async () => {
  setLoading(true);
  setMessage("");
  setError("");

  try {
    const matchData = {
      ...formData,
      playersPerTeam: Number(formData.playersPerTeam),
      overs: Number(formData.overs),
      latitude: Number(formData.latitude),
      longitude: Number(formData.longitude),
      matchTime: `${formData.matchTime}:00`,
      liveStreamUrl: formData.liveStreamUrl || null
    };

    const response = await createMatch(matchData);

    setMessage(`Match created successfully! Match ID: ${response.id}`);
  } catch (error) {
    console.error(error);

    setError(
      error.response?.data?.message ||
      "Failed to create match."
    );
  } finally {
    setLoading(false);
  }
};

  return (
    <div>
      <h2>Create Match</h2>

      <div>
        <label>Team 1 Name</label>
        <input
          type="text"
          name="team1Name"
          value={formData.team1Name}
          onChange={handleChange}
        />
      </div>

      <div>
        <label>Team 1 Logo</label>
        <select
          name="team1Logo"
          value={formData.team1Logo}
          onChange={handleChange}
        >
          <option value="logo_01">Logo 01</option>
          <option value="logo_02">Logo 02</option>
          <option value="logo_03">Logo 03</option>
          <option value="logo_04">Logo 04</option>
          <option value="logo_05">Logo 05</option>
        </select>
      </div>

      <div>
        <label>Team 2 Name</label>
        <input
          type="text"
          name="team2Name"
          value={formData.team2Name}
          onChange={handleChange}
        />
      </div>

      <div>
        <label>Team 2 Logo</label>
        <select
          name="team2Logo"
          value={formData.team2Logo}
          onChange={handleChange}
        >
          <option value="logo_01">Logo 01</option>
          <option value="logo_02">Logo 02</option>
          <option value="logo_03">Logo 03</option>
          <option value="logo_04">Logo 04</option>
          <option value="logo_05">Logo 05</option>
        </select>
      </div>

      <div>
        <label>Players Per Team</label>
        <input
          type="number"
          name="playersPerTeam"
          min="1"
          value={formData.playersPerTeam}
          onChange={handleChange}
        />
      </div>

      <div>
        <label>Overs</label>
        <select
          name="overs"
          value={formData.overs}
          onChange={handleChange}
        >
          {Array.from({ length: 90 }, (_, index) => index + 1).map(
            (over) => (
              <option key={over} value={over}>
                {over} {over === 1 ? "Over" : "Overs"}
              </option>
            )
          )}
        </select>
      </div>

      <div>
        <label>Match Date</label>
        <input
          type="date"
          name="matchDate"
          value={formData.matchDate}
          onChange={handleChange}
        />
      </div>

      <div>
        <label>Match Time</label>
        <input
          type="time"
          name="matchTime"
          value={formData.matchTime}
          onChange={handleChange}
        />
      </div>

      <div>
        <label>Venue Name</label>
        <input
          type="text"
          name="venueName"
          value={formData.venueName}
          onChange={handleChange}
        />
      </div>

      <div>
        <label>Address</label>
        <textarea
          name="address"
          value={formData.address}
          onChange={handleChange}
        />
      </div>

     
      <div className="mb-3">
  <label className="form-label">
    Match Location
  </label>

    <button
      type="button"
      className="btn btn-outline-primary"
      onClick={handleGetLocation}
      disabled={locationLoading}
    >
      {locationLoading
        ? "Getting Location..."
        : "📍 Use My Current Location"}
    </button>

    {locationSelected && (
      <p className="text-success mt-2">
        ✓ Location selected
      </p>
    )}

    {locationError && (
      <p className="text-danger mt-2">
        {locationError}
      </p>
    )}
  </div>
        

      <div>
        <label>Live Stream URL (Optional)</label>
        <input
          type="url"
          name="liveStreamUrl"
          value={formData.liveStreamUrl}
          onChange={handleChange}
        />
      </div>

       <button type="button" onClick={handleSubmit} disabled={loading}>
          {loading ? "Creating..." : "Create Match"}
        </button>       
        {message && <p>{message}</p>}

        {error && <p>{error}</p>} 
    </div>
  );
};

export default MatchCreation;