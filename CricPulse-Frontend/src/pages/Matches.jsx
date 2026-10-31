import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  FaMapMarkerAlt,
  FaRedo,
  FaSpinner,
} from 'react-icons/fa';

import {
  getMatches,
  getIndianStates,
} from '../services/matchService';
import '../styles/umpire-dashboard.css';

const MATCH_STATUSES = ['Upcoming', 'Live', 'Finished', 'Cancelled'];

function getValue(object, ...keys) {
  for (const key of keys) {
    if (object?.[key] !== undefined && object[key] !== null) {
      return object[key];
    }
  }

  return null;
}

function unwrapList(response, preferredKeys = []) {
  if (Array.isArray(response)) return response;

  for (const key of preferredKeys) {
    if (Array.isArray(response?.[key])) return response[key];
  }

  if (Array.isArray(response?.data)) return response.data;

  for (const key of preferredKeys) {
    if (Array.isArray(response?.data?.[key])) return response.data[key];
  }

  return [];
}

function normalizeStates(response) {
  return unwrapList(response, ['states', 'States', 'items', 'Items'])
    .map((item) => {
      if (typeof item === 'string') return item;

      return getValue(
        item,
        'stateName',
        'StateName',
        'name',
        'Name',
        'state',
        'State'
      );
    })
    .filter(Boolean)
    .sort((a, b) => String(a).localeCompare(String(b)));
}

function getPlayersPerTeam(match) {
  const values = [match?.playersPerTeam, match?.PlayersPerTeam];
  const populatedValue = values.find(
    (value) =>
      value !== undefined &&
      value !== null &&
      value !== '' &&
      Number(value) > 0
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

function getLiveMatchUpdate(match, innings, teamA, teamB, totalOvers) {
  const tossAgreed = getValue(match, 'tossAgreed', 'TossAgreed');
  const tossWinner = getValue(match, 'tossWinnerTeam', 'TossWinnerTeam');
  const tossDecision = String(
    getValue(match, 'tossDecision', 'TossDecision') || ''
  ).toLowerCase();

  const firstInnings = Array.isArray(innings)
    ? innings.find(
        (item) =>
          Number(getValue(item, 'inningsNumber', 'InningsNumber')) === 1
      ) || innings[0]
    : null;

  const secondInnings = Array.isArray(innings)
    ? innings.find(
        (item) =>
          Number(getValue(item, 'inningsNumber', 'InningsNumber')) === 2
      )
    : null;

  const firstInningsStatus = String(
    getValue(firstInnings, 'status', 'Status') || ''
  ).toLowerCase();

  const firstInningsFinished =
    ['completed', 'complete', 'finished'].includes(firstInningsStatus) ||
    Boolean(secondInnings);

  if (!tossAgreed) {
    return 'Match has started';
  }

  if (!firstInningsFinished) {
    if (!tossWinner || !tossDecision) {
      return 'Match has started';
    }

    const winner = String(tossWinner).toLowerCase();
    const winnerName =
      ['teama', 'team a', 'a'].includes(winner) ? teamA :
      ['teamb', 'team b', 'b'].includes(winner) ? teamB :
      tossWinner;

    const decision = tossDecision.includes('bat')
      ? 'bat first'
      : 'bowl first';

    return `${winnerName} won the toss and chose to ${decision}`;
  }

  const firstRuns = Number(
    getValue(firstInnings, 'totalRuns', 'TotalRuns') ?? 0
  );

  const target = firstRuns + 1;

  const secondRuns = Number(
    getValue(secondInnings, 'totalRuns', 'TotalRuns') ?? 0
  );

  const runsNeeded = Math.max(target - secondRuns, 0);

  const legalBalls = Number(
    getValue(secondInnings, 'legalBalls', 'LegalBalls') ?? 0
  );

  const configuredOvers = Number(totalOvers) || 0;
  const remainingBalls = Math.max(
    configuredOvers * 6 - legalBalls,
    0
  );

  const remainingOvers =
    `${Math.floor(remainingBalls / 6)}.${remainingBalls % 6}`;

  const chasingTeam =
    getValue(secondInnings, 'battingTeam', 'BattingTeam') ||
    getValue(firstInnings, 'bowlingTeam', 'BowlingTeam') ||
    (String(getValue(firstInnings, 'battingTeam', 'BattingTeam') || '')
      .toLowerCase()
      .includes('a')
      ? teamB
      : teamA);

  const chasingName =
    String(chasingTeam).toLowerCase() === 'teama' ? teamA :
    String(chasingTeam).toLowerCase() === 'teamb' ? teamB :
    chasingTeam;

  return `${chasingName} need ${runsNeeded} runs in ${remainingOvers} overs to win`;
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
  const [nearbyLoading, setNearbyLoading] = useState(true);
  const [nearbyError, setNearbyError] = useState('');
  const [locationBlocked, setLocationBlocked] = useState(false);

  const [states, setStates] = useState([]);
  const [statesLoading, setStatesLoading] = useState(true);
  const [statesError, setStatesError] = useState('');

  const [selectedStatus, setSelectedStatus] = useState('Upcoming');
  const [selectedState, setSelectedState] = useState('');
  const [stateMatches, setStateMatches] = useState([]);
  const [stateMatchesLoading, setStateMatchesLoading] = useState(false);
  const [stateMatchesError, setStateMatchesError] = useState('');

  const filteredNearbyMatches = useMemo(() => {
  return nearbyMatches
    .filter((match) => {
      const matchStatus = getValue(match, 'status', 'Status');
      return String(matchStatus).toLowerCase() === selectedStatus.toLowerCase();
    })
    .sort((a, b) => {
      const dateA = new Date(
        getValue(a, 'scheduledAt', 'ScheduledAt') || 0
      ).getTime();

      const dateB = new Date(
        getValue(b, 'scheduledAt', 'ScheduledAt') || 0
      ).getTime();

      return dateB - dateA;
    });
}, [nearbyMatches, selectedStatus]);

  const loadNearbyMatches = useCallback(() => {
    if (!navigator.geolocation) {
      setNearbyError('Your browser does not support location access.');
      setNearbyLoading(false);
      return;
    }

    setNearbyLoading(true);
    setNearbyError('');
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

          setNearbyMatches(
            unwrapList(response, ['matches', 'Matches', 'items', 'Items'])
          );
        } catch (err) {
          setNearbyError(
            err.response?.data?.message || 'Unable to load nearby matches.'
          );
        } finally {
          setNearbyLoading(false);
        }
      },
      (error) => {
        setLocationBlocked(true);
        setNearbyLoading(false);

        if (error.code === error.PERMISSION_DENIED) {
          setNearbyError(
            'Location permission is disabled. Allow location access to see nearby matches.'
          );
        } else {
          setNearbyError('Unable to get your location. Please try again.');
        }
      },
      {
        enableHighAccuracy: true,
        timeout: 10000,
        maximumAge: 300000,
      }
    );
  }, []);

  const sortedStateMatches = useMemo(() => {
  return [...stateMatches].sort((a, b) => {
    const dateA = new Date(
      getValue(a, 'scheduledAt', 'ScheduledAt') || 0
    ).getTime();

    const dateB = new Date(
      getValue(b, 'scheduledAt', 'ScheduledAt') || 0
    ).getTime();

    return dateB - dateA;
  });
}, [stateMatches]);

  const loadStates = useCallback(async () => {
    setStatesLoading(true);
    setStatesError('');

    try {
      const response = await getIndianStates();
      setStates(normalizeStates(response));
    } catch (err) {
      setStatesError(
        err.response?.data?.message || 'Unable to load states.'
      );
    } finally {
      setStatesLoading(false);
    }
  }, []);

  // Load nearby matches and Indian states when the page mounts.
  useEffect(() => {
    loadNearbyMatches();
    loadStates();
  }, [loadNearbyMatches, loadStates]);

  // Load state-wise matches whenever status or state changes.
  const loadStateMatches = useCallback(async () => {
    if (!selectedState) {
      setStateMatches([]);
      setStateMatchesError('');
      setStateMatchesLoading(false);
      return;
    }

    setStateMatchesLoading(true);
    setStateMatchesError('');

    try {
      const response = await getMatches({
        status: selectedStatus,
        state: selectedState,
      });

      setStateMatches(
        unwrapList(response, ['matches', 'Matches', 'items', 'Items'])
      );
    } catch (err) {
      console.error('State matches API error:', err);

      setStateMatchesError(
        err.response?.data?.message ||
        'Unable to load matches for this state.'
      );

      setStateMatches([]);
    } finally {
      setStateMatchesLoading(false);
    }
  }, [selectedState, selectedStatus]);

  useEffect(() => {
    loadStateMatches();
  }, [loadStateMatches]);

  // Navigate to the existing public Match Review page.
  const handleMatchClick = (match) => {
    const matchId = getValue(
      match,
      'matchId',
      'MatchId',
      'id',
      'Id'
    );

    if (!matchId) return;

    navigate(`/umpire/matches/${matchId}`, {
      state: {
        viewMode: 'audience',
        returnTo: '/matches',
      },
    });
  };

    const renderMatchCard = (match, index, sectionType) => {
    const id = getValue(match, 'matchId', 'MatchId', 'id', 'Id');

    const name =
      getValue(match, 'name', 'Name') ||
      `${getValue(match, 'teamAName', 'TeamAName') || 'Team A'} vs ${
        getValue(match, 'teamBName', 'TeamBName') || 'Team B'
      }`;

    const teamA = getValue(match, 'teamAName', 'TeamAName') || 'Team A';
    const teamB = getValue(match, 'teamBName', 'TeamBName') || 'Team B';
    const scheduledAt = getValue(match, 'scheduledAt', 'ScheduledAt');
    const status = getValue(match, 'status', 'Status') || selectedStatus;
    const statusKey = String(status).toLowerCase();

    const overs = getValue(match, 'overs', 'Overs');
    const playersPerTeam = getPlayersPerTeam(match);
    const distance = getValue(match, 'distanceKm', 'DistanceKm');
    const venue = getValue(match, 'venue', 'Venue');
    const state = getValue(match, 'state', 'State');
    const innings = getValue(match, 'innings', 'Innings') || [];
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
    const teamAInnings = getTeamScore(innings, 'TeamA');
    const teamBInnings = getTeamScore(innings, 'TeamB');

    const resultText = getValue(match, 'resultText', 'ResultText');
    const cancellationReason =
      getValue(match, 'cancellationReason', 'CancellationReason') ||
      getValue(match, 'cancellationReasonType', 'CancellationReasonType');

    const showDistance =
      sectionType === 'nearby' &&
      ['upcoming', 'live'].includes(statusKey) &&
      distance !== null &&
      distance !== undefined;

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


    const liveMatchUpdate =
  statusKey === 'live'
    ? getLiveMatchUpdate(match, innings, teamA, teamB, overs)
    : '';

    return (
      <article
        className={`umpire-match-card my-match-card audience-match-card status-card-${statusKey}`}
        key={`${sectionType}-${id || index}`}
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
        <div className="audience-card-header">
          <div className="my-match-date">
            <span>{formatDate(scheduledAt)}</span>
            <span>{formatTime(scheduledAt) || 'Time not available'}</span>
          </div>

          {showDistance ? (
            <span className="my-match-distance-pill">
              <FaMapMarkerAlt />
              {Number(distance).toFixed(2)} km away
            </span>
          ) : (
            <span className="audience-card-header-spacer" />
          )}

          <span className={`my-match-status status-${statusKey}`}>
            {statusKey === 'live' && (
              <span className="live-pulse-dot" aria-hidden="true" />
            )}
            {status}
          </span>
        </div>

        <MatchCardTitle name={name} />

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

        <div className="audience-match-result audience-result-location-box">
  {statusKey === 'live' && (
    <div className="audience-result-text audience-live-update">
      <strong>LIVE MATCH</strong>
      <span>{liveMatchUpdate}</span>
    </div>
  )}

  {statusKey === 'finished' && (
    <div className="audience-result-text">
      {resultText || 'Match completed'}
    </div>
  )}

  {statusKey === 'cancelled' && (
    <div className="audience-result-text audience-cancellation-update">
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

        <div className="audience-match-footer">
          <span className="my-match-meta-pill">Overs: {overs ?? '—'}</span>
          <span className="my-match-meta-pill">
            Per team: {playersPerTeam ?? '—'}
          </span>
        </div>
      </article>
    );
  };
    const renderNearbySection = () => (
    <section className="audience-match-subsection">
      <div className="audience-subsection-heading">
        <div>
          <h3>Nearby Matches</h3>
          <p>Matches within 30 km of your location</p>
        </div>

        <button
          type="button"
          className="umpire-secondary-button audience-refresh-button"
          onClick={loadNearbyMatches}
          disabled={nearbyLoading}
        >
          {nearbyLoading ? (
            <FaSpinner className="umpire-spin" />
          ) : (
            <FaRedo />
          )}
          Refresh
        </button>
      </div>

      {nearbyLoading ? (
        <div className="audience-section-message">
          <FaSpinner className="umpire-spin" />
          <span>Finding nearby matches...</span>
        </div>
      ) : nearbyError ? (
        <div className="audience-section-message audience-section-error">
          <FaMapMarkerAlt />
          <div>
            <strong>
              {locationBlocked
                ? 'Location access required'
                : 'Unable to load nearby matches'}
            </strong>
            <p>{nearbyError}</p>
            <button
              type="button"
              className="umpire-secondary-button"
              onClick={loadNearbyMatches}
            >
              <FaRedo /> Try Again
            </button>
          </div>
        </div>
      ) : filteredNearbyMatches.length === 0 ? (
        <div className="audience-section-message">
          <FaMapMarkerAlt />
          <span>No {selectedStatus.toLowerCase()} matches found nearby.</span>
        </div>
      ) : (
        <>
          <p className="audience-match-count">
            {filteredNearbyMatches.length} nearby{' '}
            {selectedStatus.toLowerCase()} match
            {filteredNearbyMatches.length === 1 ? '' : 'es'}
          </p>
          <div className="audience-match-scroll">
            {filteredNearbyMatches.map((match, index) =>
              renderMatchCard(match, index, 'nearby')
            )}
          </div>
        </>
      )}
    </section>
  );

  const renderStateSection = () => (
    <section className="audience-match-subsection audience-state-subsection">
      <div className="audience-subsection-heading audience-state-heading">
        <div>
          <h3>State-wise Matches</h3>
          <p>Explore matches happening across a selected state</p>
        </div>

        <label className="audience-state-select-wrap">
          <span className="sr-only">Select state</span>
          <select
            className="audience-state-select"
            value={selectedState}
            onChange={(event) => setSelectedState(event.target.value)}
            disabled={statesLoading || states.length === 0}
          >
            <option value="">
              {statesLoading ? 'Loading states...' : 'Select State'}
            </option>
            {states.map((state) => (
              <option key={state} value={state}>
                {state}
              </option>
            ))}
          </select>
        </label>
      </div>

      {statesError ? (
        <div className="audience-section-message audience-section-error">
          <span>{statesError}</span>
          <button
            type="button"
            className="umpire-secondary-button"
            onClick={loadStates}
          >
            <FaRedo /> Retry
          </button>
        </div>
      ) : !selectedState ? (
        <div className="audience-section-message">
          <FaMapMarkerAlt />
          <span>
            Select a state to browse its {selectedStatus.toLowerCase()} matches.
          </span>
        </div>
      ) : stateMatchesLoading ? (
        <div className="audience-section-message">
          <FaSpinner className="umpire-spin" />
          <span>Loading matches in {selectedState}...</span>
        </div>
      ) : stateMatchesError ? (
        <div className="audience-section-message audience-section-error">
          {stateMatchesError}
        </div>
      ) : stateMatches.length === 0 ? (
        <div className="audience-section-message">
          <FaMapMarkerAlt />
          <span>
            No {selectedStatus.toLowerCase()} matches found in {selectedState}.
          </span>
        </div>
      ) : (
        <>
          <p className="audience-match-count">
            {stateMatches.length} {selectedStatus.toLowerCase()} match
            {stateMatches.length === 1 ? '' : 'es'} in {selectedState}
          </p>
          <div className="audience-match-scroll">
            {sortedStateMatches.map((match, index) =>
            renderMatchCard(match, index, 'state')

            )}
          </div>
        </>
      )}
    </section>
  );

  return (
    <main className="umpire-dashboard audience-matches-page">
      <section className="umpire-matches-section">
        <div className="umpire-section-heading audience-page-heading">
          <div>
            <span className="umpire-eyebrow">DISCOVER CRICKET</span>
            <h2>Matches</h2>
            <p>Find nearby games or explore matches state-wise.</p>
          </div>
        </div>

        <div
          className="audience-status-tabs"
          role="tablist"
          aria-label="Match status"
        >
          {MATCH_STATUSES.map((status) => {
            const isLive = status === 'Live';

            return (
              <button
                key={status}
                type="button"
                role="tab"
                aria-selected={selectedStatus === status}
                className={`audience-status-tab ${
                  selectedStatus === status ? 'active' : ''
                } ${isLive ? 'audience-live-tab' : ''}`}
                onClick={() => setSelectedStatus(status)}
              >
                {isLive && (
                  <span
                    className="live-pulse-dot audience-tab-live-dot"
                    aria-hidden="true"
                  />
                )}
                {status}
              </button>
            );
          })}
        </div>

        <div className="umpire-match-panel audience-status-content">
          {renderNearbySection()}
          <div className="audience-section-divider" />
          {renderStateSection()}
        </div>
      </section>
    </main>
  );
}

export default Matches;