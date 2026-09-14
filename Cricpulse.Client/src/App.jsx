import { useState } from "react";

import MainPage from "./pages/MainPage";
import UmpireDashboard from "./pages/UmpireDashboard";

import LoginModal from "./components/Auth/LoginModal";
import RegisterModal from "./components/Auth/RegisterModal";
import MatchCreation from "./components/Match/MatchCreation";
import LiveScoring from "./pages/LiveScoring";
import TossScreen from "./pages/TossScreen";
import InningsSetup from "./pages/InningsSetup";

function App() {
  const [showLoginModal, setShowLoginModal] = useState(false);
  const [showRegisterModal, setShowRegisterModal] = useState(false);
  const [showMatchCreation, setShowMatchCreation] = useState(false);

  const [scoringMatch, setScoringMatch] = useState(null);
  const [tossResult, setTossResult] = useState(null);
  const [inningsStarted, setInningsStarted] = useState(false);

  const [loggedInUser, setLoggedInUser] = useState(
    JSON.parse(localStorage.getItem("user"))
  );

  const [appMode, setAppMode] = useState("normal");

  // Purpose:
  // Handle successful login and update the authenticated user in the application.
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

  // Purpose:
  // Clear the authenticated session and return the application to normal mode.
  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");

    setLoggedInUser(null);
    setAppMode("normal");
    setShowMatchCreation(false);

    setScoringMatch(null);
    setTossResult(null);
    setInningsStarted(false);
  };

  // Purpose:
  // Initialize the correct scoring screen from the persisted match state
  // when the umpire chooses to continue scoring.
  const handleContinueScoring = (match) => {
    setShowMatchCreation(false);
    setScoringMatch(match);

    if (match.battingFirstTeam) {
      setTossResult({
        tossWinnerTeam: match.tossWinnerTeam,
        tossDecision: match.tossDecision,
        battingFirstTeam: match.battingFirstTeam,
        bowlingFirstTeam:
          match.battingFirstTeam === match.team1Name
            ? match.team2Name
            : match.team1Name
      });
    } else {
      setTossResult(null);
    }

    setInningsStarted(
      match.hasStartedInnings === true
    );
  };

  // Purpose:
  // Clear the current scoring session and return the umpire to the dashboard.
  const handleBackToDashboard = () => {
    setScoringMatch(null);
    setTossResult(null);
    setInningsStarted(false);
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
        <>
          {!showMatchCreation ? (
            <MainPage
              onLogin={() => setShowLoginModal(true)}
              onRegister={() => setShowRegisterModal(true)}
              loggedInUser={loggedInUser}
              onCreateMatch={() =>
                setShowMatchCreation(true)
              }
            />
          ) : (
            <div className="container mt-4">
              <button
                className="btn btn-secondary mb-3"
                onClick={() =>
                  setShowMatchCreation(false)
                }
              >
                ← Back
              </button>

              <MatchCreation />
            </div>
          )}
        </>
      )}

      {appMode === "umpire" &&
        loggedInUser?.isUmpire && (
          <>
            {scoringMatch &&
              !tossResult ? (
              <TossScreen
                match={scoringMatch}
                onTossComplete={(result) => {
                  setTossResult(result);
                }}
              />
            ) : scoringMatch &&
              tossResult &&
              !inningsStarted ? (
              <InningsSetup
                match={scoringMatch}
                tossResult={tossResult}
                onInningsStarted={() => {
                  setInningsStarted(true);
                }}
              />
            ) : scoringMatch ? (
              <LiveScoring
                match={{
                  ...scoringMatch,
                  battingTeamName:
                    tossResult?.battingFirstTeam
                }}
                onBack={
                  handleBackToDashboard
                }
              />
            ) : !showMatchCreation ? (
              <UmpireDashboard
                onCreateMatch={() =>
                  setShowMatchCreation(true)
                }
                onContinueScoring={
                  handleContinueScoring
                }
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
        onClose={() =>
          setShowLoginModal(false)
        }
        onLoginSuccess={
          handleLoginSuccess
        }
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
        onRegistered={(
          registrationResult
        ) => {
          setShowRegisterModal(false);

          localStorage.setItem(
            "token",
            registrationResult.token
          );

          localStorage.setItem(
            "user",
            JSON.stringify(
              registrationResult.user
            )
          );

          setLoggedInUser(
            registrationResult.user
          );

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