
import { useEffect, useRef, useState } from 'react';
import '../styles/profile-capsule.css';
import {
  FaChevronDown,
  FaUser,
  FaSignInAlt,
  FaUserPlus,
  FaExchangeAlt,
  FaChartBar,
  FaPlusCircle,
  FaSignOutAlt,
} from 'react-icons/fa';

function ProfileCapsule({
  user = null,
  role = 'User',
  onLoginAsPlayer,
  onLoginAsUmpire,
  onRegister,
  onSwitchRole,
  onViewProfile,
  onCreateMatch,
  onLogout,
}) {
  const [menuOpen, setMenuOpen] = useState(false);
  const capsuleRef = useRef(null);

  const isLoggedIn = Boolean(user);
  const currentRole = isLoggedIn ? role : 'User';

  const displayName = isLoggedIn
    ? `${user.firstName || ''} ${user.lastName || ''}`.trim() || 'Player'
    : 'Guest';

  const profileImage = user?.profileImageUrl;

  useEffect(() => {
    const handleOutsideClick = (event) => {
      if (
        capsuleRef.current &&
        !capsuleRef.current.contains(event.target)
      ) {
        setMenuOpen(false);
      }
    };

    const handleEscape = (event) => {
      if (event.key === 'Escape') setMenuOpen(false);
    };

    document.addEventListener('mousedown', handleOutsideClick);
    document.addEventListener('keydown', handleEscape);

    return () => {
      document.removeEventListener('mousedown', handleOutsideClick);
      document.removeEventListener('keydown', handleEscape);
    };
  }, []);

  const handleAction = (callback) => {
    setMenuOpen(false);
    callback?.();
  };

  return (
    <div className="cp-profile-capsule" ref={capsuleRef}>
      <button
        type="button"
        className={`cp-profile-trigger ${menuOpen ? 'is-open' : ''}`}
        onClick={() => setMenuOpen((open) => !open)}
        aria-expanded={menuOpen}
        aria-haspopup="menu"
        aria-label="Open profile menu"
      >
        <span className="cp-profile-avatar">
          {profileImage ? (
            <img src={profileImage} alt="" />
          ) : (
            <FaUser />
          )}
        </span>

        <span className="cp-profile-identity">
          <span className="cp-profile-name">{displayName}</span>
          <span className="cp-profile-role">{currentRole}</span>
        </span>

        <FaChevronDown className="cp-profile-chevron" />
      </button>

      {menuOpen && (
        <div className="cp-profile-menu" role="menu">
          <div className="cp-profile-menu-heading">
            <span className="cp-profile-menu-name">{displayName}</span>
            <span className="cp-profile-menu-role">{currentRole} mode</span>
          </div>

          <div className="cp-profile-menu-divider" />

          {!isLoggedIn ? (
            <>
              <button
                type="button"
                role="menuitem"
                className="cp-profile-menu-item"
                onClick={() => handleAction(onLoginAsPlayer)}
              >
                <FaSignInAlt />
                <span>Login as Player</span>
              </button>

              <button
                type="button"
                role="menuitem"
                className="cp-profile-menu-item"
                onClick={() => handleAction(onLoginAsUmpire)}
              >
                <FaSignInAlt />
                <span>Login as Umpire</span>
              </button>

              <button
                type="button"
                role="menuitem"
                className="cp-profile-menu-item"
                onClick={() => handleAction(onRegister)}
              >
                <FaUserPlus />
                <span>New user? Register</span>
              </button>
            </>
          ) : (
            <>
              {currentRole === 'Player' && (
                <>
                  <button
                    type="button"
                    role="menuitem"
                    className="cp-profile-menu-item"
                    onClick={() => handleAction(() => onSwitchRole?.('Umpire'))}
                  >
                    <FaExchangeAlt />
                    <span>Switch to Umpire Mode</span>
                  </button>

                  <button
                    type="button"
                    role="menuitem"
                    className="cp-profile-menu-item"
                    onClick={() => handleAction(onViewProfile)}
                  >
                    <FaChartBar />
                    <span>View Records & Personal Details</span>
                  </button>
                </>
              )}

              {currentRole === 'Umpire' && (
                <>
                  <button
                    type="button"
                    role="menuitem"
                    className="cp-profile-menu-item"
                    onClick={() => handleAction(() => onSwitchRole?.('Player'))}
                  >
                    <FaExchangeAlt />
                    <span>Switch to Player Mode</span>
                  </button>

                  <button
                    type="button"
                    role="menuitem"
                    className="cp-profile-menu-item"
                    onClick={() => handleAction(onCreateMatch)}
                  >
                    <FaPlusCircle />
                    <span>Create a Match</span>
                  </button>
                </>
              )}

              <div className="cp-profile-menu-divider" />

              <button
                type="button"
                role="menuitem"
                className="cp-profile-menu-item cp-profile-logout"
                onClick={() => handleAction(onLogout)}
              >
                <FaSignOutAlt />
                <span>Logout</span>
              </button>
            </>
          )}
        </div>
      )}
    </div>
  );
}

export default ProfileCapsule;
