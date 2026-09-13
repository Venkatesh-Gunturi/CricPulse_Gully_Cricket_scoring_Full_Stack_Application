import { useState } from "react";

import MainPage from "./pages/MainPage";
import UmpireDashboard from "./pages/UmpireDashboard";

import LoginModal from "./components/Auth/LoginModal";
import RegisterModal from "./components/Auth/RegisterModal";
import MatchCreation from "./components/Match/MatchCreation";

function App() {
  const [showLoginModal, setShowLoginModal] = useState(false);
  const [showRegisterModal, setShowRegisterModal] = useState(false);
 
  const [showMatchCreation, setShowMatchCreation] =
    useState(false);

  const [loggedInUser, setLoggedInUser] = useState(
    JSON.parse(localStorage.getItem("user"))
  );

  const [appMode, setAppMode] = useState("normal");

  const handleLoginSuccess = (user) => {
    setLoggedInUser(user);

    localStorage.setItem(
      "user",
      JSON.stringify(user)
    );

    setShowLoginModal(false);

    if (user?.isUmpire) {
      setAppMode("normal");
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");

    setLoggedInUser(null);
    setAppMode("normal");
    setShowMatchCreation(false);
  };

  return (
    <>
      {loggedInUser?.isUmpire && (
        <nav className="navbar navbar-dark bg-dark">
          <div className="container">
            <span className="navbar-brand">
              CricPulse 🏏
            </span>

            <div className="d-flex gap-2">
              <button
                className={
                  appMode === "normal"
                    ? "btn btn-light"
                    : "btn btn-outline-light"
                }
                onClick={() => {
                  setShowMatchCreation(false);
                  setAppMode("normal");
                }}
              >
                Normal Mode
              </button>

              <button
                className={
                  appMode === "umpire"
                    ? "btn btn-warning"
                    : "btn btn-outline-warning"
                }
                onClick={() => {
                  setShowMatchCreation(false);
                  setAppMode("umpire");
                }}
              >
                Umpire Mode
              </button>

              <button
                className="btn btn-outline-danger"
                onClick={handleLogout}
              >
                Logout
              </button>
            </div>
          </div>
        </nav>
      )}

      {!loggedInUser?.isUmpire && loggedInUser && (
        <nav className="navbar navbar-dark bg-dark">
          <div className="container">
            <span className="navbar-brand">
              CricPulse 🏏
            </span>

            <button
              className="btn btn-outline-danger"
              onClick={handleLogout}
            >
              Logout
            </button>
          </div>
        </nav>
      )}

      {appMode === "normal" && (
        <MainPage
          onLogin={() => setShowLoginModal(true)}
          onRegister={() => setShowRegisterModal(true)}
          loggedInUser={loggedInUser}
          onCreateMatch={() => setShowMatchCreation(true)}
        />
      )}

      {showMatchCreation && (
        <MatchCreation />
      )}

      {appMode === "umpire" &&
        loggedInUser?.isUmpire && (
          <>
            {!showMatchCreation ? (
              <UmpireDashboard
                onCreateMatch={() =>
                  setShowMatchCreation(true)
                }
                onContinueScoring={(match) => {
                  console.log("Continue scoring:", match);
                }}
              />
            ) : (
              <div className="container mt-4">
                <button
                  className="btn btn-secondary mb-3"
                  onClick={() =>
                    setShowMatchCreation(false)
                  }
                >
                  ← Back to My Matches
                </button>

                <MatchCreation />
              </div>
            )}
          </>
        )}

        <LoginModal
          show={showLoginModal}
          onClose={() => setShowLoginModal(false)}
          onLoginSuccess={handleLoginSuccess}
          onRegister={() => {
            setShowLoginModal(false);
            setShowRegisterModal(true);
          }}
        />

        <RegisterModal
          show={showRegisterModal}
          onClose={() =>
            setShowRegisterModal(false)
          }
                  onRegistered={(registrationResult) => {
            setShowRegisterModal(false);

            localStorage.setItem(
              "token",
              registrationResult.token
            );

            localStorage.setItem(
              "user",
              JSON.stringify(registrationResult.user)
            );

            setLoggedInUser(registrationResult.user);
            setAppMode("normal");
          }}
          onLogin={() => {
            setShowRegisterModal(false);
            setShowLoginModal(true);
          }}
        />

    </>
  );
}

export default App;