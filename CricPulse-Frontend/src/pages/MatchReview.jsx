import { useCallback, useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { FaArrowLeft, FaInfoCircle, FaHistory, FaTrophy, FaBullseye, FaAward } from 'react-icons/fa';

import { getMatchById } from '../services/matchService';
import { getMatchState } from '../services/scoringService';
import { useAuth } from '../context/AuthContext';

import '../styles/match-review.css';

const getValue = (object, ...keys) => {
  for (const key of keys) {
    if (object?.[key] !== undefined && object[key] !== null) {
      return object[key];
    }
  }

  return null;
};

const getText = (value, fallback = '—') => {
  if (value === null || value === undefined || value === '') {
    return fallback;
  }

  return String(value);
};

const getStatus = (value) =>
  String(value ?? '').trim().toLowerCase();

const isCompletedStatus = (value) =>
  ['completed', 'complete', 'finished'].includes(getStatus(value));

const isLiveStatus = (value) =>
  ['live', 'inprogress', 'in progress', 'ongoing'].includes(
    getStatus(value).replace(/[-_]/g, ' ')
  );

const getPlayerName = (player) => {
  const fullName = getValue(
    player,
    'playerName',
    'PlayerName',
    'displayName',
    'DisplayName',
    'fullName',
    'FullName'
  );

  if (fullName) return fullName;

  const firstName = getValue(player, 'firstName', 'FirstName') || '';
  const lastName = getValue(player, 'lastName', 'LastName') || '';

  return `${firstName} ${lastName}`.trim() || 'Player';
};

const getTeamCode = (team) => {
  const value = String(team ?? '').trim().toLowerCase();

  if (['teama', 'team a', 'a'].includes(value)) return 'TeamA';
  if (['teamb', 'team b', 'b'].includes(value)) return 'TeamB';

  return team ?? '';
};

const getInningsNumber = (innings) =>
  Number(getValue(innings, 'inningsNumber', 'InningsNumber') ?? 0);

const getInningsStatus = (innings) =>
  getValue(innings, 'status', 'Status');

const getRecords = (innings, ...keys) =>
  getValue(innings, ...keys) || [];

const formatOvers = (legalBalls, fallbackOvers) => {
  if (legalBalls === null || legalBalls === undefined) {
    return getText(fallbackOvers, '0');
  }

  const balls = Number(legalBalls) || 0;

  return `${Math.floor(balls / 6)}.${balls % 6}`;
};

export default function MatchReview() {
  const { matchId } = useParams();
  const navigate = useNavigate();
  const { user } = useAuth();

  const organizerUserId = getValue(
    user,
    'userId',
    'UserId',
    'id',
    'userID'
  );

  const [match, setMatch] = useState(null);
  const [matchState, setMatchState] = useState(null);
  const [activeTeam, setActiveTeam] = useState('TeamA');
  const [activeTab, setActiveTab] = useState('match-info');

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [stateError, setStateError] = useState('');

  const loadMatchReview = useCallback(async () => {
    if (!matchId) {
      setError('Match ID is missing from the URL.');
      setLoading(false);
      return;
    }

    setLoading(true);
    setError('');
    setStateError('');

    try {
      const matchResult = await getMatchById(
        matchId,
        organizerUserId
      );

      setMatch(matchResult);

      try {
        const stateResult = await getMatchState(matchId);
        setMatchState(stateResult);
      } catch (stateRequestError) {
        setMatchState(null);
        setStateError(
          stateRequestError.response?.data?.message ||
          stateRequestError.response?.data?.title ||
          'Match scoring history could not be loaded.'
        );
      }
    } catch (requestError) {
      setError(
        requestError.response?.data?.message ||
        requestError.response?.data?.title ||
        'Unable to load match details.'
      );
    } finally {
      setLoading(false);
    }
  }, [matchId, organizerUserId]);

  useEffect(() => {
    loadMatchReview();
  }, [loadMatchReview]);

  // All hooks stay above the conditional returns.
  const teamAName =
    getValue(match, 'teamAName', 'TeamAName', 'team1Name', 'Team1Name') ||
    'Team A';

  const teamBName =
    getValue(match, 'teamBName', 'TeamBName', 'team2Name', 'Team2Name') ||
    'Team B';

  const matchStatus = getValue(
    matchState,
    'matchStatus',
    'MatchStatus'
  ) ?? getValue(match, 'status', 'Status', 'matchStatus', 'MatchStatus');

  const innings = useMemo(() => {
    const result = getValue(matchState, 'innings', 'Innings');

    return Array.isArray(result) ? result : [];
  }, [matchState]);

  const firstInnings = useMemo(
    () => innings.find((item) => getInningsNumber(item) === 1) ?? null,
    [innings]
  );

  const secondInnings = useMemo(
    () => innings.find((item) => getInningsNumber(item) === 2) ?? null,
    [innings]
  );

  const firstInningsStatus = getInningsStatus(firstInnings);
  const secondInningsStatus = getInningsStatus(secondInnings);

  const overallMatchFinished =
    isCompletedStatus(matchStatus) ||
    isCompletedStatus(getValue(match, 'status', 'Status'));

  const firstInningsCompleted =
    isCompletedStatus(firstInningsStatus) ||
    Boolean(secondInnings) ||
    overallMatchFinished;

  const scoringStarted =
    isLiveStatus(matchStatus) ||
    Boolean(firstInnings) ||
    Boolean(secondInnings) ||
    overallMatchFinished;

  const showTossDetails = scoringStarted;

  // Show the First Innings section after the first innings is complete.
  // Its history is available when the innings data has been returned.
  const showFirstInningsSection =
    scoringStarted && firstInningsCompleted;

  const players = useMemo(() => {
    const result = getValue(
      match,
      'matchPlayers',
      'MatchPlayers',
      'players',
      'Players'
    );

    return Array.isArray(result) ? result : [];
  }, [match]);

  const teamAPlayers = players.filter((player) => {
    const team = getTeamCode(
      getValue(player, 'team', 'Team', 'teamName', 'TeamName')
    );

    return team === 'TeamA' ||
      String(team).toLowerCase() === String(teamAName).toLowerCase();
  });

  const teamBPlayers = players.filter((player) => {
    const team = getTeamCode(
      getValue(player, 'team', 'Team', 'teamName', 'TeamName')
    );

    return team === 'TeamB' ||
      String(team).toLowerCase() === String(teamBName).toLowerCase();
  });

  const visiblePlayers =
    activeTeam === 'TeamA' ? teamAPlayers : teamBPlayers;

  const tossWinner = getValue(
    match,
    'tossWinnerTeam',
    'TossWinnerTeam'
  );

  const tossDecision = getValue(
    match,
    'tossDecision',
    'TossDecision'
  );

  const displayTeam = (team) => {
    const code = getTeamCode(team);

    if (code === 'TeamA') return teamAName;
    if (code === 'TeamB') return teamBName;

    return getText(team);
  };


  const firstBattingTeam = getValue(
    firstInnings,
    'battingTeam',
    'BattingTeam'
  );
  const isCancelled = getStatus(matchStatus) === 'cancelled' ||
    getStatus(getValue(match, 'status', 'Status')) === 'cancelled';
  const showReviewTabs = overallMatchFinished || scoringStarted || (isCancelled && Boolean(firstInnings));
  const secondBattingTeam = getValue(secondInnings, 'battingTeam', 'BattingTeam');
  const secondBowlingTeam = getValue(secondInnings, 'bowlingTeam', 'BowlingTeam');
  const secondInningsRuns = getValue(secondInnings, 'totalRuns', 'TotalRuns');
  const secondInningsWickets = getValue(secondInnings, 'wickets', 'Wickets');
  const secondInningsBalls = getValue(secondInnings, 'legalBalls', 'LegalBalls');
  const secondInningsOvers = getValue(secondInnings, 'totalOvers', 'TotalOvers');
  const secondBattingRecords = getRecords(secondInnings, 'battingRecords', 'BattingRecords');
  const secondBowlingRecords = getRecords(secondInnings, 'bowlingRecords', 'BowlingRecords');

  const firstTeamScore = firstInnings ? {
    team: displayTeam(firstBattingTeam),
    runs: Number(getValue(firstInnings, 'totalRuns', 'TotalRuns') ?? 0),
    wickets: Number(getValue(firstInnings, 'wickets', 'Wickets') ?? 0),
  } : null;
  const secondTeamScore = secondInnings ? {
    team: displayTeam(secondBattingTeam),
    runs: Number(secondInningsRuns ?? 0),
    wickets: Number(secondInningsWickets ?? 0),
  } : null;
  const teamAScore = [firstTeamScore, secondTeamScore].find((score) =>
    String(score?.team).toLowerCase() === String(teamAName).toLowerCase()
  );
  const teamBScore = [firstTeamScore, secondTeamScore].find((score) =>
    String(score?.team).toLowerCase() === String(teamBName).toLowerCase()
  );
  const allBattingRecords = [
    ...getRecords(firstInnings, 'battingRecords', 'BattingRecords'),
    ...secondBattingRecords,
  ];
  const allBowlingRecords = [
    ...getRecords(firstInnings, 'bowlingRecords', 'BowlingRecords'),
    ...secondBowlingRecords,
  ];
  const topRunScorer = allBattingRecords.reduce((best, record) => {
    const runs = Number(getValue(record, 'runs', 'Runs') ?? 0);
    return !best || runs > best.runs ? { record, runs } : best;
  }, null);
  const topBowler = allBowlingRecords.reduce((best, record) => {
    const wickets = Number(getValue(record, 'wickets', 'Wickets') ?? 0);
    const runs = Number(getValue(record, 'runsConceded', 'RunsConceded') ?? 0);
    return !best || wickets > best.wickets || (wickets === best.wickets && runs < best.runs)
      ? { record, wickets, runs } : best;
  }, null);
  const playerOfTheMatch = getValue(match, 'playerOfTheMatch', 'PlayerOfTheMatch', 'pom', 'POM') ??
    getValue(matchState, 'playerOfTheMatch', 'PlayerOfTheMatch', 'pom', 'POM');
  const resultText = getValue(match, 'resultText', 'ResultText') ??
    getValue(matchState, 'resultText', 'ResultText');


  const firstBowlingTeam = getValue(
    firstInnings,
    'bowlingTeam',
    'BowlingTeam'
  );

  const firstInningsRuns = getValue(
    firstInnings,
    'totalRuns',
    'TotalRuns'
  );

  const firstInningsWickets = getValue(
    firstInnings,
    'wickets',
    'Wickets'
  );

  const firstInningsBalls = getValue(
    firstInnings,
    'legalBalls',
    'LegalBalls'
  );

  const firstInningsOvers = getValue(
    firstInnings,
    'totalOvers',
    'TotalOvers'
  );

  const battingRecords = getRecords(
    firstInnings,
    'battingRecords',
    'BattingRecords'
  );

  const bowlingRecords = getRecords(
    firstInnings,
    'bowlingRecords',
    'BowlingRecords'
  );

  const hasFirstInningsHistory =
    Boolean(firstInnings) &&
    (battingRecords.length > 0 ||
      bowlingRecords.length > 0 ||
      firstInningsRuns !== null);

  const showUpdateButton =
    !scoringStarted && !overallMatchFinished;

  const backToDashboard = () => navigate('/umpire');

  const backToScoring = () => {
    navigate(`/umpire/matches/${matchId}/live-scoring`);
  };

  const updateMatch = () => {
    navigate(`/umpire/matches/${matchId}/edit`);
  };

  const renderBattingCard = () => (
    <div className="match-review-history-card">
      <h3>{displayTeam(firstBattingTeam)} — Batting</h3>

      {battingRecords.length === 0 ? (
        <p className="match-review-empty">
          No batting records are available yet.
        </p>
      ) : (
        <div className="match-review-table-wrap">
          <table className="match-review-table">
            <thead>
              <tr>
                <th>Player</th>
                <th>R</th>
                <th>B</th>
                <th>4s</th>
                <th>6s</th>
                <th>SR</th>
                <th>Dismissal</th>
              </tr>
            </thead>

            <tbody>
              {battingRecords.map((record, index) => (
                <tr
                  key={
                    getValue(record, 'matchPlayerId', 'MatchPlayerId') ??
                    `${getPlayerName(record)}-${index}`
                  }
                >
                  <td>
                    {getPlayerName(record)}
                  </td>
                  <td>{getText(getValue(record, 'runs', 'Runs'), '0')}</td>
                  <td>{getText(getValue(record, 'balls', 'Balls'), '0')}</td>
                  <td>{getText(getValue(record, 'fours', 'Fours'), '0')}</td>
                  <td>{getText(getValue(record, 'sixes', 'Sixes'), '0')}</td>
                  <td>
                    {getText(
                      getValue(record, 'strikeRate', 'StrikeRate'),
                      '0.00'
                    )}
                  </td>
                  <td>
                    {getText(
                      getValue(record, 'dismissal', 'Dismissal'),
                      'Not out'
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );

  const renderBowlingCard = () => (
    <div className="match-review-history-card">
      <h3>{displayTeam(firstBowlingTeam)} — Bowling</h3>

      {bowlingRecords.length === 0 ? (
        <p className="match-review-empty">
          No bowling records are available yet.
        </p>
      ) : (
        <div className="match-review-table-wrap">
          <table className="match-review-table">
            <thead>
              <tr>
                <th>Bowler</th>
                <th>Overs</th>
                <th>M</th>
                <th>Runs</th>
                <th>Wickets</th>
                <th>Economy</th>
              </tr>
            </thead>

            <tbody>
              {bowlingRecords.map((record, index) => (
                <tr
                  key={
                    getValue(record, 'matchPlayerId', 'MatchPlayerId') ??
                    `${getPlayerName(record)}-${index}`
                  }
                >
                  <td>{getPlayerName(record)}</td>
                  <td>
                    {getText(
                      getValue(record, 'overs', 'Overs'),
                      '0'
                    )}
                  </td>
                  <td>
                    {getText(
                      getValue(record, 'maidens', 'Maidens'),
                      '0'
                    )}
                  </td>
                  <td>
                    {getText(
                      getValue(
                        record,
                        'runsConceded',
                        'RunsConceded'
                      ),
                      '0'
                    )}
                  </td>
                  <td>
                    {getText(
                      getValue(record, 'wickets', 'Wickets'),
                      '0'
                    )}
                  </td>
                  <td>
                    {getText(
                      getValue(record, 'economy', 'Economy'),
                      '0.00'
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );

  if (loading) {
    return (
      <main className="match-review-page">
        <p role="status">Loading match details...</p>
      </main>
    );
  }

  if (error || !match) {
    return (
      <main className="match-review-page">
        <section className="match-review-error">
          <h1>Unable to load match review</h1>
          <p role="alert">
            {error || 'Match details are unavailable.'}
          </p>

          <button
            type="button"
            onClick={backToDashboard}
          >
            Back to Dashboard
          </button>

          <button
            type="button"
            onClick={loadMatchReview}
          >
            Try Again
          </button>
        </section>
      </main>
    );
  }

  return (
    <main className="match-review-page">
      <header className="match-review-topbar">
        <button
          type="button"
          className="match-review-back"
          onClick={scoringStarted && !overallMatchFinished && !isCancelled ? backToScoring : backToDashboard}
        >
          <FaArrowLeft />
          {scoringStarted && !overallMatchFinished && !isCancelled ? 'Back to Scoring' : 'Back to Dashboard'}
        </button>

        <div className="match-review-heading">
          <p className="match-review-eyebrow">CRICPULSE · UMPIRE</p>
          <h1>Match Review</h1>
        </div>
      </header>

      <section className="match-review-summary">
        <div>
          <p className="match-review-label">MATCH</p>
          <h2>
            {getText(
              getValue(match, 'name', 'Name', 'matchName', 'MatchName'),
              `${teamAName} vs ${teamBName}`
            )}
          </h2>
          <p>{teamAName} vs {teamBName}</p>
        </div>

        <span className="match-review-status">
          {getText(matchStatus, 'Upcoming')}
        </span>
      </section>

      {showTossDetails && (
        <section className="match-review-details match-review-toss">
          <div className="match-review-section-heading">
            <FaInfoCircle />
            <div>
              <h2>Toss Details</h2>
              <p>Information recorded before the innings started.</p>
            </div>
          </div>

          <div className="match-review-detail-grid">
            <div className="match-review-detail">
              <span>Toss Winner</span>
              <strong>{displayTeam(tossWinner)}</strong>
            </div>

            <div className="match-review-detail">
              <span>Decision</span>
              <strong>
                {getText(tossDecision).toUpperCase()}
              </strong>
            </div>

            <div className="match-review-detail">
              <span>Batting First</span>
              <strong>{displayTeam(firstBattingTeam)}</strong>
            </div>

            <div className="match-review-detail">
              <span>Bowling First</span>
              <strong>{displayTeam(firstBowlingTeam)}</strong>
            </div>
          </div>
        </section>
      )}


      {overallMatchFinished && (
        <section className="match-review-result">
          <div className="match-review-section-heading"><FaTrophy /><div><h2>Match Result</h2><p>Final score and match highlights.</p></div></div>
          <div className="match-review-scoreline">
            <div className="match-review-score-team"><span>{teamAName}</span><strong>{teamAScore ? `${teamAScore.runs}/${teamAScore.wickets}` : '—'}</strong></div>
            <span className="match-review-versus">VS</span>
            <div className="match-review-score-team"><span>{teamBName}</span><strong>{teamBScore ? `${teamBScore.runs}/${teamBScore.wickets}` : '—'}</strong></div>
          </div>
          <p className="match-review-result-text">{getText(resultText, (() => {
              const winner = getValue(match, 'winnerTeam', 'WinnerTeam', 'winner', 'Winner');
              if (!winner) return 'Match completed';
              const winningScore = [teamAScore, teamBScore].find((score) => String(score?.team).toLowerCase() === String(displayTeam(winner)).toLowerCase());
              const losingScore = [teamAScore, teamBScore].find((score) => score && score !== winningScore);
              if (winningScore && losingScore && winningScore.runs > losingScore.runs) return `${displayTeam(winner)} won by ${winningScore.runs - losingScore.runs} runs`;
              if (winningScore && losingScore && winningScore.runs < losingScore.runs) return `${displayTeam(winner)} won`;
              return `${displayTeam(winner)} won`;
            })())}</p>
          <div className="match-review-awards">
            <article className="match-review-award-card"><FaTrophy /><span>Top Run Scorer</span><strong>{topRunScorer ? getPlayerName(topRunScorer.record) : 'Not available'}</strong><small>{topRunScorer ? `${topRunScorer.runs} runs` : 'No batting records'}</small></article>
            <article className="match-review-award-card"><FaBullseye /><span>Top Bowler</span><strong>{topBowler ? getPlayerName(topBowler.record) : 'Not available'}</strong><small>{topBowler ? `${topBowler.wickets} wickets · ${topBowler.runs} runs` : 'No bowling records'}</small></article>
            <article className="match-review-award-card"><FaAward /><span>Player of the Match</span><strong>{playerOfTheMatch ? (typeof playerOfTheMatch === 'string' ? playerOfTheMatch : getPlayerName(playerOfTheMatch)) : 'Not recorded'}</strong><small>Official award</small></article>
          </div>
        </section>
      )}

      {scoringStarted && (
        <nav className="match-review-tabs" aria-label="Match review sections">
          <button type="button" className={activeTab === 'match-info' ? 'active' : ''} aria-pressed={activeTab === 'match-info'} onClick={() => setActiveTab('match-info')}>
            <FaInfoCircle /> Match Info
          </button>
          <button type="button" className={activeTab === 'first-innings' ? 'active' : ''} aria-pressed={activeTab === 'first-innings'} onClick={() => setActiveTab('first-innings')}>
            <FaHistory /> First Innings
          </button>
          {secondInnings && (
            <button type="button" className={activeTab === 'second-innings' ? 'active' : ''} aria-pressed={activeTab === 'second-innings'} onClick={() => setActiveTab('second-innings')}>
              <FaHistory /> Second Innings
            </button>
          )}
        </nav>
      )}

      {(!scoringStarted || activeTab === 'match-info') && (
        <>
      <section className="match-review-details">
        <div className="match-review-section-heading">
          <FaInfoCircle />
          <div>
            <h2>Match Details</h2>
            <p>Review the match setup and schedule.</p>
          </div>
        </div>

        <div className="match-review-detail-grid">
          <div className="match-review-detail">
            <span>Team A</span>
            <strong>{teamAName}</strong>
          </div>

          <div className="match-review-detail">
            <span>Team B</span>
            <strong>{teamBName}</strong>
          </div>

          <div className="match-review-detail">
            <span>Venue</span>
            <strong>
              {getText(
                getValue(match, 'venue', 'Venue', 'locationName', 'LocationName')
              )}
            </strong>
          </div>

          <div className="match-review-detail">
            <span>Scheduled</span>
            <strong>
              {(() => {
                const scheduled = getValue(
                  match,
                  'scheduledAt',
                  'ScheduledAt',
                  'matchDate',
                  'MatchDate'
                );

                if (!scheduled) return '—';

                const date = new Date(scheduled);

                return Number.isNaN(date.getTime())
                  ? String(scheduled)
                  : date.toLocaleString('en-IN');
              })()}
            </strong>
          </div>

          <div className="match-review-detail">
            <span>Overs</span>
            <strong>
              {getText(
                getValue(match, 'overs', 'Overs', 'totalOvers', 'TotalOvers')
              )}
            </strong>
          </div>

          <div className="match-review-detail">
            <span>Players per team</span>
            <strong>
              {getText(
                getValue(
                  match,
                  'playersPerTeam',
                  'PlayersPerTeam',
                  'teamSize',
                  'TeamSize'
                )
              )}
            </strong>
          </div>
        </div>
      </section>

      <section className="match-review-rosters">
        <div className="match-review-section-heading">
          <FaInfoCircle />
          <div>
            <h2>Player Roster</h2>
            <p>Switch between teams to view their players.</p>
          </div>
        </div>

        <div className="match-review-team-switcher">
          <button
            type="button"
            className={activeTeam === 'TeamA' ? 'active' : ''}
            onClick={() => setActiveTeam('TeamA')}
          >
            {teamAName}
          </button>

          <button
            type="button"
            className={activeTeam === 'TeamB' ? 'active' : ''}
            onClick={() => setActiveTeam('TeamB')}
          >
            {teamBName}
          </button>
        </div>

        <div className="match-review-team">
          <h3>{activeTeam === 'TeamA' ? teamAName : teamBName}</h3>

          {visiblePlayers.length === 0 ? (
            <p className="match-review-empty">
              No players were returned for this team.
            </p>
          ) : (
            <ul className="match-review-player-list">
              {visiblePlayers.map((player, index) => {
                const playerId = getValue(
                  player,
                  'matchPlayerId',
                  'MatchPlayerId',
                  'id',
                  'Id'
                );

                const isCaptain = Boolean(
                  getValue(
                    player,
                    'isCaptain',
                    'IsCaptain',
                    'captain',
                    'Captain'
                  )
                );

                return (
                  <li
                    key={playerId ?? `${getPlayerName(player)}-${index}`}
                  >
                    <span>{getPlayerName(player)}</span>
                    {isCaptain && (
                      <span className="match-review-captain">
                        Captain
                      </span>
                    )}
                  </li>
                );
              })}
            </ul>
          )}
        </div>
      </section>


        </>
      )}

      {scoringStarted && activeTab === 'first-innings' && (
        <section className="match-review-first-innings">
          <div className="match-review-section-heading">
            <FaHistory />
            <div>
              <h2>First Innings</h2>
              <p>Batting and bowling history for the opening innings.</p>
            </div>
          </div>

          {!firstInnings ? (
            <div className="match-review-history-placeholder">
              <FaHistory />
              <h3>First innings history is not available yet</h3>
              <p>
                First innings history will appear here after the second innings starts.
              </p>
            </div>
          ) : stateError ? (
            <div className="match-review-history-placeholder">
              <p role="alert">{stateError}</p>
            </div>
          ) : !firstInnings ? (
            <div className="match-review-history-placeholder">
              <p>
                The first innings has been marked as complete, but its
                history has not been returned by the scoring API.
              </p>
            </div>
          ) : (
            <>
              <div className="match-review-innings-score">
                <div>
                  <span>Batting team</span>
                  <strong>{displayTeam(firstBattingTeam)}</strong>
                </div>

                <div>
                  <span>Score</span>
                  <strong>
                    {getText(firstInningsRuns, '0')}/
                    {getText(firstInningsWickets, '0')}
                  </strong>
                </div>

                <div>
                  <span>Overs</span>
                  <strong>
                    {formatOvers(firstInningsBalls, firstInningsOvers)}
                  </strong>
                </div>

                <div>
                  <span>Status</span>
                  <strong>
                    {getText(firstInningsStatus, 'Unknown')}
                  </strong>
                </div>
              </div>

              {hasFirstInningsHistory ? (
                <div className="match-review-history-grid">
                  {renderBattingCard()}
                  {renderBowlingCard()}
                </div>
              ) : (
                <div className="match-review-history-placeholder">
                  <p>
                    First innings history has not been returned by the
                    scoring API yet.
                  </p>
                </div>
              )}
            </>
          )}
        </section>
      )}

      {activeTab === 'second-innings' && (
        <section className="match-review-first-innings">
          <div className="match-review-section-heading"><FaHistory /><div><h2>Second Innings</h2><p>Batting and bowling history for the second innings.</p></div></div>
          {!secondInnings ? (
            <div className="match-review-history-placeholder"><p>Second innings data is not available for this match.</p></div>
          ) : <>
          <div className="match-review-innings-score">
            <div><span>Batting team</span><strong>{displayTeam(secondBattingTeam)}</strong></div>
            <div><span>Score</span><strong>{getText(secondInningsRuns, '0')}/{getText(secondInningsWickets, '0')}</strong></div>
            <div><span>Overs</span><strong>{formatOvers(secondInningsBalls, secondInningsOvers)}</strong></div>
            <div><span>Status</span><strong>{getText(secondInningsStatus, 'Unknown')}</strong></div>
          </div>
          {secondBattingRecords.length === 0 && secondBowlingRecords.length === 0 ? (
            <div className="match-review-history-placeholder"><p>Second innings history has not been returned by the scoring API yet.</p></div>
          ) : (
            <div className="match-review-history-grid">
              <div className="match-review-history-card"><h3>{displayTeam(secondBattingTeam)} — Batting</h3>
                {secondBattingRecords.length ? <div className="match-review-table-wrap"><table className="match-review-table"><thead><tr><th>Player</th><th>R</th><th>B</th><th>4s</th><th>6s</th><th>SR</th><th>Dismissal</th></tr></thead><tbody>{secondBattingRecords.map((record, index) => <tr key={getValue(record, 'matchPlayerId', 'MatchPlayerId') ?? `${getPlayerName(record)}-${index}`}><td>{getPlayerName(record)}</td><td>{getText(getValue(record, 'runs', 'Runs'), '0')}</td><td>{getText(getValue(record, 'balls', 'Balls'), '0')}</td><td>{getText(getValue(record, 'fours', 'Fours'), '0')}</td><td>{getText(getValue(record, 'sixes', 'Sixes'), '0')}</td><td>{getText(getValue(record, 'strikeRate', 'StrikeRate'), '0.00')}</td><td>{getText(getValue(record, 'dismissal', 'Dismissal'), 'Not out')}</td></tr>)}</tbody></table></div> : <p className="match-review-empty">No batting records available.</p>}
              </div>
              <div className="match-review-history-card"><h3>{displayTeam(secondBowlingTeam)} — Bowling</h3>
                {secondBowlingRecords.length ? <div className="match-review-table-wrap"><table className="match-review-table"><thead><tr><th>Bowler</th><th>Overs</th><th>M</th><th>Runs</th><th>Wickets</th><th>Economy</th></tr></thead><tbody>{secondBowlingRecords.map((record, index) => <tr key={getValue(record, 'matchPlayerId', 'MatchPlayerId') ?? `${getPlayerName(record)}-${index}`}><td>{getPlayerName(record)}</td><td>{getText(getValue(record, 'overs', 'Overs'), '0')}</td><td>{getText(getValue(record, 'maidens', 'Maidens'), '0')}</td><td>{getText(getValue(record, 'runsConceded', 'RunsConceded'), '0')}</td><td>{getText(getValue(record, 'wickets', 'Wickets'), '0')}</td><td>{getText(getValue(record, 'economy', 'Economy'), '0.00')}</td></tr>)}</tbody></table></div> : <p className="match-review-empty">No bowling records available.</p>}
              </div>
            </div>
          )}
          </>}
        </section>
      )}

      <footer className="match-review-actions">
        {showUpdateButton && (
          <button
            type="button"
            className="match-review-update"
            onClick={updateMatch}
          >
            Update Match
          </button>
        )}

        <button
          type="button"
          className="match-review-dashboard"
          onClick={scoringStarted && !overallMatchFinished && !isCancelled ? backToScoring : backToDashboard}
        >
          {scoringStarted && !overallMatchFinished && !isCancelled ? 'Back to Scoring' : 'Back to Dashboard'}
        </button>
      </footer>
    </main>
  );
}