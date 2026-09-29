import { useEffect, useMemo, useState } from "react";

import { getMyMatches } from "../services/matchService";
import MatchManagement from "../components/Match/MatchManagement";
import Navbar from "../components/Layout/Navbar";

import "./UmpireDashboard.css";

const INITIAL_VISIBLE_MATCHES = 3;

const UmpireDashboard = ({
  loggedInUser,
  onCreateMatch,
  onContinueScoring,
  onMatches,
  onPlayerProfile,
  onUmpireDashboard,
  onBackToPlayerMode,
  onLogout,
}) => {
  const [matches, setMatches] = useState([]);
  const [activeTab, setActiveTab] =
    useState("Upcoming");

  const [showAllMatches, setShowAllMatches] =
    useState(false);

  const [loading, setLoading] =
    useState(true);

  const [error, setError] =
    useState("");

  useEffect(() => {
    loadMatches();
  }, []);

  const loadMatches = async () => {
    try {
      setLoading(true);
      setError("");

      const response =
        await getMyMatches();

      setMatches(
        Array.isArray(response)
          ? response
          : []
      );
    } catch (err) {
      console.error(
        "Unable to load umpire matches:",
        err
      );

      setError(
        err?.response?.data?.message ||
          "Unable to load your matches."
      );
    } finally {
      setLoading(false);
    }
  };

  const getStatus = (match) =>
    String(
      match?.status || ""
    ).toLowerCase();

  const upcomingMatches = useMemo(
    () =>
      matches.filter(
        (match) =>
          getStatus(match) ===
          "scheduled"
      ),
    [matches]
  );

  const liveMatches = useMemo(
    () =>
      matches.filter(
        (match) =>
          getStatus(match) ===
          "live"
      ),
    [matches]
  );

  const finishedMatches = useMemo(
    () =>
      matches.filter(
        (match) =>
          getStatus(match) ===
          "completed"
      ),
    [matches]
  );

  const cancelledMatches = useMemo(
    () =>
      matches.filter(
        (match) =>
          getStatus(match) ===
          "cancelled"
      ),
    [matches]
  );

  const tabMatches = useMemo(() => {
    switch (activeTab) {
      case "Live":
        return liveMatches;

      case "Finished":
        return finishedMatches;

      case "Cancelled":
        return cancelledMatches;

      case "Upcoming":
      default:
        return upcomingMatches;
    }
  }, [
    activeTab,
    upcomingMatches,
    liveMatches,
    finishedMatches,
    cancelledMatches,
  ]);

  const visibleMatches =
    showAllMatches
      ? tabMatches
      : tabMatches.slice(
          0,
          INITIAL_VISIBLE_MATCHES
        );

  const hasMoreMatches =
    tabMatches.length >
    INITIAL_VISIBLE_MATCHES;

  const formatDate = (value) => {
    if (!value) {
      return "Date unavailable";
    }

    const date =
      new Date(value);

    if (
      Number.isNaN(
        date.getTime()
      )
    ) {
      return value;
    }

    return date.toLocaleDateString(
      "en-IN",
      {
        day: "2-digit",
        month: "short",
        year: "numeric",
      }
    );
  };

  const formatTime = (value) => {
    if (!value) {
      return "Time unavailable";
    }

    if (
      typeof value === "string" &&
      /^\d{2}:\d{2}/.test(value)
    ) {
      const parts =
        value.split(":");

      const hours =
        Number(parts[0]);

      const minutes =
        Number(parts[1]);

      const date =
        new Date();

      date.setHours(
        hours,
        minutes,
        0,
        0
      );

      return date.toLocaleTimeString(
        "en-IN",
        {
          hour: "numeric",
          minute: "2-digit",
        }
      );
    }

    const date =
      new Date(value);

    if (
      Number.isNaN(
        date.getTime()
      )
    ) {
      return value;
    }

    return date.toLocaleTimeString(
      "en-IN",
      {
        hour: "numeric",
        minute: "2-digit",
      }
    );
  };

  const getVenue = (match) =>
    match?.venueName ||
    match?.venue ||
    "Venue not available";

  const getPlayersCount = (match) => {
    if (
      Array.isArray(
        match?.players
      )
    ) {
      return match.players.length;
    }

    if (
      match?.playersPerTeam
    ) {
      return (
        Number(
          match.playersPerTeam
        ) * 2
      );
    }

    return null;
  };

  const getScore = (
    match,
    team
  ) => {
    if (
      team === 1
    ) {
      return (
        match?.team1Score ??
        match?.team1Runs ??
        null
      );
    }

    return (
      match?.team2Score ??
      match?.team2Runs ??
      null
    );
  };

  const renderStatus = (
    match
  ) => {
    const status =
      getStatus(match);

    if (
      status === "live"
    ) {
      return (
        <span className="cp-umpire-match-status live">
          <span className="cp-umpire-live-dot" />
          LIVE
        </span>
      );
    }

    if (
      status === "scheduled"
    ) {
      return (
        <span className="cp-umpire-match-status upcoming">
          UPCOMING
        </span>
      );
    }

    if (
      status === "completed"
    ) {
      return (
        <span className="cp-umpire-match-status finished">
          FINISHED
        </span>
      );
    }

    if (
      status === "cancelled"
    ) {
      return (
        <span className="cp-umpire-match-status cancelled">
          CANCELLED
        </span>
      );
    }

    return (
      <span className="cp-umpire-match-status">
        {String(
          match?.status ||
            "UNKNOWN"
        ).toUpperCase()}
      </span>
    );
  };

  const renderMatchCard = (
    match
  ) => {
    const status =
      getStatus(match);

    const team1Score =
      getScore(match, 1);

    const team2Score =
      getScore(match, 2);

    const playersCount =
      getPlayersCount(match);

    return (
      <article
        key={
          match.id ??
          `${match.team1Name}-${match.team2Name}-${match.matchDate}`
        }
        className={`cp-umpire-match-card ${status}`}
      >

        <div className="cp-umpire-match-card-top">
          {renderStatus(match)}

          {match?.overs && (
            <span className="cp-umpire-match-overs">
              {match.overs} overs
            </span>
          )}
        </div>

        {/* TEAMS */}

        <div className="cp-umpire-teams">

          <div className="cp-umpire-team">

            <span className="cp-umpire-team-name">
              {match?.team1Name ||
                "Team 1"}
            </span>

            {team1Score !== null &&
              status !==
                "scheduled" &&
              status !==
                "cancelled" && (
                <span className="cp-umpire-team-score">
                  {team1Score}
                </span>
              )}

          </div>

          <div className="cp-umpire-vs">
            VS
          </div>

          <div className="cp-umpire-team">

            <span className="cp-umpire-team-name">
              {match?.team2Name ||
                "Team 2"}
            </span>

            {team2Score !== null &&
              status !==
                "scheduled" &&
              status !==
                "cancelled" && (
                <span className="cp-umpire-team-score">
                  {team2Score}
                </span>
              )}

          </div>

        </div>

        {/* DETAILS */}

        <div className="cp-umpire-match-details">

          <div className="cp-umpire-match-detail">
            <span className="cp-umpire-match-detail-label">
              DATE
            </span>

            <span className="cp-umpire-match-detail-value">
              {formatDate(
                match?.matchDate
              )}
            </span>
          </div>

          <div className="cp-umpire-match-detail">
            <span className="cp-umpire-match-detail-label">
              TIME
            </span>

            <span className="cp-umpire-match-detail-value">
              {formatTime(
                match?.matchTime
              )}
            </span>
          </div>

          <div className="cp-umpire-match-detail">
            <span className="cp-umpire-match-detail-label">
              PLAYERS
            </span>

            <span className="cp-umpire-match-detail-value">
              {playersCount
                ? `${playersCount} players`
                : "Not available"}
            </span>
          </div>

          <div className="cp-umpire-match-detail">
            <span className="cp-umpire-match-detail-label">
              STATE
            </span>

            <span className="cp-umpire-match-detail-value">
              {match?.state ||
                "Not available"}
            </span>
          </div>

        </div>

        {/* FOOTER */}

        <div className="cp-umpire-match-footer">

          <span className="cp-umpire-match-location">
            📍 {getVenue(match)}
          </span>

          {status === "live" && (
            <button
              type="button"
              className="cp-umpire-match-action live-action"
              onClick={() =>
                onContinueScoring?.(
                  match
                )
              }
            >
              Continue Scoring
            </button>
          )}

          {status === "scheduled" && (
            <span className="cp-umpire-match-action manage">
              Manage Match
            </span>
          )}

          {status === "completed" && (
            <span className="cp-umpire-match-action manage">
              Match Finished
            </span>
          )}

          {status === "cancelled" && (
            <span className="cp-umpire-match-action manage cancelled-action">
              Cancelled
            </span>
          )}

        </div>

        {/* EXISTING MATCH MANAGEMENT */}

        {status === "scheduled" && (
          <div className="cp-umpire-management">
            <MatchManagement
              match={match}
              onMatchUpdated={
                loadMatches
              }
            />
          </div>
        )}

      </article>
    );
  };

  if (loading) {
    return (
      <>
        <Navbar
          loggedInUser={
            loggedInUser
          }
          onMatches={
            onMatches
          }
          onPlayerProfile={
            onPlayerProfile
          }
          onUmpireDashboard={
            onUmpireDashboard
          }
          onCreateMatch={
            onCreateMatch
          }
          onBackToPlayerMode={
            onBackToPlayerMode
          }
          onLogout={
            onLogout
          }
        />

        <main className="cp-umpire-dashboard">
          <div className="cp-umpire-dashboard-container">
            <div className="cp-umpire-loading">
              <div className="cp-umpire-loader">
                <span />
                <span />
                <span />
              </div>

              <span>
                Loading your matches...
              </span>
            </div>
          </div>
        </main>
      </>
    );
  }

  return (
    <>
      {/* ================= NAVBAR ================= */}

      <Navbar
        loggedInUser={
          loggedInUser
        }

        onMatches={
          onMatches
        }

        onPlayerProfile={
          onPlayerProfile
        }

        onUmpireDashboard={
          onUmpireDashboard
        }

        onCreateMatch={
          onCreateMatch
        }

        onBackToPlayerMode={
          onBackToPlayerMode
        }

        onLogout={
          onLogout
        }
      />

      <main className="cp-umpire-dashboard">

        <div className="cp-umpire-dashboard-container">

          {/* ================= HEADER ================= */}

          <header className="cp-umpire-header">

            <div className="cp-umpire-header-copy">

              <span className="cp-umpire-eyebrow">
                UMPIRE WORKSPACE
              </span>

              <h1>
                Manage your matches.
              </h1>

              <p>
                Create, organize and score your
                local cricket matches from one place.
              </p>

            </div>

            <button
              type="button"
              className="cp-umpire-create-button"
              onClick={onCreateMatch}
            >
              <span>＋</span>
              Create Match
            </button>

          </header>

          {error && (
            <div className="cp-umpire-error">
              {error}
            </div>
          )}

          {/* ================= CREATE CARD ================= */}

          <section className="cp-umpire-create-card">

            <div className="cp-umpire-create-card-icon">
              🏏
            </div>

            <div className="cp-umpire-create-card-content">

              <span className="cp-umpire-create-card-label">
                ORGANIZE THE GAME
              </span>

              <h2>
                Ready to set up your next match?
              </h2>

              <p>
                Set up a local cricket match, add
                the participating players, and get
                ready to score the game live.
              </p>

            </div>

            <button
              type="button"
              className="cp-umpire-create-card-action"
              onClick={onCreateMatch}
            >
              Create Match
              <span>→</span>
            </button>

          </section>

          {/* ================= STATS ================= */}

          <section className="cp-umpire-stats">

            <div className="cp-umpire-stat-card created">

              <div className="cp-umpire-stat-icon">
                🏏
              </div>

              <div className="cp-umpire-stat-content">

                <span className="cp-umpire-stat-label">
                  Matches Created
                </span>

                <strong className="cp-umpire-stat-value">
                  {matches.length}
                </strong>

              </div>

            </div>

            <div className="cp-umpire-stat-card organized">

              <div className="cp-umpire-stat-icon">
                ✓
              </div>

              <div className="cp-umpire-stat-content">

                <span className="cp-umpire-stat-label">
                  Organized Successfully
                </span>

                <strong className="cp-umpire-stat-value">
                  {finishedMatches.length}
                </strong>

              </div>

            </div>

            <div className="cp-umpire-stat-card cancelled">

              <div className="cp-umpire-stat-icon">
                ×
              </div>

              <div className="cp-umpire-stat-content">

                <span className="cp-umpire-stat-label">
                  Cancelled Matches
                </span>

                <strong className="cp-umpire-stat-value">
                  {cancelledMatches.length}
                </strong>

              </div>

            </div>

          </section>

          {/* ================= MY MATCHES ================= */}

          <section className="cp-my-matches-section">

            <div className="cp-my-matches-heading">

              <div>
                <span className="cp-my-matches-eyebrow">
                  MATCH MANAGEMENT
                </span>

                <h2>
                  My Matches
                </h2>

                <p>
                  Manage and follow the matches
                  you organize.
                </p>
              </div>

            </div>

            {/* ================= TABS ================= */}

            <div className="cp-umpire-status-tabs">

              <button
                type="button"
                className={`cp-umpire-status-tab ${
                  activeTab === "Upcoming"
                    ? "active"
                    : ""
                }`}
                onClick={() => {
                  setActiveTab("Upcoming");
                  setShowAllMatches(false);
                }}
              >
                <span>
                  Upcoming
                </span>

                <span className="cp-umpire-status-count">
                  {upcomingMatches.length}
                </span>
              </button>

              <button
                type="button"
                className={`cp-umpire-status-tab ${
                  activeTab === "Live"
                    ? "active"
                    : ""
                }`}
                onClick={() => {
                  setActiveTab("Live");
                  setShowAllMatches(false);
                }}
              >
                <span>
                  Live
                </span>

                <span className="cp-umpire-status-count">
                  {liveMatches.length}
                </span>
              </button>

              <button
                type="button"
                className={`cp-umpire-status-tab ${
                  activeTab === "Finished"
                    ? "active"
                    : ""
                }`}
                onClick={() => {
                  setActiveTab("Finished");
                  setShowAllMatches(false);
                }}
              >
                <span>
                  Finished
                </span>

                <span className="cp-umpire-status-count">
                  {finishedMatches.length}
                </span>
              </button>

              <button
                type="button"
                className={`cp-umpire-status-tab ${
                  activeTab === "Cancelled"
                    ? "active"
                    : ""
                }`}
                onClick={() => {
                  setActiveTab("Cancelled");
                  setShowAllMatches(false);
                }}
              >
                <span>
                  Cancelled
                </span>

                <span className="cp-umpire-status-count">
                  {cancelledMatches.length}
                </span>
              </button>

            </div>

            {/* ================= MATCH GRID ================= */}

            {visibleMatches.length > 0 ? (
              <>
                <div className="cp-umpire-match-grid">
                  {visibleMatches.map(
                    renderMatchCard
                  )}
                </div>

                {hasMoreMatches && (
                  <div className="cp-umpire-view-more">

                    <button
                      type="button"
                      onClick={() =>
                        setShowAllMatches(
                          (current) =>
                            !current
                        )
                      }
                    >
                      {showAllMatches
                        ? "Show Less ↑"
                        : `View More (${tabMatches.length - INITIAL_VISIBLE_MATCHES}) ↓`}
                    </button>

                  </div>
                )}
              </>
            ) : (
              <div className="cp-umpire-empty">

                <div className="cp-umpire-empty-icon">
                  🏏
                </div>

                <h3>
                  No{" "}
                  {activeTab.toLowerCase()}{" "}
                  matches
                </h3>

                <p>
                  Matches in this category will
                  appear here when they are available.
                </p>

              </div>
            )}

          </section>

        </div>

      </main>
    </>
  );
};

export default UmpireDashboard;