import { useState } from "react";

import MainPage from "./pages/MainPage";
import UmpireDashboard from "./pages/UmpireDashboard";

import LoginModal from "./components/Auth/LoginModal";
import RegisterModal from "./components/Auth/RegisterModal";
import MatchCreation from "./components/Match/MatchCreation";

import LiveScoring from "./pages/LiveScoring";
import TossScreen from "./pages/TossScreen";
import InningsSetup from "./pages/InningsSetup";
import SecondInningsSetup from "./pages/SecondInningsSetup";
import FirstInningsCompleted from "./pages/FirstInningsCompleted";
import MatchCompleted from "./pages/MatchCompleted";

import MatchesPage from "./pages/Matches/MatchesPage";
import PlayerProfile from "./pages/PlayerProfile";

function App() {
 
  // ============================================================
  // NORMAL APPLICATION NAVIGATION
  // ============================================================

  const [showMatches, setShowMatches] = useState(false);

  const [showPlayerProfile, setShowPlayerProfile] =
    useState(false);

  const [showMatchCreation, setShowMatchCreation] =
    useState(false);

  // ============================================================
  // AUTH MODALS
  // ============================================================

  const [showLoginModal, setShowLoginModal] =
    useState(false);

  const [showRegisterModal, setShowRegisterModal] =
    useState(false);

  // ============================================================
  // UMPIRE / LIVE SCORING STATE
  // ============================================================

  const [scoringMatch, setScoringMatch] =
    useState(null);

  const [tossResult, setTossResult] =
    useState(null);

  const [inningsStarted, setInningsStarted] =
    useState(false);

  const [firstInningsCompleted, setFirstInningsCompleted] =
    useState(false);

  const [firstInningsData, setFirstInningsData] =
    useState(null);

  const [showSecondInningsSetup, setShowSecondInningsSetup] =
    useState(false);

  const [matchCompleted, setMatchCompleted] =
    useState(false);

  const [completedMatchData, setCompletedMatchData] =
    useState(null);

  // ============================================================
  // AUTHENTICATED USER
  // ============================================================

  const [loggedInUser, setLoggedInUser] = useState(() => {
    try {
      const savedUser =
        localStorage.getItem("user");

      return savedUser
        ? JSON.parse(savedUser)
        : null;
    } catch (error) {
      console.error(
        "Unable to restore logged-in user:",
        error
      );

      return null;
    }
  });

  // ============================================================
  // APP MODE
  //
  // normal
  //   Home / Player application
  //
  // umpire
  //   Umpire Dashboard / Match creation / Scoring
  // ============================================================

  const [appMode, setAppMode] =
    useState("normal");

  // ============================================================
  // HELPER - RESET NORMAL NAVIGATION
  // ============================================================

  const resetNormalNavigation = () => {
    setShowMatches(false);
    setShowPlayerProfile(false);
    setShowMatchCreation(false);
  };

  // ============================================================
  // HELPER - RESET SCORING STATE
  // ============================================================

  const resetScoringState = () => {
    setScoringMatch(null);
    setTossResult(null);

    setInningsStarted(false);

    setFirstInningsCompleted(false);
    setFirstInningsData(null);

    setShowSecondInningsSetup(false);

    setMatchCompleted(false);
    setCompletedMatchData(null);
  };

  // ============================================================
  // LOGIN SUCCESS
  // ============================================================

  const handleLoginSuccess = (user) => {
    setLoggedInUser(user);

    localStorage.setItem(
      "user",
      JSON.stringify(user)
    );

    setShowLoginModal(false);

    // Always start after login in normal/player mode.
    setAppMode("normal");

    resetNormalNavigation();
    resetScoringState();

    window.location.hash = "";

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  // ============================================================
  // LOGOUT
  // ============================================================

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("user");

    setLoggedInUser(null);

    setAppMode("normal");

    resetNormalNavigation();
    resetScoringState();

    setShowLoginModal(false);
    setShowRegisterModal(false);

    window.location.hash = "";

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  // ============================================================
  // OPEN MATCHES
  // ============================================================

  const handleOpenMatches = () => {
    resetNormalNavigation();

    setShowMatches(true);

    setAppMode("normal");

    window.location.hash = "matches";

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  // ============================================================
  // CLOSE MATCHES
  // ============================================================

  const handleCloseMatches = () => {
    setShowMatches(false);

    window.location.hash = "";

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  // ============================================================
  // OPEN PLAYER PROFILE
  // ============================================================

  const handleOpenPlayerProfile = () => {
    resetNormalNavigation();

    setShowPlayerProfile(true);

    setAppMode("normal");

    window.location.hash = "player-profile";

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  // ============================================================
  // CLOSE PLAYER PROFILE
  // ============================================================

  const handleClosePlayerProfile = () => {
    setShowPlayerProfile(false);

    window.location.hash = "";

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  // ============================================================
  // OPEN ABOUT
  // ============================================================

  const handleOpenAbout = () => {
    // About exists on MainPage.
    // Therefore we must leave any other normal page first.

    setShowMatches(false);
    setShowPlayerProfile(false);
    setShowMatchCreation(false);

    // About should never leave the user in umpire mode.
    setAppMode("normal");

    window.location.hash = "about";

    // Wait until MainPage has rendered.
    setTimeout(() => {
      const aboutSection =
        document.getElementById("about");

      if (aboutSection) {
        aboutSection.scrollIntoView({
          behavior: "smooth",
          block: "start",
        });
      }
    }, 0);
  };

  // ============================================================
  // RETURN HOME
  // ============================================================

  const handleBackToHome = () => {
    resetNormalNavigation();
    resetScoringState();

    setAppMode("normal");

    window.location.hash = "";

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  // ============================================================
  // OPEN UMPIRE DASHBOARD
  // ============================================================

  const handleOpenUmpireDashboard = () => {
    // Clear normal application pages.
    resetNormalNavigation();

    // A fresh dashboard should not have an old scoring session.
    resetScoringState();

    // Enter umpire mode.
    setAppMode("umpire");

    window.location.hash = "";

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  // ============================================================
  // BACK TO PLAYER MODE
  // ============================================================

  const handleBackToPlayerMode = () => {
    // Clear everything related to umpire/scoring mode.
    resetScoringState();

    // Clear normal sub-pages.
    resetNormalNavigation();

    // THIS is the important state change.
    setAppMode("normal");

    window.location.hash = "";

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  // ============================================================
  // OPEN MATCH CREATION
  //
  // This is intentionally used ONLY from the Umpire Dashboard.
  // Home Create Match buttons should first open the dashboard.
  // ============================================================

  const handleOpenCreateMatch = () => {
    setShowMatches(false);
    setShowPlayerProfile(false);

    setShowMatchCreation(true);

    setAppMode("normal");
  };

  // ============================================================
  // START MATCH CREATION FROM UMPIRE DASHBOARD
  // ============================================================

  const handleStartMatchCreation = () => {
    setShowMatchCreation(true);

    setShowMatches(false);
    setShowPlayerProfile(false);

    // Remain in umpire mode.
    setAppMode("umpire");
  };

  // ============================================================
  // CLOSE MATCH CREATION
  // ============================================================

  const handleCloseCreateMatch = () => {
    setShowMatchCreation(false);

    // If this was opened from umpire dashboard,
    // stay in umpire mode.
    setAppMode("umpire");
  };

  // ============================================================
  // CONTINUE EXISTING MATCH SCORING
  // ============================================================

  const handleContinueScoring = (match) => {
    // Match creation must not remain visible.
    setShowMatchCreation(false);

    // Store selected match.
    setScoringMatch(match);

    // Restore toss information when available.
    if (match.battingFirstTeam) {
      setTossResult({
        tossWinnerTeam:
          match.tossWinnerTeam,

        tossDecision:
          match.tossDecision,

        battingFirstTeam:
          match.battingFirstTeam,

        bowlingFirstTeam:
          match.battingFirstTeam ===
          match.team1Name
            ? match.team2Name
            : match.team1Name,
      });
    } else {
      setTossResult(null);
    }

    // Reset innings transition state.
    setFirstInningsCompleted(false);
    setFirstInningsData(null);
    setShowSecondInningsSetup(false);

    // Reset completion state.
    setMatchCompleted(false);
    setCompletedMatchData(null);

    // Restore whether innings has already started.
    setInningsStarted(
      match.hasStartedInnings === true
    );

    // Enter umpire mode.
    setAppMode("umpire");

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  // ============================================================
  // BACK TO UMPIRE DASHBOARD
  // ============================================================

  const handleBackToDashboard = () => {
    resetScoringState();

    setShowMatchCreation(false);

    setAppMode("umpire");

    window.location.hash = "";

    window.scrollTo({
      top: 0,
      behavior: "smooth",
    });
  };

  // ============================================================
  // SECOND INNINGS STARTED
  // ============================================================

  const handleSecondInningsStarted = () => {
    setFirstInningsCompleted(false);

    setShowSecondInningsSetup(false);
  };

  // ============================================================
  // RENDER
  // ============================================================

  return (
    <>
      {/* ======================================================
          NORMAL / PLAYER APPLICATION
          ====================================================== */}

      {appMode === "normal" && (
        <>
          {/* --------------------------------------------------
              PLAYER PROFILE
              -------------------------------------------------- */}

          {showPlayerProfile ? (
            <PlayerProfile
              onBack={
                handleClosePlayerProfile
              }

              loggedInUser={
                loggedInUser
              }

              appMode="normal"

              onMatches={
                handleOpenMatches
              }

              onPlayerProfile={
                handleOpenPlayerProfile
              }

              onUmpireDashboard={
                handleOpenUmpireDashboard
              }

              onBackToPlayerMode={
                handleBackToPlayerMode
              }

              onAbout={
                handleOpenAbout
              }

              onLogout={
                handleLogout
              }
            />
          ) : showMatches ? (

            /* ------------------------------------------------
               MATCHES PAGE
               ------------------------------------------------ */

            <MatchesPage
              onBack={
                handleCloseMatches
              }

              loggedInUser={
                loggedInUser
              }

              onMatches={
                handleOpenMatches
              }

              onPlayerProfile={
                handleOpenPlayerProfile
              }

              onUmpireDashboard={
                handleOpenUmpireDashboard
              }

              onBackToPlayerMode={
                handleBackToPlayerMode
              }

              onAbout={
                handleOpenAbout
              }

              onLogout={
                handleLogout
              }
            />

          ) : showMatchCreation ? (

            /* ------------------------------------------------
               MATCH CREATION
               ------------------------------------------------ */

            <div className="container mt-4">

              <button
                type="button"
                className="btn btn-secondary mb-3"
                onClick={
                  handleCloseCreateMatch
                }
              >
                ← Back
              </button>

              <MatchCreation />

            </div>

          ) : (

            /* ------------------------------------------------
               MAIN PAGE
               ------------------------------------------------ */

            <MainPage
              onLogin={() =>
                setShowLoginModal(true)
              }

              onRegister={() =>
                setShowRegisterModal(true)
              }

              loggedInUser={
                loggedInUser
              }

              appMode={
                appMode
              }

              onCreateMatch={
                handleOpenUmpireDashboard
              }

              onMatches={
                handleOpenMatches
              }

              onPlayerProfile={
                handleOpenPlayerProfile
              }

              onUmpireDashboard={
                handleOpenUmpireDashboard
              }

              onBackToPlayerMode={
                handleBackToPlayerMode
              }

              onAbout={
                handleOpenAbout
              }

              onLogout={
                handleLogout
              }
            />
          )}
        </>
      )}

      {/* ======================================================
          UMPIRE APPLICATION
          ====================================================== */}

     {appMode === "umpire" &&
  loggedInUser?.isUmpire && (
          <>
            {/* ------------------------------------------------
                MATCH COMPLETED
                ------------------------------------------------ */}

            {matchCompleted &&
            completedMatchData ? (

              <MatchCompleted
                match={
                  completedMatchData
                }

                onBack={
                  handleBackToDashboard
                }
              />

            ) : scoringMatch &&
              !tossResult ? (

              /* ------------------------------------------------
                 TOSS SCREEN
                 ------------------------------------------------ */

              <TossScreen
                match={
                  scoringMatch
                }

                onTossComplete={
                  (result) => {
                    setTossResult(
                      result
                    );
                  }
                }
              />

            ) : scoringMatch &&
              tossResult &&
              !inningsStarted ? (

              /* ------------------------------------------------
                 INNINGS SETUP
                 ------------------------------------------------ */

              <InningsSetup
                match={
                  scoringMatch
                }

                tossResult={
                  tossResult
                }

                onInningsStarted={
                  () => {
                    setInningsStarted(
                      true
                    );
                  }
                }
              />

            ) : scoringMatch &&
              firstInningsCompleted &&
              !showSecondInningsSetup ? (

              /* ------------------------------------------------
                 FIRST INNINGS COMPLETED
                 ------------------------------------------------ */

              <FirstInningsCompleted
                battingTeam={
                  tossResult
                    .bowlingFirstTeam
                }

                targetRuns={
                  (firstInningsData?.totalRuns ?? 0) +
                  1
                }

                wickets={
                  scoringMatch.playersPerTeam -
                  1
                }

                overs={
                  scoringMatch.overs
                }

                onUndoLastBall={() => {
                  // Existing functionality
                  // remains unchanged.
                }}

                onCompleteInnings={
                  () => {
                    setShowSecondInningsSetup(
                      true
                    );
                  }
                }
              />

            ) : scoringMatch &&
              firstInningsCompleted &&
              showSecondInningsSetup ? (

              /* ------------------------------------------------
                 SECOND INNINGS SETUP
                 ------------------------------------------------ */

              <SecondInningsSetup
                match={{
                  ...scoringMatch,

                  firstInningsBattingTeam:
                    tossResult.battingFirstTeam,

                  firstInningsBowlingTeam:
                    tossResult.bowlingFirstTeam,
                }}

                onInningsStarted={
                  handleSecondInningsStarted
                }
              />

            ) : scoringMatch ? (

              /* ------------------------------------------------
                 LIVE SCORING
                 ------------------------------------------------ */

              <LiveScoring
                match={{
                  ...scoringMatch,

                  battingTeamName:
                    tossResult?.battingFirstTeam,

                  firstInningsTotalRuns:
                    firstInningsData?.totalRuns ??
                    null,
                }}

                onBack={
                  handleBackToDashboard
                }

                onFirstInningsCompleted={
                  (innings) => {
                    setFirstInningsCompleted(
                      true
                    );

                    setFirstInningsData(
                      innings
                    );

                    setScoringMatch(
                      (currentMatch) => ({
                        ...currentMatch,

                        firstInningsTotalRuns:
                          innings.totalRuns,
                      })
                    );
                  }
                }

                onMatchCompleted={
                  (completedMatch) => {
                    setCompletedMatchData({
                      ...scoringMatch,
                      ...completedMatch,
                    });

                    setMatchCompleted(
                      true
                    );
                  }
                }
              />

            ) : showMatchCreation ? (

              /* ------------------------------------------------
                 UMPIRE MATCH CREATION
                 ------------------------------------------------ */

              <div className="container mt-4">

                <button
                  type="button"
                  className="btn btn-secondary mb-3"
                  onClick={
                    handleCloseCreateMatch
                  }
                >
                  ← Back to My Matches
                </button>

                <MatchCreation />

              </div>

            ) : (

              /* ------------------------------------------------
                 UMPIRE DASHBOARD
                 ------------------------------------------------ */

              <UmpireDashboard
                loggedInUser={
                  loggedInUser
                }

                appMode="umpire"

                onCreateMatch={
                  handleStartMatchCreation
                }

                onContinueScoring={
                  handleContinueScoring
                }

                onMatches={
                  handleOpenMatches
                }

                onPlayerProfile={
                  handleOpenPlayerProfile
                }

                onUmpireDashboard={
                  handleOpenUmpireDashboard
                }

                onBackToPlayerMode={
                  handleBackToPlayerMode
                }

                onAbout={
                  handleOpenAbout
                }

                onLogout={
                  handleLogout
                }
              />
            )}
          </>
        )}

      {/* ======================================================
          LOGIN MODAL
          ====================================================== */}

      <LoginModal
        show={
          showLoginModal
        }

        onClose={() =>
          setShowLoginModal(false)
        }

        onLoginSuccess={
          handleLoginSuccess
        }

        onRegister={() => {
          setShowLoginModal(false);

          setShowRegisterModal(
            true
          );
        }}
      />

      {/* ======================================================
          REGISTER MODAL
          ====================================================== */}

      <RegisterModal
        show={
          showRegisterModal
        }

        onClose={() =>
          setShowRegisterModal(false)
        }

        onRegistered={
          (registrationResult) => {
            setShowRegisterModal(
              false
            );

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

            resetNormalNavigation();
            resetScoringState();

            setAppMode(
              "normal"
            );

            window.location.hash =
              "";

            window.scrollTo({
              top: 0,
              behavior: "smooth",
            });
          }
        }

        onLogin={() => {
          setShowRegisterModal(
            false
          );

          setShowLoginModal(
            true
          );
        }}
      />
    </>
  );
}

export default App;