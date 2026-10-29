
import '../styles/match-cancellation-modal.css';
import '../styles/match-start-modal.css';
import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react';
import { createPortal } from 'react-dom';
import { useLocation, useNavigate } from 'react-router-dom';
import Toast from '../components/Toast';
import {
  cancelMatch,
} from '../services/matchService';
import CancelMatchModal from '../components/CancelMatchModal';
import {
  FaArrowLeft,
  FaPlus,
  FaCalendarAlt,
  FaClipboardList,
  FaCheckCircle,
  FaTimesCircle,
  FaMapMarkerAlt,
  FaClock,
  FaRedo,
  FaTrophy,
  FaSpinner,
  FaEdit,
  FaBan,
  FaPlay,
} from 'react-icons/fa';



import { useAuth } from '../context/AuthContext';

import apiClient from '../services/apiClient';

import '../styles/umpire-dashboard.css';

const matchTabs = [

  { label: 'Upcoming', status: 'Upcoming', Icon: FaCalendarAlt },

  { label: 'Live', status: 'Live', Icon: FaClock },

  { label: 'Finished', status: 'Finished', Icon: FaTrophy },

  { label: 'Cancelled', status: 'Cancelled', Icon: FaTimesCircle },

];


function MatchCardTitle({ name }) {
  const titleRef = useRef(null);
  const [overflow, setOverflow] = useState(0);

  useEffect(() => {
    const element = titleRef.current;
    if (!element) return;

    const measure = () => {
      setOverflow(
        Math.max(0, element.scrollWidth - element.clientWidth)
      );
    };

    measure();

    const observer = new ResizeObserver(measure);
    observer.observe(element);

    return () => observer.disconnect();
  }, [name]);

  return (
    <div
      ref={titleRef}
      className={`my-match-title ${overflow > 0 ? 'my-match-marquee' : ''}`}
      style={{ '--title-overflow': `${overflow}px` }}
      title={name}
    >
      <span>{name}</span>
    </div>
  );
}



const umpireStatistics = [

  {

    label: 'Created Matches',

    className: 'stat-created',

    Icon: FaClipboardList,

  },

  {

    label: 'Organized Successfully',

    className: 'stat-success',

    Icon: FaCheckCircle,

  },

  {

    label: 'Cancelled Matches',

    className: 'stat-cancelled',

    Icon: FaTimesCircle,

  },

];



function getValue(object, ...keys) {

  for (const key of keys) {

    if (object?.[key] !== undefined && object[key] !== null) {

      return object[key];

    }

  }
  return null;
}

function formatDate(value) {
  if (!value) return 'Date not available';

  const hasTimezone = /(?:Z|[+-]\d{2}:\d{2})$/i.test(value);
  const date = new Date(
    hasTimezone ? value : `${value}Z`
  );

  if (Number.isNaN(date.getTime())) {
    return 'Date not available';
  }

  return date.toLocaleDateString('en-IN', {
    timeZone: 'Asia/Kolkata',
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  });
}

function formatTime(value) {
  if (!value) return '';

  const hasTimezone = /(?:Z|[+-]\d{2}:\d{2})$/i.test(value);
  const date = new Date(
    hasTimezone ? value : `${value}Z`
  );

  if (Number.isNaN(date.getTime())) {
    return '';
  }

  return date.toLocaleTimeString('en-IN', {
    timeZone: 'Asia/Kolkata',
    hour: 'numeric',
    minute: '2-digit',
    hour12: true,
  });
}

function UmpireDashboard() {

  const [cancelModalMatch, setCancelModalMatch] = useState(null);
const [startModalMatch, setStartModalMatch] = useState(null);
const [startSubmitting, setStartSubmitting] = useState(false);
const [startError, setStartError] = useState('');
const [cancelError, setCancelError] = useState('');
const [cancelSubmitting, setCancelSubmitting] = useState(false);
const [cancelNotice, setCancelNotice] = useState('');
  const navigate = useNavigate();
  const location = useLocation();
  
const [toastMessage, setToastMessage] = useState('');

const handleToastClose = useCallback(() => {
    setToastMessage('');
  }, []);

useEffect(() => {
  const message = location.state?.toastMessage;

  if (!message) return;

  setToastMessage(message);

  navigate(location.pathname, {
    replace: true,
    state: null,
  });
}, [location.state, location.pathname, navigate]);

  const { user, setRole } = useAuth();



  const [activeTab, setActiveTab] = useState('Upcoming');

  const [matches, setMatches] = useState([]);

  const [matchesLoading, setMatchesLoading] = useState(true);

  const [matchesError, setMatchesError] = useState('');



  // Supports common user ID property names while AuthContext is in use.

const organizerUserId = getValue(
  user,
  'userId',
  'UserId',
  'id',
  'userID'
);


  const displayName = user

    ? `${user.firstName || ''} ${user.lastName || ''}`.trim()

    : '';



  const loadMatches = useCallback(async () => {

    if (!organizerUserId) {

      setMatches([]);

      setMatchesError(

        'Your user ID is not available. Please log in again and reopen the dashboard.'

      );

      setMatchesLoading(false);

      return;

    }



    setMatchesLoading(true);

    setMatchesError('');



    try {

      // No status parameter: retrieve all match statuses.

     const response = await apiClient.get('/Match/mine', {
      params: {
        createdByUserId: organizerUserId,
      },
    });



      const data = response.data;

      const result = Array.isArray(data)

        ? data

        : Array.isArray(data?.matches)

          ? data.matches

          : Array.isArray(data?.data)

            ? data.data

            : null;



      if (!result) {

        throw new Error('The server returned an unexpected match response.');

      }



      setMatches(result);

    } catch (error) {

      setMatchesError(

        error.response?.data?.message ||

          error.message ||

          'Unable to load your matches. Please try again.'

      );

    } finally {

      setMatchesLoading(false);

    }

  }, [organizerUserId]);



  useEffect(() => {

    loadMatches();

  }, [loadMatches]);



  const visibleMatches = useMemo(

    () =>

      matches.filter((match) => {

        const status = getValue(match, 'status', 'Status');

        return String(status || '').toLowerCase() === activeTab.toLowerCase();

      }),

    [matches, activeTab]

  );



  const counts = useMemo(() => {

    const result = {};



    for (const tab of matchTabs) {

      result[tab.status] = matches.filter(

        (match) =>

          String(getValue(match, 'status', 'Status') || '').toLowerCase() ===

          tab.status.toLowerCase()

      ).length;

    }



    return result;

  }, [matches]);



  const handleBack = () => {

    setRole('Player');

    navigate('/');

  };



  const handleCreateMatch = () => {
    navigate('/umpire/create-match');
  };



  
  const handleMatchClick = (match) => {
  const matchId = getValue(match, 'id', 'matchId', 'Id', 'MatchId');
  const status = getValue(match, 'status', 'Status');

  if (!matchId) return;

  if (status?.toLowerCase() === 'live') {
    navigate(`/umpire/matches/${matchId}/toss`);
  } else {
    navigate(`/umpire/matches/${matchId}`);
  }
};

  const handleUpdateMatch = (match) => {
    const matchId = getValue(match, 'id', 'matchId', 'Id', 'MatchId');

    if (matchId) {
      navigate(`/umpire/matches/${matchId}/edit`);
    }
  };


  const handleStartMatch = (match) => {
    setStartModalMatch(match);
    setStartError('');
  };

  const getCurrentCoordinates = () =>
    new Promise((resolve, reject) => {
      if (!navigator.geolocation) {
        reject(new Error('Location is not supported by this browser.'));
        return;
      }

      navigator.geolocation.getCurrentPosition(
        ({ coords }) => resolve({
          latitude: coords.latitude,
          longitude: coords.longitude,
        }),
        (error) => {
          const message = error.code === error.PERMISSION_DENIED
            ? 'Allow location access to start the match.'
            : error.code === error.TIMEOUT
              ? 'Location request timed out. Please try again.'
              : 'Unable to get your location. Please try again.';
          reject(new Error(message));
        },
        { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 }
      );
    });

  const handleConfirmStartMatch = async () => {
  if (!startModalMatch || startSubmitting) return;

  setStartSubmitting(true);
  setStartError('');

  const matchId = getValue(
    startModalMatch,
    'id',
    'matchId',
    'Id',
    'MatchId'
  );

  const umpireUserId = getValue(
    user,
    'userId',
    'UserId',
    'id',
    'userID'
  );

  if (!matchId || !umpireUserId) {
    setStartError('Unable to identify the match or umpire.');
    setStartSubmitting(false);
    return;
  }

  if (!navigator.geolocation) {
    setStartError('Your browser does not support location access.');
    setStartSubmitting(false);
    return;
  }

  try {
    const position = await new Promise((resolve, reject) => {
      navigator.geolocation.getCurrentPosition(
        resolve,
        reject,
        {
          enableHighAccuracy: true,
          timeout: 15000,
          maximumAge: 0,
        }
      );
    });

    const payload = {
      matchId,
      createdByUserId: umpireUserId,
      latitude: position.coords.latitude,
      longitude: position.coords.longitude,
    };

    await apiClient.post('/Scoring/start', payload);

    setStartModalMatch(null);

    // Replace this with your existing Toss page route if different.
    navigate(`/umpire/matches/${matchId}/toss`);
  } catch (error) {
    let message =
      error.response?.data?.message ||
      error.response?.data?.title;

    if (error.code === 1) {
      message = 'Location permission was denied. Allow location access and try again.';
    } else if (error.code === 2) {
      message = 'Your current location is unavailable. Please try again.';
    } else if (error.code === 3) {
      message = 'Location request timed out. Please try again.';
    }

    setStartError(
      message ||
      'Unable to start the match. Please try again.'
    );
  } finally {
    setStartSubmitting(false);
  }
};

const handleCancelMatch = (match) => {
  const matchId = getValue(match, 'id', 'matchId', 'Id', 'MatchId');

  if (!matchId) {
    setCancelNotice('Unable to identify this match.');
    return;
  }

  setCancelModalMatch(match);
  setCancelError('');
  setCancelNotice('');
};



const handleConfirmCancellation = async (cancellationData) => {
  const matchId = getValue(
    cancelModalMatch,
    'id',
    'matchId',
    'Id',
    'MatchId'
  );

  if (!matchId || !organizerUserId) {
    setCancelError('Unable to identify the match or organizer.');
    return;
  }

  setCancelSubmitting(true);
  setCancelError('');

  try {
    await cancelMatch(matchId, organizerUserId, cancellationData);

    const matchName =
      getValue(cancelModalMatch, 'name', 'Name') || 'Match';

    setCancelModalMatch(null);
    setToastMessage(`${matchName} cancelled successfully.`);

    await loadMatches();
  } catch (error) {
    setCancelError(
      error.response?.data?.message ||
      error.response?.data?.title ||
      'Unable to cancel the match. Please try again.'
    );
  } finally {
    setCancelSubmitting(false);
  }
};


  return (

    <main className="umpire-dashboard">
     <Toast
        message={toastMessage}
        type="success"
        onClose={handleToastClose}
      />

      <header className="umpire-topbar">

        <button

          type="button"

          className="umpire-back-button"

          onClick={handleBack}

        >

          <FaArrowLeft />

          <span>Back to CricPulse</span>

        </button>



        <span className="umpire-topbar-label">UMPIRE DASHBOARD</span>



        <button

          type="button"

          className="umpire-top-create-button"

          onClick={handleCreateMatch}

        >

          <FaPlus />

          <span>Create Match</span>

        </button>

      </header>



      <section className="umpire-hero">

        <div className="umpire-hero-content">

          <span className="umpire-eyebrow">THE GAME NEEDS YOU</span>

          <h1>Every great match needs a great umpire.</h1>

          <p>

            Keep the game fair, manage your matches, and make every cricket

            moment count.

          </p>

        </div>



        <div
          className="umpire-hero-art cricket-wicket-scene"
          role="img"
          aria-label="Cricket ball hits the wickets when hovered; out signal"
          tabIndex={0}
        >
          <div className="cartoon-out-label" aria-hidden="true">OUT!</div>
          <div className="wicket-ball" aria-hidden="true" />
          <div className="wicket-set" aria-hidden="true">
            <span className="wicket-bail wicket-bail-left" />
            <span className="wicket-bail wicket-bail-right" />
            <span className="wicket-stump wicket-stump-left" />
            <span className="wicket-stump wicket-stump-middle" />
            <span className="wicket-stump wicket-stump-right" />
          </div>
          <div className="cartoon-ground" />
          <span className="wicket-hint">Hover to bowl</span>
        </div>

      </section>



      <section className="umpire-profile-stats-section">

        <div className="umpire-profile-section">
          <div className="umpire-avatar">
            {user?.profileImageUrl ? (
              <img
                src={user.profileImageUrl}
                alt={`${displayName || 'Umpire'} profile`}
                onError={(event) => {
                  event.currentTarget.style.display = 'none';
                  event.currentTarget.nextElementSibling.style.display = 'grid';
                }}
              />
            ) : null}
            <span
              className="umpire-avatar-fallback"
              style={{ display: user?.profileImageUrl ? 'none' : 'grid' }}
            >
              {(user?.firstName || 'U').charAt(0).toUpperCase()}
            </span>
          </div>

          <div className="umpire-profile-info">
            <span className="umpire-profile-kicker">YOUR UMPIRE PROFILE</span>
            <h2>{displayName || 'Umpire'}</h2>
            <p className="umpire-profile-phone">{user?.mobileNumber || 'Phone not added'}</p>
            <p className="umpire-profile-email">{user?.email || 'Umpire account'}</p>
          </div>
        </div>



        <div className="umpire-stats-section">

          <div className="umpire-section-heading">

            <div>

              <span className="umpire-eyebrow">YOUR PERFORMANCE</span>

              <h2>Match Statistics</h2>

            </div>

          </div>



          <div className="umpire-stats-grid">

            {umpireStatistics.map(({ label, className, Icon }) => (

              <article

                className={`umpire-stat-card ${className}`}

                key={label}

              >

                <div className="umpire-stat-icon">

                  <Icon />

                </div>

                <div className="umpire-stat-value">
                  {matchesLoading ? (
                    <FaSpinner className="umpire-spin" aria-label="Loading statistics" />
                  ) : matchesError ? (
                    '—'
                  ) : label === 'Created Matches' ? (
                    matches.length
                  ) : label === 'Organized Successfully' ? (
                    counts.Finished ?? 0
                  ) : (
                    counts.Cancelled ?? 0
                  )}
                </div>

                <h3>{label}</h3>

              </article>

            ))}

          </div>
          <p className="umpire-data-note">
            Statistics are calculated from the matches currently available
            in your dashboard.
          </p>

        </div>

      </section>

      <section className="umpire-matches-section">

        <div className="umpire-section-heading">

          <div>

            <span className="umpire-eyebrow">YOUR MATCHES</span>

            <h2>My Matches</h2>

            <p>Manage and review the matches associated with you.</p>

          </div>



          <button

            type="button"

            className="umpire-secondary-button"

            onClick={loadMatches}

            disabled={matchesLoading}

          >

            {matchesLoading ? <FaSpinner className="umpire-spin" /> : <FaRedo />}

            Refresh

          </button>

        </div>



        <div className="umpire-match-tabs" role="tablist" aria-label="Match status">

          {matchTabs.map(({ label, status, Icon }) => (

            <button

              type="button"

              role="tab"

              aria-selected={activeTab === status}

              aria-controls="umpire-match-panel"

              className={

                activeTab === status

                  ? 'umpire-match-tab active'

                  : 'umpire-match-tab'

              }

              key={status}

              onClick={() => setActiveTab(status)}

            >

              <Icon />

              <span>{label}</span>

              <span className="umpire-tab-count">({counts[status] ?? 0})</span>

            </button>

          ))}

        </div>



        <div

          id="umpire-match-panel"

          role="tabpanel"

          className="umpire-match-panel"

        >

          {matchesLoading ? (

            <div className="umpire-empty-state" aria-live="polite">

              <FaSpinner className="umpire-loading-icon umpire-spin" />

              <h3>Loading your matches…</h3>

              <p>Fetching the latest match details from CricPulse.</p>

            </div>

          ) : matchesError ? (

            <div className="umpire-empty-state umpire-error-state" role="alert">

              <FaTimesCircle className="umpire-loading-icon" />

              <h3>Couldn't load matches</h3>

              <p>{matchesError}</p>

              <button

                type="button"

                className="umpire-secondary-button"

                onClick={loadMatches}

              >

                <FaRedo />

                Try again

              </button>

            </div>

          ) : visibleMatches.length === 0 ? (

            <div className="umpire-empty-state">

              <div className="umpire-empty-icon">

                <FaCalendarAlt />

              </div>

              <h3>No {activeTab.toLowerCase()} matches</h3>

              <p>

                {activeTab === 'Upcoming'

                  ? 'Matches you create will appear here.'

                  : `You don't have any ${activeTab.toLowerCase()} matches right now.`}

              </p>

              {activeTab === 'Upcoming' && (

                <button

                  type="button"

                  className="umpire-secondary-button"

                  onClick={handleCreateMatch}

                >

                  <FaPlus />

                  Create a match

                </button>

              )}

            </div>

          ) : (

            <div className="umpire-match-grid">              
                {visibleMatches.map((match, index) => {
                  const id = getValue(match, 'id', 'matchId', 'Id', 'MatchId');
                  const name = getValue(match, 'name', 'Name') || `${getValue(match, 'teamAName', 'TeamAName') || 'Team A'} vs ${getValue(match, 'teamBName', 'TeamBName') || 'Team B'}`;
                  const teamA = getValue(match, 'teamAName', 'TeamAName') || 'Team A';
                  const teamB = getValue(match, 'teamBName', 'TeamBName') || 'Team B';
                  const scheduledAt = getValue(match, 'scheduledAt', 'ScheduledAt');
                  const venue = getValue(match, 'venue', 'Venue');
                  const address = getValue(match, 'locationAddress', 'LocationAddress');
                  const state = getValue(match, 'state', 'State');
                  const overs = getValue(match, 'overs', 'Overs');
                  const playersPerTeam = getValue(match, 'playersPerTeam', 'PlayersPerTeam');
                  const status = getValue(match, 'status', 'Status') || activeTab;
                  const statusKey = String(status).toLowerCase();

                  const innings = getValue(match, 'innings', 'Innings') || [];
                  const activeInnings = Array.isArray(innings)
                    ? innings.find((item) =>
                        ['live', 'inprogress', 'in progress'].includes(
                          String(getValue(item, 'status', 'Status') || '').toLowerCase()
                        )
                      )
                    : null;

                  const runs = getValue(activeInnings, 'totalRuns', 'TotalRuns');
                  const wickets = getValue(activeInnings, 'wickets', 'Wickets');
                  const legalBalls = getValue(activeInnings, 'legalBalls', 'LegalBalls');
                  const battingTeam =
                    getValue(activeInnings, 'battingTeamName', 'BattingTeamName') ||
                    getValue(activeInnings, 'battingTeam', 'BattingTeam') ||
                    'Batting team';

                  const liveScore =
                    activeInnings && runs != null && wickets != null
                      ? `${battingTeam}: ${runs}/${wickets} (${Math.floor((legalBalls || 0) / 6)}.${(legalBalls || 0) % 6})`
                      : 'Live score unavailable';

                  const winner = getValue(match, 'winnerTeam', 'WinnerTeam');
                  const resultText = getValue(match, 'resultText', 'ResultText');
                  const cancellationReason =
                    getValue(match, 'cancellationReason', 'CancellationReason');

                  return (
                    <article
                      className={`umpire-match-card my-match-card status-card-${statusKey}`}
                      key={id || index}
                      role="link"
                      tabIndex={0}
                      onClick={() => handleMatchClick(match)}
                      onKeyDown={(event) => {
                        if (event.key === 'Enter' || event.key === ' ') {
                          event.preventDefault();
                          handleMatchClick(match);
                        }
                      }}
                    >
                      <div className="my-match-card-header">
                        <div className="my-match-date">
                          {statusKey === 'live' ? (
                            <span className="my-match-live-score">{liveScore}</span>
                          ) : (
                            <>
                              <span>{formatDate(scheduledAt)}</span>
                              <span>{formatTime(scheduledAt) || 'Time not available'}</span>
                            </>
                          )}
                        </div>

                        <span className={`my-match-status status-${statusKey}`}>
                          {statusKey === 'live' && (
                            <span className="live-pulse-dot" aria-hidden="true" />
                          )}
                          {status}
                        </span>
                      </div>

                     <MatchCardTitle name={name} />

                      <div className="my-match-teams" title={`${teamA} vs ${teamB}`}>
                        <span className={teamA.length > 16 ? 'team-marquee' : ''}>
                          {teamA}
                        </span>
                        <span className="my-match-vs">VS</span>
                        <span className={teamB.length > 16 ? 'team-marquee' : ''}>
                          {teamB}
                        </span>
                      </div>

                      <div className="my-match-location">
                        <p>
                          <strong>Venue:</strong> {venue || address || 'Not provided'}
                        </p>
                        <p>
                          <strong>State:</strong> {state || 'Not available'}
                        </p>
                      </div>

                      {statusKey === 'finished' && (winner || resultText) && (
                        <div className="my-match-result">
                          {winner && <strong>{winner} won</strong>}
                          {resultText && <span>{resultText}</span>}
                        </div>
                      )}

                      {statusKey === 'cancelled' && (
                        <div className="my-match-cancellation">
                          <strong>Cancellation reason</strong>
                          <span>
                            {cancellationReason ||
                              getValue(match, 'cancellationReasonType', 'CancellationReasonType') ||
                              'Reason not recorded'}
                          </span>
                        </div>
                      )}

                      <div className="my-match-footer">
                        <span className="my-match-meta-pill">
                          Overs: {overs ?? '—'}
                        </span>
                        <span className="my-match-meta-pill">
                          Per team: {playersPerTeam ?? '—'}
                        </span>
                      </div>

                     
       {(statusKey === 'upcoming' || statusKey === 'live') && (
  <div className="my-match-actions">
  {statusKey === 'upcoming' && (
    <>
      <button
        type="button"
        className="umpire-secondary-button my-match-action-start"
        onClick={(event) => {
          event.stopPropagation();
          handleStartMatch(match);
        }}
      >
        <FaPlay />
        Start
      </button>

      <button
        type="button"
        className="umpire-secondary-button my-match-action-update"
        onClick={(event) => {
          event.stopPropagation();
          handleUpdateMatch(match);
        }}
      >
        <FaEdit />
        Update
      </button>
    </>
  )}

  {(statusKey === 'upcoming' || statusKey === 'live') && (
    <button
      type="button"
      className="umpire-secondary-button my-match-action-cancel"
      disabled={cancelSubmitting}
      onClick={(event) => {
        event.stopPropagation();
        handleCancelMatch(match);
      }}
    >
      <FaBan />
      Cancel
    </button>
  )}
</div>
)}

                    </article>
                  );
                })}
            </div>
          )}

        </div>

      </section>

      

{cancelNotice && (
  <div className="cancel-notice" role="status">
    <span>{cancelNotice}</span>
    <button
      type="button"
      onClick={() => setCancelNotice('')}
      aria-label="Dismiss message"
    >
      ×
    </button>
  </div>
)}


<CancelMatchModal
  match={cancelModalMatch}
  isSubmitting={cancelSubmitting}
  error={cancelError}
  onClose={() => {
    if (cancelSubmitting) return;
    setCancelModalMatch(null);
    setCancelError('');
  }}
  onConfirm={handleConfirmCancellation}
/>



{startModalMatch &&
  createPortal(
    <div
      className="start-match-modal-backdrop"
      onMouseDown={(event) => {
        if (event.target === event.currentTarget && !startSubmitting) {
          setStartModalMatch(null);
          setStartError('');
        }
      }}
    >
      <section
        className="start-match-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="start-match-title"
      >
        <button
          type="button"
          className="start-match-close"
          aria-label="Close confirmation"
          disabled={startSubmitting}
          onClick={() => {
            setStartModalMatch(null);
            setStartError('');
          }}
        >
          &times;
        </button>

        <span className="start-match-eyebrow">
          CRICPULSE · MATCH DAY
        </span>

        <h2 id="start-match-title">Ready to play?</h2>

        <p className="start-match-description">
          You're about to start
        </p>

        <div className="start-match-details">
          <strong>
            {getValue(startModalMatch, 'name', 'Name') ||
              `${getValue(startModalMatch, 'teamAName', 'TeamAName') || 'Team A'} vs ${
                getValue(startModalMatch, 'teamBName', 'TeamBName') || 'Team B'
              }`}
          </strong>
          <span>
            <FaMapMarkerAlt />
            Your current location will be recorded
          </span>
        </div>

        {startError && (
          <p className="start-match-error" role="alert">
            {startError}
          </p>
        )}

        <div className="start-match-actions">
          <button
            type="button"
            className="start-match-cancel"
            disabled={startSubmitting}
            onClick={() => {
              setStartModalMatch(null);
              setStartError('');
            }}
          >
            Not yet
          </button>

          <button
            type="button"
            className="start-match-confirm"
            disabled={startSubmitting}
            onClick={handleConfirmStartMatch}
          >
            {startSubmitting ? (
              <>
                <FaSpinner className="umpire-spin" />
                Starting...
              </>
            ) : (
              <>
                Start Match <span aria-hidden="true">→</span>
              </>
            )}
          </button>
        </div>

        <p className="start-match-footnote">
          Make sure you're at the match venue before continuing.
        </p>
      </section>
    </div>,
    document.body
  )}

    </main>

  );

}



export default UmpireDashboard;
