
import { Link } from 'react-router-dom';
import '../styles/home.css';
import heroImage from '../assets/hero-bg.jpg';
import matchesImage from '../assets/matches-card.jpg';
import statsImage from '../assets/stats-card.jpg';
import createMatchImage from '../assets/create-match-card.jpg';

const features = [
  {
    eyebrow: 'DISCOVER THE GAME',
    title: 'Matches Near You',
    description:
      'Find cricket matches, follow the action, and discover your next game.',
    link: '/matches',
    linkText: 'Explore Matches',
    image: matchesImage,
    imageAlt: 'Cricket scoreboard and players in action',
  },
  {
    eyebrow: 'YOUR CRICKET JOURNEY',
    title: 'Check Your Records',
    description:
      'Keep your performances and achievements close as your cricket story grows.',
    link: '/player/profile',
    linkText: 'View Player Stats',
    image: statsImage,
    imageAlt: 'Cricket player with a player statistics panel',
  },
  {
    eyebrow: 'BRING THE GAME TO LIFE',
    title: 'Create a Match',
    description:
      'Organize a match, manage your games, and follow the action from your dashboard.',
    link: '/umpire',
    linkText: 'Open Dashboard',
    image: createMatchImage,
    imageAlt: 'Cricket bat, ball, and stumps on a pitch',
  },
];

const communityBenefits = [
  {
    icon: '🏏',
    title: 'Play Local',
    description: 'Find opportunities to get out on the field.',
  },
  {
    icon: '📊',
    title: 'Track Progress',
    description: 'Follow your performances and celebrate milestones.',
  },
  {
    icon: '🏆',
    title: 'Organize Matches',
    description: 'Bring players together for your next game.',
  },
  {
    icon: '🤝',
    title: 'Connect Through Cricket',
    description: 'Be part of a community that loves the game.',
  },
];

function Home() {
  return (
    <div className="home-page">
      {/* Image-only hero with white fading edges */}
      <section
        className="home-hero"
        style={{ backgroundImage: `url(${heroImage})` }}
        aria-label="Cricket stadium at sunset"
      />

      {/* Main feature cards */}
      <section className="home-features" aria-labelledby="home-features-title">
        <div className="home-section-heading">
          <span className="home-section-eyebrow">
            YOUR NEXT INNINGS STARTS HERE
          </span>

          <h2 id="home-features-title">
            Your Game. Your <span>Choice.</span>
          </h2>

          <p>
            Find a match, follow your cricket journey, or bring a game of your
            own to life.
          </p>
        </div>

        <div className="home-feature-grid">
          {features.map((feature, index) => (
            <Link
              to={feature.link}
              className="home-feature-card"
              key={feature.title}
            >
              <img
                src={feature.image}
                alt={feature.imageAlt}
                className="home-feature-image"
                loading={index === 0 ? 'eager' : 'lazy'}
              />

              <div className="home-feature-overlay" />

              <div className="home-feature-content">
                <span className="home-feature-eyebrow">
                  {feature.eyebrow}
                </span>

                <h3>{feature.title}</h3>
                <p>{feature.description}</p>

                <span className="home-feature-link">
                  {feature.linkText}
                  <span aria-hidden="true"> →</span>
                </span>
              </div>

              <span className="home-feature-number" aria-hidden="true">
                0{index + 1}
              </span>
            </Link>
          ))}
        </div>
      </section>

      {/* Community section */}
      <section className="home-community">
        <div className="home-community-heading">
          <span className="home-section-eyebrow">
            MORE THAN JUST CRICKET
          </span>

          <h2>
            Be Part of the <span>CricPulse</span> Community
          </h2>

          <p>
            Every player has a story. Every match creates a memory. Let’s make
            yours count.
          </p>
        </div>

        <div className="home-benefits-grid">
          {communityBenefits.map((benefit) => (
            <div className="home-benefit" key={benefit.title}>
              <span className="home-benefit-icon" aria-hidden="true">
                {benefit.icon}
              </span>

              <h3>{benefit.title}</h3>
              <p>{benefit.description}</p>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}

export default Home;
