import "../styles/cricket-iq.css";
function CricketIQ() {
  return (
    <main className="cricket-iq-page">

      {/* Page Introduction */}
      <section className="cricket-iq-hero">
        <h1>Cricket IQ</h1>
        <p>
          Understand the game. Know the rules. Play it your way.
        </p>
        <p>
          Whether you play professional cricket or enjoy weekend gully
          cricket with friends, Cricket IQ helps you understand the
          rules, scoring methods, and different ways the game is played.
        </p>
      </section>

      {/* Cricket Rules */}
      <section className="cricket-iq-section">
        <h2>Understanding Cricket Rules</h2>
        <p>
          Cricket has a standard set of laws, but local matches and
          tournaments may follow their own agreed playing conditions.
          Here are some important scoring concepts to understand.
        </p>

        <div className="cricket-iq-topics">

          <article className="cricket-iq-topic">
            <h3>Wide Ball</h3>
            <p>
              In standard cricket, a wide generally awards an extra run
              and the delivery does not count as a legal ball.
            </p>
            <p>
              In some gully cricket matches, players agree to different
              wide rules. For example, a wide may carry no extra run,
              while the delivery still does not count towards the over.
              CricPulse is designed to accommodate such local scoring
              variations.
            </p>
          </article>

          <article className="cricket-iq-topic">
            <h3>No Ball</h3>
            <p>
              A no-ball is an illegal delivery under the applicable
              playing conditions. It normally awards an extra run and
              does not count as a legal delivery.
            </p>
            <p>
              Professional limited-overs cricket commonly uses a free
              hit after certain no-balls. However, not every local match
              follows the free-hit rule.
            </p>
            <p>
              If a local match does not use free hits, CricPulse also
              provides a Wide + Runs scoring option. This allows the
              umpire to record the agreed extra and completed runs
              without counting the delivery as a legal ball.
            </p>
          </article>

          <article className="cricket-iq-topic">
            <h3>Byes</h3>
            <p>
              Byes are runs scored when the ball passes the batter
              without touching the bat or body, and the batters run
              or the ball reaches the boundary.
            </p>
            <p>
              Some local matches include byes, while others may not.
              CricPulse supports bye scoring where it is part of the
              match rules.
            </p>
          </article>

          <article className="cricket-iq-topic">
            <h3>Leg Byes</h3>
            <p>
              Leg byes are runs scored when the ball hits the batter's
              body, rather than the bat, and the batters meet the
              conditions required to take those runs.
            </p>
            <p>
              Leg byes are used in standard cricket and can also be
              included in local match scoring when agreed by the teams.
            </p>
          </article>

          <article className="cricket-iq-topic">
            <h3>Free Hit</h3>
            <p>
              A free hit gives the batter special protection from
              certain dismissals on the next delivery after a qualifying
              no-ball, according to the applicable playing conditions.
            </p>
            <p>
              Local matches can decide whether free hits are included
              in their rules.
            </p>
          </article>

          <article className="cricket-iq-topic">
            <h3>Undo Last Ball</h3>
            <p>
              Made a scoring mistake? CricPulse provides an Undo Last
              Ball option so the umpire can correct the most recently
              recorded delivery.
            </p>
            <p>
              Only one ball can be undone at a time. After undoing,
              another undo is disabled until a new ball is scored.
              This helps prevent accidental multiple reversals.
            </p>
          </article>

        </div>
      </section>

      {/* CricPulse Scoring */}
      <section className="cricket-iq-section">
        <h2>One Scoring System, Different Ways to Play</h2>
        <p>
          CricPulse is designed for different types of cricket,
          including professional-style scoring and local gully cricket
          variations.
        </p>
        <p>
          Match scoring depends on the rules agreed upon by the teams.
          Umpires should understand those rules before recording
          deliveries, extras, wickets, and completed runs.
        </p>
      </section>

      {/* Official Resources */}
      <section className="cricket-iq-section cricket-iq-resources">
        <h2>Want to Learn More?</h2>
        <p>
          Explore these official cricket resources for detailed laws
          and playing conditions.
        </p>

        <div className="cricket-iq-resource-links">
          <a
            href="https://www.lords.org/mcc/about-the-laws-of-cricket"
            target="_blank"
            rel="noopener noreferrer"
          >
            MCC – Laws of Cricket
          </a>

          <a
            href="https://www.icc-cricket.com/about/cricket/rules-and-regulations/playing-conditions"
            target="_blank"
            rel="noopener noreferrer"
          >
            ICC – Playing Conditions
          </a>
        </div>
      </section>

      {/* Feedback */}
      <section className="cricket-iq-feedback">
        <h2>Help Us Improve CricPulse</h2>
        <p>
          Spotted an incorrect score, misleading scoring information,
          a bug, or something that could work better? or also if you are looking for a better feature in the application or better replacement feature in the current existing feature, Raise an E-Mail.
        </p>
        <p>
          Don't hesitate to reach out. Your feedback matters, and
          every suggestion helps us make CricPulse better for
          cricket communities.
        </p>

        <a
          className="cricket-iq-contact-link"
          href="mailto:nani66035@gmail.com?subject=CricPulse%20Feedback or Report or Requesting a feature!!!"
        >
          Report an Issue 
        </a>
      </section>

    </main>
  );
}

export default CricketIQ;
