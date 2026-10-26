import { useState } from 'react';
import { Link, NavLink, useNavigate } from 'react-router-dom';
import { FaBars, FaTimes, FaCircle } from 'react-icons/fa';
import { GiCricketBat } from 'react-icons/gi';

import { useAuth } from '../context/AuthContext';
import LoginModal from './LoginModal';
import RegisterModal from './RegisterModal';
import ProfileCapsule from './ProfileCapsule';

function Navbar() {
  const navigate = useNavigate();

  const { user, setUser, role, setRole, logout } = useAuth();

  const [menuOpen, setMenuOpen] = useState(false);
  const [activeModal, setActiveModal] = useState(null);
  const [loginMode, setLoginMode] = useState('Player');

  const navItems = [
    { label: 'Home', path: '/' },
    { label: 'Matches', path: '/matches' },
    { label: 'About', path: '/about' },
    { label: 'Cricket IQ', path: '/cricket-iq' },
  ];

  const closeMenu = () => setMenuOpen(false);

  const openModal = (modal, selectedMode = 'Player') => {
    closeMenu();

    if (modal === 'login') {
      setLoginMode(selectedMode);
    }

    setActiveModal(modal);
  };

  const closeModal = () => setActiveModal(null);

  const switchToRegister = () => {
    setActiveModal('register');
  };

  const switchToLogin = () => {
    setLoginMode('Player');
    setActiveModal('login');
  };

  
const handleLoginSuccess = (
  authenticatedUser,
  selectedMode,
  rememberMe
) => {
  const nextRole = selectedMode === 'Umpire' ? 'Umpire' : 'Player';

  setUser(authenticatedUser, rememberMe);
  setRole(nextRole, rememberMe);

  setActiveModal(null);
  closeMenu();

  navigate(nextRole === 'Umpire' ? '/umpire' : '/');
};


  const handleLogout = () => {
    logout();
    setActiveModal(null);
    closeMenu();
    navigate('/');
  };

  const handleSwitchRole = (nextRole) => {
    setRole(nextRole);
    closeMenu();

    navigate(nextRole === 'Umpire' ? '/umpire' : '/');
  };

  
  const handleViewProfile = () => {
  closeMenu();
  navigate(role === 'Umpire' ? '/umpire' : '/player/profile');
};

  const handleCreateMatch = () => {
    closeMenu();
    navigate('/matches');
  };

  return (
    <>
      <header className="site-navbar sticky-top">
        <nav className="container-fluid navbar-shell px-5">
          <div className="navbar-left">
            <button
              className="mobile-menu-toggle"
              type="button"
              aria-label={
                menuOpen
                  ? 'Close navigation menu'
                  : 'Open navigation menu'
              }
              aria-expanded={menuOpen}
              onClick={() => setMenuOpen((open) => !open)}
            >
              {menuOpen ? <FaTimes /> : <FaBars />}
            </button>

            <Link
              to="/"
              className="navbar-brand d-flex align-items-center gap-2"
              onClick={closeMenu}
            >
              <span className="cricpulse-logo" aria-hidden="true">
                <GiCricketBat className="logo-bat" />
                <FaCircle className="logo-ball" />
              </span>

              <span className="brand-name">CricPulse</span>
            </Link>
          </div>

          <div className={`navbar-links ${menuOpen ? 'is-open' : ''}`}>
            {navItems.map((item) => (
              <NavLink
                key={item.path}
                to={item.path}
                end={item.path === '/'}
                onClick={closeMenu}
                className={({ isActive }) =>
                  `navbar-link ${isActive ? 'active' : ''}`
                }
              >
                {item.label}
              </NavLink>
            ))}

            <div className="mobile-auth-actions">
              {!user && (
                <>
                  <button
                    type="button"
                    className="btn btn-outline-success"
                    onClick={() => openModal('login', 'Player')}
                  >
                    Login
                  </button>

                  <button
                    type="button"
                    className="btn btn-success"
                    onClick={() => openModal('register')}
                  >
                    Register
                  </button>
                </>
              )}
            </div>
          </div>

          <div className="navbar-auth-actions">
            {!user ? (
              <>
                <button
                  type="button"
                  className="btn btn-outline-success"
                  onClick={() => openModal('login', 'Player')}
                >
                  Login
                </button>

                <button
                  type="button"
                  className="btn btn-success"
                  onClick={() => openModal('register')}
                >
                  Register
                </button>
              </>
            ) : (
              <button
                type="button"
                className="btn btn-outline-success"
                onClick={handleLogout}
              >
                Logout
              </button>
            )}
          </div>

          <ProfileCapsule
            user={user}
            role={role}
            onLoginAsPlayer={() => openModal('login', 'Player')}
            onLoginAsUmpire={() => openModal('login', 'Umpire')}
            onRegister={() => openModal('register')}
            onSwitchRole={handleSwitchRole}
            onViewProfile={handleViewProfile}
            onCreateMatch={handleCreateMatch}
            onLogout={handleLogout}
          />
        </nav>
      </header>

      <LoginModal
        show={activeModal === 'login'}
        onClose={closeModal}
        onSwitchToRegister={switchToRegister}
        onLoginSuccess={handleLoginSuccess}
        loginMode={loginMode}
      />

      <RegisterModal
        show={activeModal === 'register'}
        onClose={closeModal}
        onSwitchToLogin={switchToLogin}
      />
    </>
  );
}

export default Navbar;
