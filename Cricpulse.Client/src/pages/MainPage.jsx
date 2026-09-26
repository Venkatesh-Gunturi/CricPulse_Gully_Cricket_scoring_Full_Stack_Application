import "./MainPage.css";
import Navbar from "../components/Layout/Navbar";
import Footer from "../components/Layout/Footer";

function MainPage({
  onLogin,
  onRegister,
  loggedInUser,
  onCreateMatch
}) {

   const isLoggedIn = !!loggedInUser;

  const isUmpire =
    loggedInUser?.isUmpire === true ||
    loggedInUser?.role?.toLowerCase() === "umpire";

  const isPlayer =
    isLoggedIn && !isUmpire;

  return (
    <div className="cricpulse-home">
      {/* ================= NAVBAR ================= */}
      <Navbar
        onLogin={onLogin}
        onRegister={onRegister}
        loggedInUser={loggedInUser}
      />

      {/* ================= HERO ================= */}
      <main>

        <section className="cp-hero">
          <div className="cp-hero-overlay" />

          <div className="cp-hero-content">

            <div className="cp-hero-copy">

              <div className="cp-eyebrow">
                <span className="cp-live-dot" />
                THE HOME OF GULLY CRICKET
              </div>

              <h1>
                Local Cricket.
                <br />
                <span>Real People.</span>
                <br />
                Real Matches.
              </h1>

              <p>
                Discover cricket happening around you.
                Play with your people, organize matches,
                follow live scores, and build your cricket
                journey with CricPulse.
              </p>

              <div className="cp-hero-actions">
                {!isLoggedIn ? (
                  <>
                    <button
                      type="button"
                      className="cp-primary-button"
                      onClick={onRegister}
                    >
                      <span>🏏</span>
                      Join CricPulse
                    </button>

                    <button
                      type="button"
                      className="cp-secondary-button"
                      onClick={onLogin}
                    >
                      Login
                      <span>→</span>
                    </button>
                  </>
                ) : isUmpire ? (
                  <button
                    type="button"
                    className="cp-primary-button"
                    onClick={onCreateMatch}
                  >
                    <span>🏏</span>
                    Create a Match
                  </button>
                ) : (
                  <button
                    type="button"
                    className="cp-primary-button"
                    onClick={() => {
                      window.location.hash = "player-profile";
                    }}
                  >
                    <span>📊</span>
                    View My Records
                  </button>
                )}
              </div>
            </div>

          <div className="cp-hero-visual" aria-hidden="true" />
          </div>

          <div className="cp-hero-scroll">
            <span>SCROLL TO EXPLORE</span>
            <span className="cp-scroll-line" />
          </div>
        </section>

        {/* ================= CTA SECTION ================= */}
        <section className="cp-action-section">

          <div className="cp-section-heading">
            <span>YOUR GAME. YOUR COMMUNITY.</span>

            <h2>
              What brings you
              <em> to the crease?</em>
            </h2>

            <p>
              CricPulse connects players, umpires and local
              cricket communities in one place.
            </p>
          </div>

      
    
          <div className="cp-action-grid">

            {/* ================= LIVE MATCHES ================= */}
            <button
              type="button"
              className="cp-action-card cp-action-live"
              onClick={() => {
                window.location.hash = "matches";
              }}
            >
              <div className="cp-action-image" />

              <div className="cp-action-overlay" />

              <div className="cp-card-number">
                01
              </div>

              <div className="cp-action-content">
                <span className="cp-action-label">
                  FOLLOW THE GAME
                </span>

                <h3>
                  View Live
                  <br />
                  Matches
                </h3>

                <p>
                  See matches happening now and
                  follow the action live.
                </p>
              </div>

              <div className="cp-card-arrow">
                ↗
              </div>
            </button>


            {/* ================= CREATE MATCH ================= */}
            <button
              type="button"
              className="cp-action-card cp-action-create"
              onClick={() => {
                if (isUmpire) {
                  onCreateMatch();
                } else {
                  onRegister();
                }
              }}
            >
             <div className="cp-action-image" />

              <div className="cp-action-overlay" />

              <div className="cp-card-number">
                02
              </div>

              <div className="cp-action-content">
                <span className="cp-action-label">
                  ORGANIZE THE GAME
                </span>

                <h3>
                  Create
                  <br />
                  a Match
                </h3>

                <p>
                  Bring your cricket crew together
                  and start a new match.
                </p>
              </div>

              <div className="cp-card-arrow">
                ↗
              </div>
            </button>


            {/* ================= PLAYER RECORDS ================= */}
            <button
              type="button"
              className="cp-action-card cp-action-records"
              onClick={() => {
                if (isPlayer) {
                  window.location.hash = "player-profile";
                } else {
                  onRegister();
                }
              }}
            >
              <div className="cp-action-image" />

              <div className="cp-action-overlay" />

              <div className="cp-card-number">
                03
              </div>

              <div className="cp-action-content">
                <span className="cp-action-label">
                  BUILD YOUR LEGACY
                </span>

                <h3>
                  See Your
                  <br />
                  Records
                </h3>

                <p>
                  Track your matches, runs, wickets
                  and cricket milestones.
                </p>
              </div>

              <div className="cp-card-arrow">
                ↗
              </div>
            </button>

          </div>




        </section>

        {/* ================= ABOUT ================= */}
        <section
          id="about"
          className="cp-about-section"
        >
          <div className="cp-about-inner">

            <div className="cp-about-badge">
              <span>CP</span>
            </div>

            <div className="cp-about-copy">
              <span className="cp-section-label">
                ABOUT CRICPULSE
              </span>

              <h2>
                Cricket doesn't need
                <br />
                a stadium to matter.
              </h2>

              <p>
                From streets and empty grounds to
                neighborhood pitches, local cricket
                has always been about people coming
                together.
              </p>

              <p>
                CricPulse gives that cricket a place
                to live — with real matches, real
                players, live scoring and records
                that follow the journey.
              </p>
            </div>

            <div className="cp-about-quote">
              <span>“</span>
              <p>
                Local Cricket.
                <br />
                Real People.
                <br />
                Real Matches.
              </p>
            </div>

          </div>
        </section>

      </main>
        
        {/* ================= FOOTER ================= */}
        <Footer/>
    

    </div>
  );
}

export default MainPage;

