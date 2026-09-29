import { useEffect, useState } from "react";

import { getPlayerProfile } from "../services/playerService";

import Navbar from "../components/Layout/Navbar";

import "./PlayerProfile.css";

const PlayerProfile = ({
  loggedInUser,
  onLogin,
  onRegister,
  onMatches,
  onPlayerProfile,
  onUmpireDashboard,
  onCreateMatch,
  onLogout
}) => {
  const [profile, setProfile] =
    useState(null);

  const [activeSection, setActiveSection] =
    useState("batting");

  const [loading, setLoading] =
    useState(true);

  const [error, setError] =
    useState("");

  // ============================================================
  // LOAD PROFILE
  // ============================================================

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      setLoading(true);
      setError("");

      const data =
        await getPlayerProfile();

      setProfile(data);
    } catch (err) {
      console.error(
        "Failed to load player profile:",
        err
      );

      if (
        err.response?.status === 401
      ) {
        setError(
          "Please login to view your profile."
        );
      } else if (
        err.response?.status === 403
      ) {
        setError(
          "Only players can access this profile."
        );
      } else if (
        err.response?.status === 404
      ) {
        setError(
          "Player profile not found."
        );
      } else {
        setError(
          "Unable to load player profile."
        );
      }
    } finally {
      setLoading(false);
    }
  };

  // ============================================================
  // DATA
  // ============================================================

  const player =
    profile?.profile;

  const statistics =
    profile?.statistics || {};

  // ============================================================
  // PLAYER NAME
  // ============================================================

  const getPlayerName = () => {
    if (!player) {
      return "Player";
    }

    const firstName =
      player.firstName ||
      player.FirstName ||
      "";

    const lastName =
      player.lastName ||
      player.LastName ||
      "";

    return (
      `${firstName} ${lastName}`.trim() ||
      "Player"
    );
  };

  // ============================================================
  // INITIALS
  // ============================================================

  const getInitials = () => {
    const name =
      getPlayerName();

    if (
      !name ||
      name === "Player"
    ) {
      return "P";
    }

    const parts =
      name.trim().split(/\s+/);

    if (
      parts.length === 1
    ) {
      return parts[0]
        .charAt(0)
        .toUpperCase();
    }

    return (
      parts[0].charAt(0) +
      parts[
        parts.length - 1
      ].charAt(0)
    ).toUpperCase();
  };

  // ============================================================
  // AGE
  // ============================================================

  const calculateAge = (
    dateOfBirth
  ) => {
    if (!dateOfBirth) {
      return null;
    }

    const birthDate =
      new Date(dateOfBirth);

    if (
      Number.isNaN(
        birthDate.getTime()
      )
    ) {
      return null;
    }

    const today =
      new Date();

    let age =
      today.getFullYear() -
      birthDate.getFullYear();

    const monthDifference =
      today.getMonth() -
      birthDate.getMonth();

    if (
      monthDifference < 0 ||
      (
        monthDifference === 0 &&
        today.getDate() <
          birthDate.getDate()
      )
    ) {
      age--;
    }

    return age;
  };

  // ============================================================
  // DATE
  // ============================================================

  const formatDate = (
    dateValue
  ) => {
    if (!dateValue) {
      return "-";
    }

    const date =
      new Date(dateValue);

    if (
      Number.isNaN(
        date.getTime()
      )
    ) {
      return "-";
    }

    return date.toLocaleDateString(
      "en-IN",
      {
        day: "2-digit",
        month: "short",
        year: "numeric"
      }
    );
  };

  // ============================================================
  // SAFE VALUE
  // ============================================================

  const formatValue = (
    value
  ) => {
    if (
      value === null ||
      value === undefined ||
      value === ""
    ) {
      return "-";
    }

    return value;
  };

  // ============================================================
  // LOADING
  // ============================================================

  if (loading) {
    return (
      <>
        <Navbar
          onLogin={onLogin}
          onRegister={onRegister}
          loggedInUser={
            loggedInUser
          }
          onMatches={onMatches}
          onPlayerProfile={
            onPlayerProfile
          }
          onUmpireDashboard={
            onUmpireDashboard
          }
          onCreateMatch={
            onCreateMatch
          }
          onLogout={onLogout}
        />

        <main className="cp-player-profile-page">

          <div className="cp-player-profile-container">

            <div className="cp-profile-loading">

              <div className="cp-profile-loader">
                <span />
                <span />
                <span />
              </div>

              <h3>
                Loading your profile
              </h3>

              <p>
                Getting your cricket
                records ready...
              </p>

            </div>

          </div>

        </main>
      </>
    );
  }

  // ============================================================
  // ERROR
  // ============================================================

  if (error) {
    return (
      <>
        <Navbar
          onLogin={onLogin}
          onRegister={onRegister}
          loggedInUser={
            loggedInUser
          }
          onMatches={onMatches}
          onPlayerProfile={
            onPlayerProfile
          }
          onUmpireDashboard={
            onUmpireDashboard
          }
          onCreateMatch={
            onCreateMatch
          }
          onLogout={onLogout}
        />

        <main className="cp-player-profile-page">

          <div className="cp-player-profile-container">

            <div className="cp-profile-error">

              <div className="cp-profile-error-icon">
                !
              </div>

              <h2>
                Profile unavailable
              </h2>

              <p>
                {error}
              </p>

              <button
                type="button"
                className="cp-profile-primary-button"
                onClick={loadProfile}
              >
                Try Again
              </button>

            </div>

          </div>

        </main>
      </>
    );
  }

  // ============================================================
  // PROFILE NOT FOUND
  // ============================================================

  if (!player) {
    return (
      <>
        <Navbar
          onLogin={onLogin}
          onRegister={onRegister}
          loggedInUser={
            loggedInUser
          }
          onMatches={onMatches}
          onPlayerProfile={
            onPlayerProfile
          }
          onUmpireDashboard={
            onUmpireDashboard
          }
          onCreateMatch={
            onCreateMatch
          }
          onLogout={onLogout}
        />

        <main className="cp-player-profile-page">

          <div className="cp-player-profile-container">

            <div className="cp-profile-error">

              <div className="cp-profile-error-icon">
                !
              </div>

              <h2>
                Player profile not found
              </h2>

              <p>
                We couldn't find your
                player profile.
              </p>

            </div>

          </div>

        </main>
      </>
    );
  }

  // ============================================================
  // PROFILE DATA
  // ============================================================

  const playerName =
    getPlayerName();

  const profileImage =
    player.profileImageUrl ||
    player.ProfileImageUrl ||
    null;

  const mobileNumber =
    player.mobileNumber ||
    player.MobileNumber ||
    "";

  const email =
    player.email ||
    player.Email ||
    "";

  const age =
    calculateAge(
      player.dateOfBirth
    );

  const matchesPlayed =
    Number(
      statistics.matches ?? 0
    );

  const hasPlayedMatches =
    matchesPlayed > 0;

  // ============================================================
  // MAIN PROFILE
  // ============================================================

  return (
    <>
      {/* ======================================================
          NAVBAR
          ====================================================== */}

      <Navbar
        onLogin={onLogin}
        onRegister={onRegister}
        loggedInUser={
          loggedInUser
        }
        onMatches={onMatches}
        onPlayerProfile={
          onPlayerProfile
        }
        onUmpireDashboard={
          onUmpireDashboard
        }
        onCreateMatch={
          onCreateMatch
        }
        onLogout={onLogout}
      />

      <main className="cp-player-profile-page">

        <div className="cp-player-profile-container">

          {/* ==================================================
              PROFILE HERO
              ================================================== */}

          <section className="cp-player-profile-hero">

            <div className="cp-profile-hero-glow cp-glow-one" />

            <div className="cp-profile-hero-glow cp-glow-two" />

            <div className="cp-profile-avatar-wrapper">

              {profileImage ? (
                <img
                  src={profileImage}
                  alt={playerName}
                  className="cp-profile-avatar"
                />
              ) : (
                <div className="cp-profile-avatar cp-profile-avatar-placeholder">
                  {getInitials()}
                </div>
              )}

              <span className="cp-profile-player-badge">
                🏏
              </span>

            </div>

            <div className="cp-profile-hero-content">

              <span className="cp-profile-eyebrow">
                CRICPULSE PLAYER
              </span>

              <h1>
                {playerName}
              </h1>

              <div className="cp-profile-role">
                <span>
                  ●
                </span>

                Player
              </div>

              <div className="cp-profile-contact-row">

                {mobileNumber && (
                  <div className="cp-profile-contact">

                    <span className="cp-contact-icon">
                      ☎
                    </span>

                    <span>
                      {mobileNumber}
                    </span>

                  </div>
                )}

                {mobileNumber &&
                  email && (
                    <span className="cp-profile-contact-divider" />
                  )}

                {email && (
                  <div className="cp-profile-contact">

                    <span className="cp-contact-icon">
                      ✉
                    </span>

                    <span>
                      {email}
                    </span>

                  </div>
                )}

              </div>

            </div>

          </section>

          {/* ==================================================
              SUMMARY CARDS
              ================================================== */}

          <section className="cp-profile-summary-grid">

            <ProfileSummaryCard
              icon="🏏"
              label="Matches"
              value={
                statistics.matches
              }
              accent="orange"
            />

            <ProfileSummaryCard
              icon="🏃"
              label="Runs"
              value={
                statistics.runs
              }
              accent="green"
            />

            <ProfileSummaryCard
              icon="🎯"
              label="Wickets"
              value={
                statistics.wickets
              }
              accent="red"
            />

            <ProfileSummaryCard
              icon="🏆"
              label="MVP Awards"
              value={
                statistics.mvpCount
              }
              accent="gold"
            />

          </section>

          {/* ==================================================
              PERSONAL INFORMATION
              ================================================== */}

          <section className="cp-profile-section">

            <div className="cp-profile-section-heading">

              <div>

                <span className="cp-profile-section-eyebrow">
                  PLAYER DETAILS
                </span>

                <h2>
                  Personal Information
                </h2>

                <p>
                  Your registered player
                  information on CricPulse.
                </p>

              </div>

            </div>

            <div className="cp-personal-info-card">

              <div className="cp-personal-info-column">

                <ProfileDetail
                  label="Age"
                  value={
                    age !== null
                      ? `${age} years`
                      : "-"
                  }
                />

                <ProfileDetail
                  label="Gender"
                  value={
                    player.gender
                  }
                />

                <ProfileDetail
                  label="Player Role"
                  value={
                    player.playerRole
                  }
                />

                <ProfileDetail
                  label="Batting Style"
                  value={
                    player.battingStyle
                  }
                />

              </div>

              <div className="cp-personal-info-column">

                <ProfileDetail
                  label="Bowling Style"
                  value={
                    player.bowlingStyle
                  }
                />

                <ProfileDetail
                  label="State"
                  value={
                    player.state
                  }
                />

                <ProfileDetail
                  label="PIN Code"
                  value={
                    player.pinCode
                  }
                />

                <ProfileDetail
                  label="Date of Birth"
                  value={
                    formatDate(
                      player.dateOfBirth
                    )
                  }
                />

              </div>

            </div>

          </section>

          {/* ==================================================
              ZERO MATCH STATE
              ================================================== */}

          {!hasPlayedMatches ? (
            <section className="cp-profile-section">

              <div
                style={{
                  background:
                    "linear-gradient(135deg, #fffaf3 0%, #ffffff 55%, #f7fbff 100%)",
                  border:
                    "1px solid rgba(0,0,0,0.07)",
                  borderRadius:
                    "24px",
                  padding:
                    "56px 28px",
                  textAlign:
                    "center",
                  boxShadow:
                    "0 18px 50px rgba(0,0,0,0.06)"
                }}
              >

                <div
                  style={{
                    width: "76px",
                    height: "76px",
                    margin:
                      "0 auto 20px",
                    borderRadius:
                      "50%",
                    display:
                      "flex",
                    alignItems:
                      "center",
                    justifyContent:
                      "center",
                    background:
                      "rgba(245, 158, 11, 0.12)",
                    fontSize:
                      "34px"
                  }}
                >
                  🏏
                </div>

                <span className="cp-profile-section-eyebrow">
                  YOUR CRICKET JOURNEY
                </span>

                <h2
                  style={{
                    marginTop:
                      "10px"
                  }}
                >
                  No matches played yet
                </h2>

                <p
                  style={{
                    marginBottom:
                      "8px",
                    fontSize:
                      "17px",
                    fontWeight:
                      "600"
                  }}
                >
                  Your CricPulse record
                  is ready to begin.
                </p>

                <p
                  style={{
                    maxWidth:
                      "650px",
                    margin:
                      "0 auto",
                    color:
                      "#6b7280",
                    lineHeight:
                      "1.7"
                  }}
                >
                  Join local matches
                  organized by umpires
                  on CricPulse and
                  start building your
                  cricket journey.
                </p>

                <div
                  style={{
                    maxWidth:
                      "680px",
                    margin:
                      "36px auto 0",
                    padding:
                      "24px",
                    borderRadius:
                      "18px",
                    background:
                      "rgba(255,255,255,0.8)",
                    border:
                      "1px solid rgba(0,0,0,0.06)"
                  }}
                >

                  <strong
                    style={{
                      display:
                        "block",
                      marginBottom:
                        "8px"
                    }}
                  >
                    Your cricket journey
                    starts with your
                    first match. 🏏
                  </strong>

                  <span
                    style={{
                      color:
                        "#6b7280",
                      lineHeight:
                        "1.6"
                    }}
                  >
                    Join a local game
                    organized by a
                    CricPulse umpire,
                    and your performances
                    will appear here
                    automatically.
                  </span>

                </div>

              </div>

            </section>
          ) : (
            /* ==================================================
               PLAYER STATISTICS
               ================================================== */

            <section className="cp-profile-section">

              <div className="cp-profile-section-heading cp-stats-heading">

                <div>

                  <span className="cp-profile-section-eyebrow">
                    CAREER RECORDS
                  </span>

                  <h2>
                    Player Statistics
                  </h2>

                  <p>
                    Track your batting and
                    bowling performance.
                  </p>

                </div>

                <div className="cp-stats-toggle">

                  <button
                    type="button"
                    className={
                      activeSection ===
                      "batting"
                        ? "active"
                        : ""
                    }
                    onClick={() =>
                      setActiveSection(
                        "batting"
                      )
                    }
                  >
                    🏏 Batting
                  </button>

                  <button
                    type="button"
                    className={
                      activeSection ===
                      "bowling"
                        ? "active"
                        : ""
                    }
                    onClick={() =>
                      setActiveSection(
                        "bowling"
                      )
                    }
                  >
                    🎯 Bowling
                  </button>

                </div>

              </div>

              {/* ==================================================
                  BATTING
                  ================================================== */}

              {activeSection ===
                "batting" && (
                <div className="cp-stat-panel">

                  <div className="cp-stat-panel-header">

                    <div className="cp-stat-panel-icon">
                      🏏
                    </div>

                    <div>

                      <h3>
                        Batting Records
                      </h3>

                      <p>
                        Your career batting
                        performance
                      </p>

                    </div>

                  </div>

                  <div className="cp-stat-grid">

                    <StatCard
                      title="Matches"
                      value={
                        statistics.matches
                      }
                      icon="🏏"
                    />

                    <StatCard
                      title="Innings"
                      value={
                        statistics.battingInnings
                      }
                      icon="📋"
                    />

                    <StatCard
                      title="Runs"
                      value={
                        statistics.runs
                      }
                      icon="⚡"
                      highlight
                    />

                    <StatCard
                      title="Balls Faced"
                      value={
                        statistics.ballsFaced
                      }
                      icon="🔵"
                    />

                    <StatCard
                      title="4s"
                      value={
                        statistics.fours
                      }
                      icon="4️⃣"
                    />

                    <StatCard
                      title="6s"
                      value={
                        statistics.sixes
                      }
                      icon="6️⃣"
                    />

                    <StatCard
                      title="50s"
                      value={
                        statistics.fifties
                      }
                      icon="5️⃣"
                    />

                    <StatCard
                      title="100s"
                      value={
                        statistics.hundreds
                      }
                      icon="💯"
                    />

                    <StatCard
                      title="Highest Score"
                      value={
                        statistics.highestScore
                      }
                      icon="🔥"
                      highlight
                    />

                    <StatCard
                      title="Strike Rate"
                      value={
                        statistics.strikeRate !==
                          null &&
                        statistics.strikeRate !==
                          undefined
                          ? Number(
                              statistics.strikeRate
                            ).toFixed(2)
                          : 0
                      }
                      icon="📈"
                    />

                    <StatCard
                      title="Batting Average"
                      value={
                        statistics.battingAverage !==
                          null &&
                        statistics.battingAverage !==
                          undefined
                          ? Number(
                              statistics.battingAverage
                            ).toFixed(2)
                          : 0
                      }
                      icon="📊"
                    />

                  </div>

                </div>
              )}

              {/* ==================================================
                  BOWLING
                  ================================================== */}

              {activeSection ===
                "bowling" && (
                <div className="cp-stat-panel">

                  <div className="cp-stat-panel-header">

                    <div className="cp-stat-panel-icon bowling">
                      🎯
                    </div>

                    <div>

                      <h3>
                        Bowling Records
                      </h3>

                      <p>
                        Your career bowling
                        performance
                      </p>

                    </div>

                  </div>

                  <div className="cp-stat-grid">

                    <StatCard
                      title="Innings"
                      value={
                        statistics.bowlingInnings
                      }
                      icon="📋"
                    />

                    <StatCard
                      title="Balls Bowled"
                      value={
                        statistics.ballsBowled
                      }
                      icon="🔵"
                    />

                    <StatCard
                      title="Runs Conceded"
                      value={
                        statistics.runsConceded
                      }
                      icon="🏃"
                    />

                    <StatCard
                      title="Wickets"
                      value={
                        statistics.wickets
                      }
                      icon="🎯"
                      highlight
                    />

                    <StatCard
                      title="Maiden Overs"
                      value={
                        statistics.maidenOvers
                      }
                      icon="🛡️"
                    />

                    <StatCard
                      title="Economy"
                      value={
                        statistics.economy !==
                          null &&
                        statistics.economy !==
                          undefined
                          ? Number(
                              statistics.economy
                            ).toFixed(2)
                          : 0
                      }
                      icon="📉"
                    />

                  </div>

                </div>
              )}

            </section>
          )}

          {/* ==================================================
              MVP
              ================================================== */}

          <section className="cp-mvp-section">

            <div className="cp-mvp-content">

              <span className="cp-mvp-eyebrow">
                🏆 MATCH PERFORMANCE
              </span>

              <h2>
                Most Valuable Player
              </h2>

              <p>
                Your MVP recognition
                across CricPulse matches.
              </p>

              <div className="cp-mvp-count">

                <strong>
                  {statistics.mvpCount ??
                    0}
                </strong>

                <span>
                  MVP Awards
                </span>

              </div>

            </div>

            <div className="cp-mvp-trophy">
              🏆
            </div>

            <div className="cp-mvp-glow" />

          </section>

        </div>

      </main>
    </>
  );
};

/* =========================================================
   PROFILE SUMMARY CARD
========================================================= */

const ProfileSummaryCard = ({
  icon,
  label,
  value,
  accent
}) => {
  return (
    <div
      className={`cp-profile-summary-card cp-accent-${accent}`}
    >

      <div className="cp-summary-icon">
        {icon}
      </div>

      <div>

        <span>
          {label}
        </span>

        <strong>
          {value ?? 0}
        </strong>

      </div>

    </div>
  );
};

/* =========================================================
   PROFILE DETAIL
========================================================= */

const ProfileDetail = ({
  label,
  value
}) => {
  return (
    <div className="cp-profile-detail-row">

      <span className="cp-profile-detail-label">
        {label}
      </span>

      <strong className="cp-profile-detail-value">
        {value === null ||
        value === undefined ||
        value === ""
          ? "-"
          : value}
      </strong>

    </div>
  );
};

/* =========================================================
   STAT CARD
========================================================= */

const StatCard = ({
  title,
  value,
  icon,
  highlight = false
}) => {
  return (
    <div
      className={`cp-stat-card ${
        highlight
          ? "highlight"
          : ""
      }`}
    >

      <div className="cp-stat-card-icon">
        {icon}
      </div>

      <div className="cp-stat-card-content">

        <span>
          {title}
        </span>

        <strong>
          {value ?? 0}
        </strong>

      </div>

    </div>
  );
};

export default PlayerProfile;