import { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  FaMapMarkerAlt,
  FaRedo,
  FaSpinner,
} from 'react-icons/fa';

import { getMatches } from '../services/matchService';
import '../styles/umpire-dashboard.css';

function getValue(object, ...keys) {
  for (const key of keys) {
    if (object?.[key] !== undefined && object[key] !== null) {
      return object[key];
    }
  }

  return null;
}

// Prefer a populated casing variant if the API happens to return both.
// A genuine zero is preserved when neither variant contains a positive value.
function getPlayersPerTeam(match) {
  const values = [match?.playersPerTeam, match?.PlayersPerTeam];
  const populatedValue = values.find(
    (value) => value !== undefined && value !== null && value !== '' && Number(value) > 0
  );

  if (populatedValue !== undefined) return populatedValue;

  return values.find((value) => value !== undefined && value !== null) ?? null;
}

function formatDate(value) {
  if (!value) return 'Date not available';

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return 'Date not available';

  return date.toLocaleDateString('en-IN', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });
}

function formatTime(value) {
  if (!value) return '';

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return '';

  return date.toLocaleTimeString('en-IN', {
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  });
}

function getTeamScore(innings, teamKey) {
  if (!Array.isArray(innings)) return null;

  return innings.find((inning) => {
    const battingTeam = String(
      getValue(inning, 'battingTeam', 'BattingTeam') || ''
    )
      .toLowerCase()
      .replace(/[\s_-]/g, '');

    return battingTeam === teamKey.toLowerCase();
  });
}

function MatchCardTitle({ name }) {
  return (
    <h3 className="my-match-title audience-match-title" title={name}>
      {name}
    </h3>
  );
}

function Matches() {
  const navigate = useNavigate();
  const [nearbyMatches, setNearbyMatches] = useState([]);
const [loading, setLoading] = useState(true);
const [error, setError] = useState('');
const [locationBlocked, setLocationBlocked] = useState(false);

const [selectedStatus, setSelectedStatus] = useState('Upcoming');

const filteredMatches = nearbyMatches.filter(
  (match) =>
    String(getValue(match, 'status', 'Status') || '')
      .toLowerCase() === selectedStatus.toLowerCase()
);
  

  const loadNearbyMatches = () => {
    if (!navigator.geolocation) {
      setError('Your browser does not support location access.');
      setLoading(false);
      return;
    }

    setLoading(true);
    setError('');
    setLocationBlocked(false);

    navigator.geolocation.getCurrentPosition(
      async (position) => {
        try {
          const { latitude, longitude } = position.coords;

          const response = await getMatches({
            latitude,
            longitude,
            radiusKm: 30,
          });

          const matches = Array.isArray(response)
            ? response
            : response?.matches || response?.data || [];

          setNearbyMatches(matches);
        } catch (err) {
          setError(
            err.response?.data?.message || 'Unable to load nearby matches.'
          );
        } finally {
          setLoading(false);
        }
      },
      (geoError) => {
        setLoading(false);

        if (geoError.code === geoError.PERMISSION_DENIED) {
          setLocationBlocked(true);
          setError(
            'Location access is blocked. Please enable location permission in your browser settings to discover nearby matches.'
          );
        } else if (geoError.code === geoError.TIMEOUT) {
          setError('Location request timed out. Please try again.');
        } else {
          setError('Unable to detect your location. Please try again.');
        }
      },
      {
        enableHighAccuracy: true,
        timeout: 15000,
        maximumAge: 0,
      }
    );
  };

  useEffect(() => {
    loadNearbyMatches();
    // Load nearby matches once when the page mounts.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const handleMatchClick = (match) => {
    const matchId = getValue(match, 'matchId', 'MatchId', 'id', 'Id');

    if (!matchId) return;

    navigate(`/umpire/matches/${matchId}`, {
      state: {
        viewMode: 'audience',
        returnTo: '/matches',
      },
    });
  };

  const renderMatchCard = (match, index) => {
    const id = getValue(match, 'matchId', 'MatchId', 'id', 'Id');

    const name =
      getValue(match, 'name', 'Name') ||
      `${getValue(match, 'teamAName', 'TeamAName') || 'Team A'} vs ${
        getValue(match, 'teamBName', 'TeamBName') || 'Team B'
      }`;

    const teamA = getValue(match, 'teamAName', 'TeamAName') || 'Team A';
    const teamB = getValue(match, 'teamBName', 'TeamBName') || 'Team B';

    const scheduledAt = getValue(match, 'scheduledAt', 'ScheduledAt');
    const status = getValue(match, 'status', 'Status') || 'Upcoming';
    const statusKey = String(status).toLowerCase();

    const overs = getValue(match, 'overs', 'Overs');
    const playersPerTeam = getPlayersPerTeam(match);

    const distance = getValue(match, 'distanceKm', 'DistanceKm');
    const venue = getValue(match, 'venue', 'Venue');
    const state = getValue(match, 'state', 'State');
    const innings = getValue(match, 'innings', 'Innings') || [];

    const teamAInnings = getTeamScore(innings, 'TeamA');
    const teamBInnings = getTeamScore(innings, 'TeamB');

    const resultText = getValue(match, 'resultText', 'ResultText');

    const cancellationReason =
      getValue(match, 'cancellationReason', 'CancellationReason') ||
      getValue(match, 'cancellationReasonType', 'CancellationReasonType');

    const renderScore = (inning) => {
      if (!inning) return '—';

      const runs = getValue(inning, 'totalRuns', 'TotalRuns') ?? 0;
      const wickets = getValue(inning, 'wickets', 'Wickets') ?? 0;

      return `${runs}/${wickets}`;
    };

    const renderOvers = (inning) => {
      if (!inning) return '';

      return `${getValue(inning, 'totalOvers', 'TotalOvers') ?? 0} overs`;
    };

    const currentInnings = Array.isArray(innings)
      ? innings.find((item) =>
          ['live', 'inprogress', 'in progress'].includes(
            String(getValue(item, 'status', 'Status') || '')
              .toLowerCase()
              .replace(/[_-]/g, ' ')
          )
        )
      : null;

    const liveBattingTeam = currentInnings
      ? getValue(currentInnings, 'battingTeam', 'BattingTeam')
      : null;

    return (
      <article
        className={`umpire-match-card my-match-card audience-match-card status-card-${statusKey}`}
        key={id || index}
        role="link"
        tabIndex={0}
        aria-label={`View ${name}`}
        onClick={() => handleMatchClick(match)}
        onKeyDown={(event) => {
          if (event.key === 'Enter' || event.key === ' ') {
            event.preventDefault();
            handleMatchClick(match);
          }
        }}
      >
        {/* Date, distance and status */}
        <div className="audience-card-header">
          <div className="my-match-date">
            <span>{formatDate(scheduledAt)}</span>
            <span>{formatTime(scheduledAt) || 'Time not available'}</span>
          </div>

          {(statusKey === 'upcoming' || statusKey === 'live') &&
distance != null ? (
  <span className="my-match-distance-pill">
    <FaMapMarkerAlt />
    {Number(distance).toFixed(2)} km away
  </span>
) : (
  <span />
)}

          <span className={`my-match-status status-${statusKey}`}>
            {statusKey === 'live' && (
              <span className="live-pulse-dot" aria-hidden="true" />
            )}
            {status}
          </span>
        </div>

        {/* Match name */}
        <MatchCardTitle name={name} />

        {/* Teams and scores */}
        <div className="audience-scoreboard">
          <div className="audience-team-column">
            <strong className="audience-team-name">{teamA}</strong>
            {(statusKey === 'live' || statusKey === 'finished') && (
              <>
                <strong className="audience-team-runs">
                  {renderScore(teamAInnings)}
                </strong>
                <span className="audience-team-overs">
                  {renderOvers(teamAInnings)}
                </span>
              </> 
            )}
          </div>

          <span className="audience-score-vs">VS</span>

          <div className="audience-team-column">
            <strong className="audience-team-name">{teamB}</strong>
            {(statusKey === 'live' || statusKey === 'finished') && (
              <>
                <strong className="audience-team-runs">
                  {renderScore(teamBInnings)}
                </strong>
                <span className="audience-team-overs">
                  {renderOvers(teamBInnings)}
                </span>
              </>
            )}
          </div>
        </div>

        {/* One result/cancellation area, followed by venue and state */}
        <div className="audience-match-result audience-result-location-box">
          {statusKey === 'live' && (
            <div className="audience-result-text">
              <strong>LIVE MATCH</strong>
              <span>{liveBattingTeam || 'Current innings in progress'}</span>
            </div>
          )}

          {statusKey === 'finished' && (
            <div className="audience-result-text">
              {resultText || 'Match completed'}
            </div>
          )}

          {statusKey === 'cancelled' && (
            <div className="audience-result-text">
              <strong>Match Cancelled</strong>
              <span>{cancellationReason || 'Reason not recorded'}</span>
            </div>
          )}

          <div className="audience-match-location-row">
            <span>
              <strong>Venue:</strong> {venue || 'Unavailable'}
            </span>
            <span>
              <strong>State:</strong> {state || 'Unavailable'}
            </span>
          </div>
        </div>

        {/* Match footer */}
        <div className="audience-match-footer">
          <span className="my-match-meta-pill">Overs: {overs ?? '—'}</span>
          <span className="my-match-meta-pill">
            Per team: {playersPerTeam ?? '—'}
          </span>
        </div>
      </article>
    );
  };

  return (
    <main className="umpire-dashboard audience-matches-page">
      <section className="umpire-matches-section">
        <div className="umpire-section-heading">
          <div>
            <span className="umpire-eyebrow">DISCOVER CRICKET</span>
            <h2>Nearby Matches</h2>
            <p>Find cricket matches happening around you.</p>
          </div>

          <button
            type="button"
            className="umpire-secondary-button"
            onClick={loadNearbyMatches}
            disabled={loading}
          >
            {loading ? <FaSpinner className="umpire-spin" /> : <FaRedo />}
            Refresh
          </button>
        </div>
<div className="audience-status-tabs">
  {['Upcoming', 'Live', 'Finished', 'Cancelled'].map((status) => (
    <button
      key={status}
      type="button"
      className={`audience-status-tab ${
        selectedStatus === status ? 'active' : ''
      }`}
      onClick={() => setSelectedStatus(status)}
    >
      {status}
    </button>
  ))}
</div>
        <div className="umpire-match-panel">
          {loading ? (
            <div className="umpire-empty-state">
              <FaSpinner className="umpire-loading-icon umpire-spin" />
              <h3>Finding matches near you...</h3>
              <p>Checking for cricket matches within 30 km.</p>
            </div>
          ) : error ? (
            <div className="umpire-empty-state umpire-error-state">
              <FaMapMarkerAlt className="umpire-loading-icon" />
              <h3>
                {locationBlocked
                  ? 'Location access required'
                  : 'Unable to find nearby matches'}
              </h3>
              <p>{error}</p>
              <button
                type="button"
                className="umpire-secondary-button"
                onClick={loadNearbyMatches}
              >
                <FaRedo />
                Try Again
              </button>
            </div>
          ) : filteredMatches.length === 0 ? (
            <div className="umpire-empty-state">
              <div className="umpire-empty-icon">
                <FaMapMarkerAlt />
              </div>
              <h3>No {selectedStatus.toLowerCase()} matches found</h3>
<p>
  There are currently no {selectedStatus.toLowerCase()} matches
  available within 30 km of your location.
</p>
            </div>
          ) : (
            <>
             <p className="audience-match-count">
  Found {filteredMatches.length} {selectedStatus.toLowerCase()} matches
</p>

<div className="audience-match-scroll">
  {filteredMatches.map(renderMatchCard)}
</div>
            </>
          )}
        </div>
      </section>
    </main>
  );
}

export default Matches;
