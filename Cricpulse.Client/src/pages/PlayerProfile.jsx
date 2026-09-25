import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api";

const PlayerProfile = () => {
  const navigate = useNavigate();

  const [profile, setProfile] = useState(null);
  const [activeSection, setActiveSection] = useState("batting");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      setLoading(true);
      setError("");

      const response = await api.get("/Player/profile");

      setProfile(response.data);
    } catch (err) {
      console.error("Failed to load player profile:", err);

      if (err.response?.status === 401) {
        setError("Please login to view your profile.");
      } else if (err.response?.status === 403) {
        setError("Only players can access this profile.");
      } else if (err.response?.status === 404) {
        setError("Player profile not found.");
      } else {
        setError("Unable to load player profile.");
      }
    } finally {
      setLoading(false);
    }
  };

  const getPlayerName = () => {
    if (!profile?.profile) {
      return "";
    }

    const firstName = profile.profile.firstName || "";
    const lastName = profile.profile.lastName || "";

    return `${firstName} ${lastName}`.trim();
  };

  const getInitials = () => {
    const name = getPlayerName();

    if (!name) {
      return "P";
    }

    const parts = name.split(" ");

    if (parts.length === 1) {
      return parts[0].charAt(0).toUpperCase();
    }

    return (
      parts[0].charAt(0) +
      parts[parts.length - 1].charAt(0)
    ).toUpperCase();
  };

  if (loading) {
    return (
      <div className="container py-5 text-center">
        <div
          className="spinner-border"
          role="status"
        >
          <span className="visually-hidden">
            Loading...
          </span>
        </div>

        <p className="mt-3">
          Loading profile...
        </p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container py-5">
        <button
          className="btn btn-outline-secondary mb-4"
          onClick={() => navigate(-1)}
        >
          ← Back
        </button>

        <div className="alert alert-danger">
          {error}
        </div>
      </div>
    );
  }

  const player = profile.profile;
  const statistics = profile.statistics;

  return (
    <div className="container py-4">

      {/* Back Button */}
      <button
        className="btn btn-outline-secondary mb-4"
        onClick={() => navigate(-1)}
      >
        ← Back
      </button>

      {/* Page Heading */}
      <h2 className="fw-bold mb-4">
        Player Profile
      </h2>

      {/* Player Header */}
      <div className="text-center mb-5">

        {player.profileImageUrl ? (
          <img
            src={player.profileImageUrl}
            alt={getPlayerName()}
            className="rounded-circle border shadow-sm"
            style={{
              width: "150px",
              height: "150px",
              objectFit: "cover"
            }}
          />
        ) : (
          <div
            className="rounded-circle border shadow-sm mx-auto d-flex align-items-center justify-content-center"
            style={{
              width: "150px",
              height: "150px",
              fontSize: "48px",
              fontWeight: "600"
            }}
          >
            {getInitials()}
          </div>
        )}

        <h3 className="mt-3 mb-0 fw-bold">
          {getPlayerName()}
        </h3>
      </div>

      {/* Player Details */}
      <section className="mb-5">

        <h4 className="fw-bold mb-3">
          Player Details
        </h4>

        <div className="card shadow-sm border-0">
          <div className="card-body">

            <div className="row g-4">

              <div className="col-md-6">
                <small className="text-muted">
                  Date of Birth
                </small>

                <div className="fw-semibold">
                  {new Date(
                    player.dateOfBirth
                  ).toLocaleDateString()}
                </div>
              </div>

              <div className="col-md-6">
                <small className="text-muted">
                  Gender
                </small>

                <div className="fw-semibold">
                  {player.gender || "-"}
                </div>
              </div>

              <div className="col-md-6">
                <small className="text-muted">
                  Batting Style
                </small>

                <div className="fw-semibold">
                  {player.battingStyle || "-"}
                </div>
              </div>

              <div className="col-md-6">
                <small className="text-muted">
                  Bowling Style
                </small>

                <div className="fw-semibold">
                  {player.bowlingStyle || "-"}
                </div>
              </div>

              <div className="col-md-6">
                <small className="text-muted">
                  Player Role
                </small>

                <div className="fw-semibold">
                  {player.playerRole || "-"}
                </div>
              </div>

              <div className="col-md-6">
                <small className="text-muted">
                  State
                </small>

                <div className="fw-semibold">
                  {player.state || "-"}
                </div>
              </div>

              <div className="col-md-6">
                <small className="text-muted">
                  Pin Code
                </small>

                <div className="fw-semibold">
                  {player.pinCode || "-"}
                </div>
              </div>

            </div>

          </div>
        </div>

      </section>

      {/* Career Statistics */}
      <section className="mb-5">

        <div className="d-flex justify-content-center mb-4">

          <div
            className="btn-group"
            role="group"
            aria-label="Player statistics"
          >

            <button
              type="button"
              className={`btn ${
                activeSection === "batting"
                  ? "btn-primary"
                  : "btn-outline-primary"
              }`}
              onClick={() =>
                setActiveSection("batting")
              }
            >
              Batting
            </button>

            <button
              type="button"
              className={`btn ${
                activeSection === "bowling"
                  ? "btn-primary"
                  : "btn-outline-primary"
              }`}
              onClick={() =>
                setActiveSection("bowling")
              }
            >
              Bowling
            </button>

          </div>

        </div>

        {/* Batting */}
        {activeSection === "batting" && (
          <div>

            <h4 className="fw-bold mb-3">
              Batting
            </h4>

            <div className="row g-3">

              <StatCard
                title="Matches"
                value={statistics.matches}
              />

              <StatCard
                title="Innings"
                value={statistics.battingInnings}
              />

              <StatCard
                title="Runs"
                value={statistics.runs}
              />

              <StatCard
                title="Balls Faced"
                value={statistics.ballsFaced}
              />

              <StatCard
                title="4s"
                value={statistics.fours}
              />

              <StatCard
                title="6s"
                value={statistics.sixes}
              />

              <StatCard
                title="50s"
                value={statistics.fifties}
              />

              <StatCard
                title="100s"
                value={statistics.hundreds}
              />

              <StatCard
                title="Highest Score"
                value={statistics.highestScore}
              />

              <StatCard
                title="Strike Rate"
                value={
                  statistics.strikeRate?.toFixed(2)
                }
              />

              <StatCard
                title="Batting Average"
                value={
                  statistics.battingAverage?.toFixed(2)
                }
              />

            </div>

          </div>
        )}

        {/* Bowling */}
        {activeSection === "bowling" && (
          <div>

            <h4 className="fw-bold mb-3">
              Bowling
            </h4>

            <div className="row g-3">

              <StatCard
                title="Innings"
                value={statistics.bowlingInnings}
              />

              <StatCard
                title="Balls Bowled"
                value={statistics.ballsBowled}
              />

              <StatCard
                title="Runs Conceded"
                value={statistics.runsConceded}
              />

              <StatCard
                title="Wickets"
                value={statistics.wickets}
              />

              <StatCard
                title="Maiden Overs"
                value={statistics.maidenOvers}
              />

              <StatCard
                title="Economy"
                value={
                  statistics.economy?.toFixed(2)
                }
              />

            </div>

          </div>
        )}

      </section>

      {/* MVP */}
      <section className="mb-4">

        <h4 className="fw-bold mb-3">
          MVP Awards
        </h4>

        <div className="card shadow-sm border-0 text-center">
          <div className="card-body py-4">

            <div
              style={{
                fontSize: "48px"
              }}
            >
              🏆
            </div>

            <h3 className="fw-bold mb-1">
              {statistics.mvpCount}
            </h3>

            <p className="text-muted mb-0">
              MVP Awards
            </p>

          </div>
        </div>

      </section>

    </div>
  );
};

const StatCard = ({ title, value }) => {
  return (
    <div className="col-6 col-md-4 col-lg-3">

      <div className="card h-100 shadow-sm border-0">

        <div className="card-body text-center">

          <div className="text-muted small mb-1">
            {title}
          </div>

          <div className="fs-4 fw-bold">
            {value ?? 0}
          </div>

        </div>

      </div>

    </div>
  );
};

export default PlayerProfile;