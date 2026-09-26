import { useState } from "react";
import "./Navbar.css";

function Navbar({
  onLogin,
  onRegister,
  loggedInUser
}) {
  const [showRoleMenu, setShowRoleMenu] = useState(false);

  const isLoggedIn = !!loggedInUser;

  const isUmpire =
    loggedInUser?.isUmpire === true ||
    loggedInUser?.role?.toLowerCase() === "umpire";

  const isPlayer =
    isLoggedIn && !isUmpire;

  const activeRole = !isLoggedIn
    ? "User"
    : isUmpire
      ? "Umpire"
      : "Player";

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

  const handleRoleAction = () => {
    setShowRoleMenu((current) => !current);
  };

  const goHome = () => {
    setShowRoleMenu(false);

    window.scrollTo({
      top: 0,
      behavior: "smooth"
    });
  };

  const goToMatches = () => {
    setShowRoleMenu(false);
    window.location.hash = "matches";
  };

  const goToAbout = () => {
    setShowRoleMenu(false);
    window.location.hash = "about";
  };

  const goToProfile = () => {
    setShowRoleMenu(false);

    if (isUmpire) {
      window.location.hash = "umpire-profile";
    } else {
      window.location.hash = "player-profile";
    }
  };

  return (
    <header className="cp-navbar">
      <div className="cp-navbar-inner">

        {/* ================= BRAND ================= */}
        <button
          type="button"
          className="cp-brand"
          onClick={goHome}
        >
          <span className="cp-brand-ball">🏏</span>

          <span className="cp-brand-text">
            Cric<span>Pulse</span>
          </span>
        </button>

        {/* ================= NAV LINKS ================= */}
        <nav className="cp-nav-links">

          <button
            type="button"
            className="cp-nav-link active"
            onClick={goHome}
          >
            Home
          </button>

          <button
            type="button"
            className="cp-nav-link"
            onClick={goToMatches}
          >
            Matches
          </button>

          <button
            type="button"
            className="cp-nav-link"
            onClick={goToAbout}
          >
            About
          </button>

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

        {/* ================= ROLE CAPSULE ================= */}
        <div className="cp-role-wrapper">

          <button
            type="button"
            className={`cp-role-capsule ${
              isLoggedIn ? "logged-in" : ""
            }`}
            onClick={handleRoleAction}
          >
            {profileImage ? (
              <img
                src={profileImage}
                alt={displayName}
                className="cp-role-avatar"
              />
            ) : (
              <span className="cp-role-avatar cp-role-avatar-placeholder">
                {isUmpire
                  ? "🧑‍⚖️"
                  : isPlayer
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

          {/* ================= ROLE DROPDOWN ================= */}
          {showRoleMenu && (
            <div className="cp-role-dropdown">

              {/* ================= LOGGED OUT ================= */}
              {!isLoggedIn ? (
                <>
                  <div className="cp-dropdown-heading">
                    <span>Welcome to CricPulse</span>
                    <small>
                      Choose how you want to enter
                    </small>
                  </div>

                  <button
                    type="button"
                    className="cp-dropdown-option"
                    onClick={() => {
                      setShowRoleMenu(false);
                      onLogin();
                    }}
                  >
                    <span className="cp-option-icon">
                      🏏
                    </span>

                    <span>
                      <strong>Login as Player</strong>
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
                      setShowRoleMenu(false);
                      onLogin();
                    }}
                  >
                    <span className="cp-option-icon">
                      🧑‍⚖️
                    </span>

                    <span>
                      <strong>Login as Umpire</strong>
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
                        setShowRoleMenu(false);
                        onRegister();
                      }}
                    >
                      Create an account
                    </button>
                  </div>
                </>
              ) : (

                /* ================= LOGGED IN ================= */
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
                        {isUmpire
                          ? "🧑‍⚖️"
                          : "🏏"}
                      </div>
                    )}

                    <div>
                      <strong>{displayName}</strong>
                      <span>{activeRole}</span>
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

                  {/* ================= PLAYER STATS ================= */}
                  {isPlayer && (
                    <div className="cp-role-stats">

                      <div>
                        <strong>
                          {loggedInUser?.matchesPlayed ?? 0}
                        </strong>

                        <span>Matches</span>
                      </div>

                      <div>
                        <strong>
                          {loggedInUser?.runs ?? 0}
                        </strong>

                        <span>Runs</span>
                      </div>

                      <div>
                        <strong>
                          {loggedInUser?.wickets ?? 0}
                        </strong>

                        <span>Wickets</span>
                      </div>

                    </div>
                  )}

                  {/* ================= UMPIRE STATS ================= */}
                  {isUmpire && (
                    <div className="cp-role-stats">

                      <div>
                        <strong>
                          {loggedInUser?.matchesCreated ?? 0}
                        </strong>

                        <span>Created</span>
                      </div>

                      <div>
                        <strong>
                          {loggedInUser?.organizedSuccessfully ?? 0}
                        </strong>

                        <span>Successful</span>
                      </div>

                      <div>
                        <strong>
                          {loggedInUser?.cancelledMatches ?? 0}
                        </strong>

                        <span>Cancelled</span>
                      </div>

                    </div>
                  )}

                  {/* ================= PROFILE BUTTON ================= */}
                  <button
                    type="button"
                    className="cp-profile-action"
                    onClick={goToProfile}
                  >
                    View {activeRole} Profile

                    <span>→</span>
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