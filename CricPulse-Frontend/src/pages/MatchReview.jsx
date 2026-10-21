import { useEffect, useState } from 'react';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { FaArrowLeft, FaCalendarAlt, FaEdit, FaMapMarkerAlt, FaShieldAlt, FaUsers } from 'react-icons/fa';
import { getMatchById } from '../services/matchService';
import Toast from '../components/Toast';
import { useAuth } from '../context/AuthContext';
import '../styles/match-review.css';

const getValue = (object, ...keys) => {
  for (const key of keys) {
    if (object?.[key] !== undefined && object[key] !== null) return object[key];
  }
  return null;
};

const formatDateTime = (value) => {
  if (!value) return 'Not available';
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? 'Not available' : date.toLocaleString('en-IN', {
    dateStyle: 'medium', timeStyle: 'short',
  });
};

export default function MatchReview() {
  const { matchId } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const { user } = useAuth();
  const organizerUserId = getValue(user, 'userId', 'UserId', 'id', 'userID');
  const [match, setMatch] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [toastMessage, setToastMessage] = useState('');

  useEffect(() => {
    if (!matchId || !organizerUserId) {
      setError('Your umpire account could not be identified. Please sign in again.');
      setLoading(false);
      return;
    }
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError('');
      try {
        const result = await getMatchById(matchId, organizerUserId);
        if (!cancelled) setMatch(result);
      } catch (requestError) {
        if (!cancelled) setError(
          requestError.response?.data?.message || requestError.response?.data?.title ||
          requestError.message || 'Unable to load this match.'
        );
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [matchId, organizerUserId]);

  useEffect(() => {
    setToastMessage(location.state?.toastMessage || '');
  }, [location.key, location.state]);

  const players = getValue(match, 'players', 'Players') || [];
  const teamA = players.filter((player) => getValue(player, 'team', 'Team') === 'TeamA');
  const teamB = players.filter((player) => getValue(player, 'team', 'Team') === 'TeamB');
  const closeReview = () => navigate('/umpire', {
    replace: true,
    state: null,
  });

  const editMatch = () => navigate(`/umpire/matches/${matchId}/edit`);

  if (loading) return <main className="match-review-page"><p className="match-review-status">Loading saved match…</p></main>;
  if (error) return (
    <main className="match-review-page">
      <div className="match-review-alert" role="alert">{error}</div>
      <button type="button" className="match-review-primary" onClick={() => navigate('/umpire')}>Close and return to dashboard</button>
    </main>
  );
  if (!match) return null;

  const teamAName = getValue(match, 'teamAName', 'TeamAName') || 'Team A';
  const teamBName = getValue(match, 'teamBName', 'TeamBName') || 'Team B';

  const renderTeam = (teamName, roster) => (
    <section className="match-review-team" key={teamName}>
      <h3>{teamName} <span>{roster.length} players</span></h3>
      {roster.length ? <ol>{roster.map((player, index) => {
        const name = getValue(player, 'playerName', 'PlayerName') ||
          [getValue(player, 'firstName', 'FirstName'), getValue(player, 'lastName', 'LastName')].filter(Boolean).join(' ') ||
          'Player';
        return <li key={getValue(player, 'userId', 'UserId') || `${teamName}-${index}`}>
          <span className="match-review-player-number">{index + 1}</span>
          <span className="match-review-player-info"><strong>{name}</strong>
            <small>{getValue(player, 'mobileNumber', 'MobileNumber') || 'No mobile'} · {getValue(player, 'email', 'Email') || 'No email'}</small>
          </span>
          {index === 0 && <span className="match-review-captain">Captain</span>}
        </li>;
      })}</ol> : <p>No players returned for this team.</p>}
    </section>
  );

  return (
    <main className="match-review-page">
      <Toast message={toastMessage} type="success" onClose={() => setToastMessage('')} />
      <header className="match-review-topbar">
        <span>CRICPULSE · UMPIRE</span>
        <span className="match-review-saved">Saved match</span>
      </header>
      <section className="match-review-heading">
        <span className="match-review-eyebrow">MATCH REVIEW</span>
        <h1>{getValue(match, 'name', 'Name') || 'Match details'}</h1>
        <p>Review the saved details and player roster. You can update the match or close this page.</p>
      </section>
      <section className="match-review-summary">
        <article className="match-review-team-name"><span>TEAM A</span><h2>{teamAName}</h2></article>
        <div className="match-review-vs">VS</div>
        <article className="match-review-team-name"><span>TEAM B</span><h2>{teamBName}</h2></article>
      </section>
      <section className="match-review-details">
        <h2>Match details</h2>
        <div className="match-review-detail-grid">
          <div><FaCalendarAlt /><span><small>Date and time</small><strong>{formatDateTime(getValue(match, 'scheduledAt', 'ScheduledAt'))}</strong></span></div>
          <div><FaMapMarkerAlt /><span><small>Venue</small><strong>{getValue(match, 'venue', 'Venue') || 'Not provided'}</strong><small>{getValue(match, 'locationAddress', 'LocationAddress') || ''}</small></span></div>
          <div><FaShieldAlt /><span><small>Match settings</small><strong>{getValue(match, 'overs', 'Overs') ?? '—'} overs · {getValue(match, 'playersPerTeam', 'PlayersPerTeam') ?? '—'} players per team</strong></span></div>
          <div><FaUsers /><span><small>Status</small><strong>{getValue(match, 'status', 'Status') || 'Upcoming'}</strong></span></div>
        </div>
      </section>
      <section className="match-review-rosters">
        <h2>Player roster</h2>
        <div className="match-review-roster-grid">
          {renderTeam(teamAName, teamA)}
          {renderTeam(teamBName, teamB)}
        </div>
      </section>
      <footer className="match-review-actions">
        <button type="button" className="match-review-secondary" onClick={closeReview}><FaArrowLeft /> Back to Dashboard</button>
        <button type="button" className="match-review-primary" onClick={editMatch}><FaEdit /> Update Match</button>
      </footer>
    </main>
  );
}
