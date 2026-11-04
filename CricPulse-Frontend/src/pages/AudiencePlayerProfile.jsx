import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import {
  FaArrowLeft,
  FaAward,
  FaBaseballBall,
  FaBullseye,
  FaSpinner,
  FaUser,
} from 'react-icons/fa';

import apiClient from '../services/apiClient';
import '../styles/audience-player-profile.css';

const EMPTY_VALUE = '—';

function read(object, ...keys) {
  for (const key of keys) {
    const value = object?.[key];
    if (value !== null && value !== undefined && value !== '') {
      return value;
    }
  }
  return null;
}

function display(value, suffix = '') {
  return value === null || value === undefined || value === ''
    ? EMPTY_VALUE
    : `${value}${suffix}`;
}

function StatCard({ label, value, note }) {
  return (
    <article className="audience-player-stat-card">
      <strong>{display(value)}</strong>
      <span>{label}</span>
      {note && <small>{note}</small>}
    </article>
  );
}

function Detail({ label, value }) {
  return (
    <div className="audience-player-detail">
      <span>{label}</span>
      <strong>{display(value)}</strong>
    </div>
  );
}

export default function AudiencePlayerProfile() {
  const { userId } = useParams();
  const navigate = useNavigate();

  const [profile, setProfile] = useState(null);
  const [activeTab, setActiveTab] = useState('batting');
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    let isMounted = true;

    async function loadPublicProfile() {
      if (!userId) {
        setError('Player ID is missing.');
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        setError('');

        const response = await apiClient.get(
          `/AccountProfile/public/${userId}`
        );

        if (isMounted) {
          setProfile(response.data);
        }
      } catch (requestError) {
        if (!isMounted) return;

        const status = requestError.response?.status;

        if (status === 404) {
          setError('This player profile could not be found.');
        } else {
          setError(
            requestError.response?.data?.message ||
              requestError.response?.data?.title ||
              'Unable to load this player profile. Please try again.'
          );
        }
      } finally {
        if (isMounted) setLoading(false);
      }
    }

    loadPublicProfile();

    return () => {
      isMounted = false;
    };
  }, [userId]);

  const stats = read(profile, 'statistics', 'Statistics') || {};

  const firstName = read(profile, 'firstName', 'FirstName') || '';
  const lastName = read(profile, 'lastName', 'LastName') || '';
  const playerName = `${firstName} ${lastName}`.trim() || 'Player';

  const imageUrl = read(profile, 'profileImageUrl', 'ProfileImageUrl');

  const initials = playerName
    .split(/\s+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part.charAt(0).toUpperCase())
    .join('');

  const matches = Number(read(stats, 'matches', 'Matches') ?? 0);
  const runs = Number(read(stats, 'runs', 'Runs') ?? 0);
  const innings = Number(read(stats, 'innings', 'Innings') ?? 0);
  const notOuts = Number(read(stats, 'notOuts', 'NotOuts') ?? 0);
  const ballsFaced = Number(read(stats, 'ballsFaced', 'BallsFaced') ?? 0);
  const ballsBowled = Number(read(stats, 'ballsBowled', 'BallsBowled') ?? 0);
  const runsConceded = Number(read(stats, 'runsConceded', 'RunsConceded') ?? 0);

  const battingAverage = useMemo(() => {
    const dismissals = innings - notOuts;
    return dismissals > 0 ? (runs / dismissals).toFixed(2) : null;
  }, [innings, notOuts, runs]);

  const strikeRate = useMemo(() => (
    ballsFaced > 0 ? ((runs / ballsFaced) * 100).toFixed(2) : null
  ), [ballsFaced, runs]);

  const economy = useMemo(() => (
    ballsBowled > 0 ? ((runsConceded / ballsBowled) * 6).toFixed(2) : null
  ), [ballsBowled, runsConceded]);

  const battingStats = [
    { label: 'Innings', value: read(stats, 'innings', 'Innings') },
    { label: 'Runs', value: read(stats, 'runs', 'Runs') },
    { label: 'Balls Faced', value: read(stats, 'ballsFaced', 'BallsFaced') },
    { label: 'Highest Score', value: read(stats, 'highestScore', 'HighestScore') },
    { label: 'Not Outs', value: read(stats, 'notOuts', 'NotOuts') },
    { label: 'Fours', value: read(stats, 'fours', 'Fours') },
    { label: 'Sixes', value: read(stats, 'sixes', 'Sixes') },
    { label: 'Fifties', value: read(stats, 'fifties', 'Fifties') },
    { label: 'Hundreds', value: read(stats, 'hundreds', 'Hundreds') },
    { label: 'Average', value: battingAverage },
    { label: 'Strike Rate', value: strikeRate },
  ];

  const bowlingStats = [
    { label: 'Bowling Innings', value: read(stats, 'bowlingInnings', 'BowlingInnings') },
    { label: 'Balls Bowled', value: read(stats, 'ballsBowled', 'BallsBowled') },
    { label: 'Runs Conceded', value: read(stats, 'runsConceded', 'RunsConceded') },
    { label: 'Wickets', value: read(stats, 'wickets', 'Wickets') },
    { label: 'Maidens', value: read(stats, 'maidens', 'Maidens') },
    { label: 'Economy', value: economy },
    { label: '3-Wicket Hauls', value: read(stats, 'threeWicketHauls', 'ThreeWicketHauls') },
    { label: '5-Wicket Hauls', value: read(stats, 'fiveWicketHauls', 'FiveWicketHauls') },
    { label: 'Best Bowling', value: read(stats, 'bestBowling', 'BestBowling') },
  ];

  const hasStatistics = matches > 0;

  const handleBack = () => {
    if (window.history.length > 1) navigate(-1);
    else navigate('/matches');
  };

  return (
    <main className="audience-player-page">
      <div className="audience-player-shell">
        <header className="audience-player-topbar">
          <button type="button" onClick={handleBack}>
            <FaArrowLeft />
            <span>Back</span>
          </button>
          <h1>Player Profile</h1>
          <span className="audience-player-topbar-spacer" />
        </header>

        {loading ? (
          <section className="audience-player-state" role="status">
            <FaSpinner className="audience-player-spinner" />
            <p>Loading player profile...</p>
          </section>
        ) : error ? (
          <section className="audience-player-state audience-player-error" role="alert">
            <FaUser />
            <h2>Profile unavailable</h2>
            <p>{error}</p>
            <button type="button" onClick={() => window.location.reload()}>
              Try Again
            </button>
          </section>
        ) : profile ? (
          <>
            <section className="audience-player-hero">
              <div className="audience-player-avatar-wrap">
                {imageUrl ? (
                  <img src={imageUrl} alt={`${playerName} profile`} />
                ) : (
                  <div className="audience-player-avatar-fallback">
                    {initials || 'P'}
                  </div>
                )}
                <span className="audience-player-avatar-badge">
                  <FaBaseballBall />
                </span>
              </div>

              <p className="audience-player-eyebrow">CRICPULSE PLAYER</p>
              <h2>{playerName}</h2>
              <p className="audience-player-role">
                {display(read(profile, 'playerRole', 'PlayerRole'), '')}
              </p>

              <div className="audience-player-quick-stats">
                <div>
                  <strong>{matches}</strong>
                  <span>Matches</span>
                </div>
                <div>
                  <strong>{display(read(stats, 'runs', 'Runs'), '')}</strong>
                  <span>Runs</span>
                </div>
                <div>
                  <strong>{display(read(stats, 'wickets', 'Wickets'), '')}</strong>
                  <span>Wickets</span>
                </div>
              </div>
            </section>

            <section className="audience-player-panel">
              <div className="audience-player-section-heading">
                <div>
                  <span className="audience-player-eyebrow">PLAYER INFORMATION</span>
                  <h2>About Player</h2>
                </div>
              </div>

              <div className="audience-player-details-grid">
                <Detail label="Age" value={read(profile, 'age', 'Age')} />
                <Detail label="Gender" value={read(profile, 'gender', 'Gender')} />
                <Detail label="State" value={read(profile, 'state', 'State')} />
                <Detail label="Player Role" value={read(profile, 'playerRole', 'PlayerRole')} />
                <Detail label="Batting Style" value={read(profile, 'battingStyle', 'BattingStyle')} />
                <Detail label="Bowling Style" value={read(profile, 'bowlingStyle', 'BowlingStyle')} />
              </div>
            </section>

            <section className="audience-player-panel">
              <div className="audience-player-section-heading">
                <div>
                  <span className="audience-player-eyebrow">CAREER PERFORMANCE</span>
                  <h2>Player Statistics</h2>
                </div>
                <span className="audience-player-match-count">
                  {matches} {matches === 1 ? 'Match' : 'Matches'}
                </span>
              </div>

              {!hasStatistics ? (
                <div className="audience-player-empty-stats">
                  <FaBullseye />
                  <h3>No match statistics yet</h3>
                  <p>Player statistics will appear here after they participate in a match.</p>
                </div>
              ) : (
                <>
                  <div className="audience-player-stat-highlights">
                    <StatCard label="Matches" value={matches} />
                    <StatCard label="Runs" value={runs} />
                    <StatCard label="Wickets" value={read(stats, 'wickets', 'Wickets')} />
                    <StatCard label="MVP Awards" value={read(stats, 'mvpCount', 'MVPCount')} />
                  </div>

                  <div className="audience-player-tabs" role="tablist" aria-label="Player statistics">
                    <button
                      type="button"
                      role="tab"
                      aria-selected={activeTab === 'batting'}
                      className={activeTab === 'batting' ? 'active' : ''}
                      onClick={() => setActiveTab('batting')}
                    >
                      Batting
                    </button>
                    <button
                      type="button"
                      role="tab"
                      aria-selected={activeTab === 'bowling'}
                      className={activeTab === 'bowling' ? 'active' : ''}
                      onClick={() => setActiveTab('bowling')}
                    >
                      Bowling
                    </button>
                  </div>

                  <div className="audience-player-stat-grid">
                    {(activeTab === 'batting' ? battingStats : bowlingStats).map((item) => (
                      <StatCard key={item.label} label={item.label} value={item.value} />
                    ))}
                  </div>

                  <div className="audience-player-fielding">
                    <h3><FaAward /> Fielding & Awards</h3>
                    <div className="audience-player-details-grid">
                      <Detail label="Catches" value={read(stats, 'catches', 'Catches')} />
                      <Detail label="Run Outs" value={read(stats, 'runOuts', 'RunOuts')} />
                      <Detail label="Stumpings" value={read(stats, 'stumpings', 'Stumpings')} />
                      <Detail label="MVP Awards" value={read(stats, 'mvpCount', 'MVPCount')} />
                    </div>
                  </div>
                </>
              )}
            </section>
          </>
        ) : null}
      </div>
    </main>
  );
}