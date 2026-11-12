import "../styles/about.css";
function About() {
  return (
    <main className="about-page">
      {/* About CricPulse */}
      <section className="mb-5">
        <h1>About CricPulse</h1>
        <p>
          CricPulse brings local cricket communities together by making
          gully cricket matches easier to create, manage, follow, and enjoy.
          From live scoring to player profiles and match results, CricPulse
          keeps everything in one place.
        </p>
      </section>

      {/* What You Can Do */}
      <section className="mb-5">
        <h2>What Can You Do with CricPulse?</h2>
        <ul>
          <li>Explore upcoming, live, and completed local matches.</li>
          <li>Follow live scores and match progress.</li>
          <li>Create and manage matches as an umpire.</li>
          <li>View player profiles and performance statistics.</li>
          <li>Explore cricket-related content through Cricket IQ.</li>
        </ul>
      </section>

      {/* User Roles */}
      <section className="mb-5">
        <h2>Choose Your Role</h2>

        <h5>Guest / User</h5>
        <p>
          Browse CricPulse, explore matches, and follow match details
          without creating an account.
        </p>

        <h5>Player</h5>
        <p>
          Register to access your player profile and view your cricket
          performance and statistics.
        </p>

        <h5>Umpire</h5>
        <p>
          Create and manage matches, add players, start matches, and
          maintain live scores.
        </p>
      </section>

      {/* Registration and Login */}
      <section className="mb-5">
        <h2>Registration & Login</h2>
        <p>
          Players can create an account using their first name, email,
          mobile number, and password. Once registered, they can log in
          using either their email address or mobile number along with
          their password.
        </p>
      </section>

      {/* Automatic Account Creation */}
      <section className="mb-5">
        <h2>Automatic Player Account Creation</h2>
        <p>
          When an umpire creates a match, they enter the required player
          details, such as name, mobile number, and email. If a player
          doesn't already have an account, CricPulse automatically creates
          one with a randomly generated password.
        </p>
        <p>
          The player can then access their account by using the
          <strong> Forgot Password </strong>
          option with the same mobile number and email provided during
          match creation to set a new password.
        </p>
      </section>

      {/* Forgot Password */}
      <section className="mb-5">
        <h2>Forgot Password?</h2>
        <p>
          If you forget your password, use the Forgot Password option.
          Enter your registered mobile number and email address, then
          provide and confirm your new password to reset it.
        </p>
      </section>

      {/* Navigation */}
      <section>
        <h2>Explore CricPulse</h2>
        <p>
          Visit Home to get started, Matches to explore local games,
          and Cricket IQ to discover cricket-related content.
        </p>
      </section>
    </main>
  );
}

export default About;