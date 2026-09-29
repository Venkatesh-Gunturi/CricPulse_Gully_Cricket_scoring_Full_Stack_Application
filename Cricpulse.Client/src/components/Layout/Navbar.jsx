import { useState } from "react";
import "./Navbar.css";

function Navbar({
  onLogin,
  onRegister,
  loggedInUser,
  appMode = "normal",
  onMatches,
  onPlayerProfile,
  onUmpireDashboard,
  onBackToPlayerMode,
  onAbout,
  onLogout,
}) {
  const [showRoleMenu, setShowRoleMenu] = useState(false);

  // ============================================================
  // AUTH / ROLE
  // ============================================================

  const isLoggedIn = !!loggedInUser;

  const accountIsUmpire =
    loggedInUser?.isUmpire === true ||
    loggedInUser?.role?.toLowerCase() === "umpire";

  const isUmpireMode =
    isLoggedIn &&
    accountIsUmpire &&
    appMode === "umpire";

  const isPlayerMode =
    isLoggedIn &&
    !isUmpireMode;

  const activeRole = !isLoggedIn
    ? "User"
    : isUmpireMode
      ? "Umpire"
      : "Player";

  // ============================================================
  // USER DETAILS
  // ============================================================

  const firstName =
    loggedInUser?.firstName ||
    loggedInUser?.FirstName ||
    "";

  const lastName =
    loggedInUser?.lastName ||
    loggedInUser?.LastName ||
    "";

  const displayName =
    `${firstName} ${lastName}`.trim() ||
    loggedInUser?.name ||
    activeRole;

  const profileImage =
    loggedInUser?.profileImageUrl ||
    loggedInUser?.ProfileImageUrl ||
    loggedInUser?.profilePicture ||
    null;

  const mobileNumber =
    loggedInUser?.mobileNumber ||
    loggedInUser?.MobileNumber ||
    "";

  const email =
    loggedInUser?.email ||
    loggedInUser?.Email ||
    "";

  // ============================================================
  // NAVIGATION
  // ============================================================

  const closeRoleMenu = () => {
    setShowRoleMenu(false);
  };

  const goHome = () => {
    closeRoleMenu();

    if (isUmpireMode) {
      onBackToPlayerMode?.();
      return;
    }

    window.location.hash = "";

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  const goToMatches = () => {
    closeRoleMenu();
    onMatches?.();
  };

  const goToAbout = () => {
    closeRoleMenu();
    onAbout?.();
  };

  const goToProfile = () => {
    closeRoleMenu();
    onPlayerProfile?.();
  };

  const goToUmpireDashboard = () => {
    closeRoleMenu();
    onUmpireDashboard?.();
  };

  const goBackToPlayerMode = () => {
    closeRoleMenu();
    onBackToPlayerMode?.();
  };

  // ============================================================
  // LOGOUT
  // ============================================================

  const handleLogout = () => {
    closeRoleMenu();
    onLogout?.();
  };

  // ============================================================
  // RENDER
  // ============================================================

  return (
    <header className="cp-navbar">
      <div className="cp-navbar-inner">

        {/* ======================================================
            BRAND
            ====================================================== */}

        <button
          type="button"
          className="cp-brand"
          onClick={goHome}
        >
          <span className="cp-brand-ball">
            🏏
          </span>

          <span className="cp-brand-text">
            Cric<span>Pulse</span>
          </span>
        </button>

        {/* ======================================================
            NAV LINKS
            ====================================================== */}

        <nav className="cp-nav-links">

          {/* HOME */}

          <button
            type="button"
            className="cp-nav-link active"
            onClick={goHome}
          >
            Home
          </button>

          {/* MATCHES */}

          <button
            type="button"
            className="cp-nav-link"
            onClick={goToMatches}
          >
            Matches
          </button>

          {/* ABOUT */}

          <button
            type="button"
            className="cp-nav-link"
            onClick={goToAbout}
          >
            About
          </button>

          {/* REGISTER / LOGIN */}

          {!isLoggedIn && (
            <>
              <button
                type="button"
                className="cp-nav-link"
                onClick={onRegister}
              >
                Register
              </button>

              <button
                type="button"
                className="cp-nav-link"
                onClick={onLogin}
              >
                Login
              </button>
            </>
          )}
        </nav>

        {/* ======================================================
            ROLE CAPSULE
            ====================================================== */}

        <div className="cp-role-wrapper">

          <button
            type="button"
            className={`cp-role-capsule ${
              isLoggedIn ? "logged-in" : ""
            }`}
            onClick={() =>
              setShowRoleMenu((current) => !current)
            }
          >

            {profileImage ? (
              <img
                src={profileImage}
                alt={displayName}
                className="cp-role-avatar"
              />
            ) : (
              <span className="cp-role-avatar cp-role-avatar-placeholder">
                {isUmpireMode
                  ? "🧑‍⚖️"
                  : isPlayerMode
                    ? "🏏"
                    : "👤"}
              </span>
            )}

            <span className="cp-role-info">
              <small>ROLE</small>
              <strong>{activeRole}</strong>
            </span>

            <span
              className={`cp-role-chevron ${
                showRoleMenu ? "open" : ""
              }`}
            >
              ⌄
            </span>

          </button>

          {/* ====================================================
              ROLE DROPDOWN
              ==================================================== */}

          {showRoleMenu && (
            <div className="cp-role-dropdown">

              {/* ==================================================
                  LOGGED OUT
                  ================================================== */}

              {!isLoggedIn ? (
                <>
                  <div className="cp-dropdown-heading">
                    <span>
                      Welcome to CricPulse
                    </span>

                    <small>
                      Choose how you want to enter
                    </small>
                  </div>

                  <button
                    type="button"
                    className="cp-dropdown-option"
                    onClick={() => {
                      closeRoleMenu();
                      onLogin?.();
                    }}
                  >
                    <span className="cp-option-icon">
                      🏏
                    </span>

                    <span>
                      <strong>
                        Login as Player
                      </strong>

                      <small>
                        View your cricket profile
                      </small>
                    </span>

                    <span className="cp-option-arrow">
                      →
                    </span>
                  </button>

                  <button
                    type="button"
                    className="cp-dropdown-option"
                    onClick={() => {
                      closeRoleMenu();
                      onLogin?.();
                    }}
                  >
                    <span className="cp-option-icon">
                      🧑‍⚖️
                    </span>

                    <span>
                      <strong>
                        Login as Umpire
                      </strong>

                      <small>
                        Manage your matches
                      </small>
                    </span>

                    <span className="cp-option-arrow">
                      →
                    </span>
                  </button>

                  <div className="cp-dropdown-footer">
                    New to CricPulse?

                    <button
                      type="button"
                      onClick={() => {
                        closeRoleMenu();
                        onRegister?.();
                      }}
                    >
                      Create an account
                    </button>
                  </div>
                </>
              ) : isUmpireMode ? (

                /* ==================================================
                   UMPIRE MODE
                   ================================================== */

                <>
                  <div className="cp-profile-dropdown-header">

                    {profileImage ? (
                      <img
                        src={profileImage}
                        alt={displayName}
                        className="cp-large-avatar"
                      />
                    ) : (
                      <div className="cp-large-avatar cp-large-avatar-placeholder">
                        🧑‍⚖️
                      </div>
                    )}

                    <div>
                      <strong>
                        {displayName}
                      </strong>

                      <span>
                        Umpire
                      </span>
                    </div>

                  </div>

                  {mobileNumber && (
                    <div className="cp-profile-detail">
                      <span>📱</span>
                      {mobileNumber}
                    </div>
                  )}

                  {email && (
                    <div className="cp-profile-detail">
                      <span>✉️</span>
                      {email}
                    </div>
                  )}

                  {/* UMPIRE DASHBOARD */}

                  <button
                    type="button"
                    className="cp-profile-action"
                    onClick={goToUmpireDashboard}
                  >
                    <span>
                      Umpire Dashboard
                    </span>

                    <span>
                      →
                    </span>
                  </button>

                  {/* CREATE MATCH
                      Intentionally goes to dashboard */}

                  <button
                    type="button"
                    className="cp-profile-action"
                    onClick={goToUmpireDashboard}
                  >
                    <span>
                      Create a Match
                    </span>

                    <span>
                      →
                    </span>
                  </button>

                  {/* BACK TO PLAYER MODE */}

                  <button
                    type="button"
                    className="cp-profile-action"
                    onClick={goBackToPlayerMode}
                  >
                    <span>
                      Back to Player Mode
                    </span>

                    <span>
                      ←
                    </span>
                  </button>

                  {/* LOGOUT */}

                  <button
                    type="button"
                    className="cp-profile-action cp-logout-action"
                    onClick={handleLogout}
                  >
                    <span>
                      Logout
                    </span>

                    <span>
                      ↪
                    </span>
                  </button>
                </>

              ) : (

                /* ==================================================
                   PLAYER MODE
                   ================================================== */

                <>
                  <div className="cp-profile-dropdown-header">

                    {profileImage ? (
                      <img
                        src={profileImage}
                        alt={displayName}
                        className="cp-large-avatar"
                      />
                    ) : (
                      <div className="cp-large-avatar cp-large-avatar-placeholder">
                        🏏
                      </div>
                    )}

                    <div>
                      <strong>
                        {displayName}
                      </strong>

                      <span>
                        Player
                      </span>
                    </div>

                  </div>

                  {mobileNumber && (
                    <div className="cp-profile-detail">
                      <span>📱</span>
                      {mobileNumber}
                    </div>
                  )}

                  {email && (
                    <div className="cp-profile-detail">
                      <span>✉️</span>
                      {email}
                    </div>
                  )}

                  <div className="cp-role-stats">

                    <div>
                      <strong>
                        {loggedInUser?.matchesPlayed ?? 0}
                      </strong>

                      <span>
                        Matches
                      </span>
                    </div>

                    <div>
                      <strong>
                        {loggedInUser?.runs ?? 0}
                      </strong>

                      <span>
                        Runs
                      </span>
                    </div>

                    <div>
                      <strong>
                        {loggedInUser?.wickets ?? 0}
                      </strong>

                      <span>
                        Wickets
                      </span>
                    </div>

                  </div>

                  {/* PLAYER PROFILE */}

                  <button
                    type="button"
                    className="cp-profile-action"
                    onClick={goToProfile}
                  >
                    <span>
                      Player Records & Info
                    </span>

                    <span>
                      →
                    </span>
                  </button>

                  {/* UMPIRE ACCESS */}

                  {accountIsUmpire && (
                    <button
                      type="button"
                      className="cp-profile-action"
                      onClick={goToUmpireDashboard}
                    >
                      <span>
                        Umpire Dashboard
                      </span>

                      <span>
                        →
                      </span>
                    </button>
                  )}

                  {/* LOGOUT */}

                  <button
                    type="button"
                    className="cp-profile-action cp-logout-action"
                    onClick={handleLogout}
                  >
                    <span>
                      Logout
                    </span>

                    <span>
                      ↪
                    </span>
                  </button>

                </>
              )}

            </div>
          )}

        </div>

      </div>
    </header>
  );
}

export default Navbar;