import { useEffect, useState } from "react";
import {createPlayerProfile, getPlayerProfile, updatePlayerProfile} from "../../services/PlayerService";
  
function PlayerProfile() {

    const [loading, setLoading] = useState(true);
    const [message, setMessage] = useState("");
    const [errorMessage, setErrorMessage] = useState("");

    const [profileData, setProfileData] = useState({
    dateOfBirth: "",
    gender: "",
    battingStyle: "",
    bowlingStyle: "",
    playerRole: "",
    state: "",
    pinCode: ""
  });

  const [profileExists, setProfileExists] = useState(false);
  
 useEffect(() => {
  const loadProfile = async () => {
    try {
      const response = await getPlayerProfile();

      setProfileData({
        dateOfBirth: response.dateOfBirth.split("T")[0],
        gender: response.gender,
        battingStyle: response.battingStyle,
        bowlingStyle: response.bowlingStyle,
        playerRole: response.playerRole,
        state: response.state,
        pinCode: response.pinCode
      });

      setProfileExists(true);

    } catch (error) {

      if (error.response?.status === 404) {
        setProfileExists(false);
      } else {
        setErrorMessage("Failed to load player profile.");
      }

    } finally {
      setLoading(false);
    }
  };

  loadProfile();
}, []);

  const handleChange = (event) => {
    const { name, value } = event.target;

    setProfileData((previousData) => ({
      ...previousData,
      [name]: value
    }));
  };

 const handleSubmit = async (event) => {
  event.preventDefault();

  setMessage("");
  setErrorMessage("");

  try {
    let response;

    if (profileExists) {
      response = await updatePlayerProfile(profileData);
      setMessage("Player profile updated successfully.");
    } else {
      response = await createPlayerProfile(profileData);
      setProfileExists(true);
      setMessage("Player profile created successfully.");
    }

  } catch (error) {
    console.error("Failed to save player profile:", error);
    setErrorMessage(
      error.response?.data || "Failed to save player profile."
    );
  }
};

    const user = JSON.parse(localStorage.getItem("user"));

  return (
  <div className="container mt-4 mb-5">

    {/* Profile Header */}
    <div className="card shadow-sm mb-4">
      <div className="card-body">

        <div className="d-flex align-items-center">

          <img
            src={
              user?.profileImageUrl ||
              "https://via.placeholder.com/100"
            }
            alt="Profile"
            className="rounded-circle me-3"
            width="100"
            height="100"
          />

          <div>
            <h3 className="mb-1">
              {user?.firstName} {user?.lastName || ""}
            </h3>

            <span className="badge bg-primary">
              PLAYER
            </span>
          </div>

        </div>

      </div>
    </div>

    {/* Player Details */}
    <div className="card shadow-sm">

      <div className="card-body">

        <h4 className="mb-4">
          Player Details
        </h4>

        {loading && (
          <div className="text-center mb-3">
            Loading profile...
          </div>
        )}

        {message && (
          <div className="alert alert-success">
            {message}
          </div>
        )}

        {errorMessage && (
          <div className="alert alert-danger">
            {errorMessage}
          </div>
        )}

        {!loading && (
          <form onSubmit={handleSubmit}>

            <div className="row">

              {/* Date of Birth */}
              <div className="col-md-6 mb-3">
                <label className="form-label">
                  Date of Birth
                </label>

                <input
                  type="date"
                  className="form-control"
                  name="dateOfBirth"
                  value={profileData.dateOfBirth}
                  onChange={handleChange}
                  required
                />
              </div>

              {/* Gender */}
              <div className="col-md-6 mb-3">
                <label className="form-label">
                  Gender
                </label>

                <select
                  className="form-select"
                  name="gender"
                  value={profileData.gender}
                  onChange={handleChange}
                  required
                >
                  <option value="">Select Gender</option>
                  <option value="Male">Male</option>
                  <option value="Female">Female</option>
                  <option value="Other">Other</option>
                </select>
              </div>

              {/* Batting Style */}
              <div className="col-md-6 mb-3">
                <label className="form-label">
                  Batting Style
                </label>

                <select
                  className="form-select"
                  name="battingStyle"
                  value={profileData.battingStyle}
                  onChange={handleChange}
                  required
                >
                  <option value="">Select Batting Style</option>
                  <option value="Right Hand">Right Hand</option>
                  <option value="Left Hand">Left Hand</option>
                </select>
              </div>

              {/* Bowling Style */}
              <div className="col-md-6 mb-3">
                <label className="form-label">
                  Bowling Style
                </label>

                <select
                  className="form-select"
                  name="bowlingStyle"
                  value={profileData.bowlingStyle}
                  onChange={handleChange}
                  required
                >
                  <option value="">Select Bowling Style</option>
                  <option value="Right Arm Fast">Right Arm Fast</option>
                  <option value="Right Arm Medium">Right Arm Medium</option>
                  <option value="Left Arm Fast">Left Arm Fast</option>
                  <option value="Left Arm Medium">Left Arm Medium</option>
                  <option value="Right Arm Spin">Right Arm Spin</option>
                  <option value="Left Arm Spin">Left Arm Spin</option>
                  <option value="None">None</option>
                </select>
              </div>

              {/* Player Role */}
              <div className="col-md-6 mb-3">
                <label className="form-label">
                  Player Role
                </label>

                <select
                  className="form-select"
                  name="playerRole"
                  value={profileData.playerRole}
                  onChange={handleChange}
                  required
                >
                  <option value="">Select Player Role</option>
                  <option value="Batter">Batter</option>
                  <option value="Bowler">Bowler</option>
                  <option value="All Rounder">All Rounder</option>
                  <option value="Wicket Keeper">Wicket Keeper</option>
                </select>
              </div>

              {/* State */}
              <div className="col-md-6 mb-3">
                <label className="form-label">
                  State
                </label>

                <input
                  type="text"
                  className="form-control"
                  name="state"
                  value={profileData.state}
                  onChange={handleChange}
                  placeholder="Enter your state"
                  required
                />
              </div>

              {/* PIN Code */}
              <div className="col-md-6 mb-3">
                <label className="form-label">
                  PIN Code
                </label>

                <input
                  type="number"
                  className="form-control"
                  name="pinCode"
                  value={profileData.pinCode}
                  onChange={handleChange}
                  placeholder="Enter PIN code"
                  required
                />
              </div>

            </div>

            <div className="mt-3 text-center">

              <button
                type="submit"
                className="btn btn-primary"
              >
                {profileExists
                  ? "Update Profile"
                  : "Save Profile"}
              </button>

            </div>

          </form>
        )}

      </div>

    </div>

  </div>
);
}

export default PlayerProfile;