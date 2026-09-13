import { useEffect, useState } from "react";

import MatchList from "../components/Match/MatchList";

import {
  getCurrentLocation,
  getStateByLocation
} from "../services/LocationService";

const indianStates = [
  "Andaman and Nicobar Islands",
  "Andhra Pradesh",
  "Arunachal Pradesh",
  "Assam",
  "Bihar",
  "Chandigarh",
  "Chhattisgarh",
  "Dadra and Nagar Haveli and Daman and Diu",
  "Delhi",
  "Goa",
  "Gujarat",
  "Haryana",
  "Himachal Pradesh",
  "Jammu and Kashmir",
  "Jharkhand",
  "Karnataka",
  "Kerala",
  "Ladakh",
  "Lakshadweep",
  "Madhya Pradesh",
  "Maharashtra",
  "Manipur",
  "Meghalaya",
  "Mizoram",
  "Nagaland",
  "Odisha",
  "Puducherry",
  "Punjab",
  "Rajasthan",
  "Sikkim",
  "Tamil Nadu",
  "Telangana",
  "Tripura",
  "Uttar Pradesh",
  "Uttarakhand",
  "West Bengal"
];

function MainPage({
  onLogin,
  onRegister,
  loggedInUser,
  onCreateMatch
}) {
  const [selectedStatus, setSelectedStatus] =
    useState("Scheduled");

  const [selectedState, setSelectedState] =
    useState("Telangana");

  const [location, setLocation] =
    useState(null);

  const [locationError, setLocationError] =
    useState("");

  useEffect(() => {
    const loadLocation = async () => {
      try {
        const coordinates =
          await getCurrentLocation();

        setLocation(coordinates);

        console.log(
          "User location:",
          coordinates
        );

        const detectedState =
          await getStateByLocation(
            coordinates.latitude,
            coordinates.longitude
          );

        setSelectedState(detectedState);

        console.log(
          "Detected state:",
          detectedState
        );
      } catch (error) {
        console.log(
          "Location permission denied or unavailable."
        );

        setLocationError(
          "Location unavailable."
        );
      }
    };

    loadLocation();
  }, []);

  return (
    <>
      <nav className="navbar navbar-dark bg-dark">
        <div className="container">
          <span className="navbar-brand mb-0 h1">
            CricPulse 🏏
          </span>

          <div>
            {loggedInUser ? (
              <button
                className="btn btn-warning"
                onClick={onCreateMatch}
              >
                🏏 Create Match
              </button>
            ) : (
              <>
                <button
                  className="btn btn-outline-light me-2"
                  onClick={onLogin}
                >
                  Login
                </button>

                <button
                  className="btn btn-primary"
                  onClick={onRegister}
                >
                  Register
                </button>
              </>
              )}
            </div>
        </div>
      </nav>

      <section className="container text-center mt-5">
        <h1>
          Your Local Cricket. One Pulse. 🏏
        </h1>

        <p className="text-muted">
          Discover nearby matches, follow live
          scores, and stay connected with cricket
          around you.
        </p>
      </section>

      <section className="container mt-4">
        <div className="d-flex justify-content-center gap-2 flex-wrap">
          <button
            className={
              selectedStatus === "Live"
                ? "btn btn-danger"
                : "btn btn-outline-danger"
            }
            onClick={() =>
              setSelectedStatus("Live")
            }
          >
            Live
          </button>

          <button
            className={
              selectedStatus === "Scheduled"
                ? "btn btn-primary"
                : "btn btn-outline-primary"
            }
            onClick={() =>
              setSelectedStatus("Scheduled")
            }
          >
            Upcoming
          </button>

          <button
            className={
              selectedStatus === "Completed"
                ? "btn btn-secondary"
                : "btn btn-outline-secondary"
            }
            onClick={() =>
              setSelectedStatus("Completed")
            }
          >
            Finished
          </button>

          <button
            className={
              selectedStatus === "Cancelled"
                ? "btn btn-dark"
                : "btn btn-outline-dark"
            }
            onClick={() =>
              setSelectedStatus("Cancelled")
            }
          >
            Cancelled
          </button>
        </div>
      </section>

      <section className="container mt-3 text-center">
        <select
          className="form-select d-inline-block"
          style={{ width: "280px" }}
          value={selectedState}
          onChange={(event) =>
            setSelectedState(event.target.value)
          }
        >
          {indianStates.map((state) => (
            <option
              key={state}
              value={state}
            >
              {state}
            </option>
          ))}
        </select>

        {locationError && (
          <p className="text-muted mt-2">
            {locationError}
          </p>
        )}
      </section>

      <MatchList
        status={selectedStatus}
        location={location}
        state={selectedState}
      />
    </>
  );
}

export default MainPage;