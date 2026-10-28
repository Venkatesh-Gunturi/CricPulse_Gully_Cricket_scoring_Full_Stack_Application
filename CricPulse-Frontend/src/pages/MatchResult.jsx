
import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { getMatchState } from '../services/scoringService';
import '../styles/match-result.css';

export default function MatchResult() {
  const { matchId } = useParams();
  const navigate = useNavigate();

  const [matchState, setMatchState] = useState(null);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState('');

  useEffect(() => {
    let cancelled = false;

    async function loadResult() {
      try {
        const state = await getMatchState(matchId);

        if (!cancelled) {
          setMatchState(state);
        }
      } catch (error) {
        if (!cancelled) {
          setPageError(
            error.response?.data?.message ||
              'Unable to load the match result.'
          );
        }
      } finally {
        if (!cancelled) {
          setLoading(false);
        }
      }
    }

    if (!matchId) {
      setPageError('Match ID is missing from the URL.');
      setLoading(false);
    } else {
      loadResult();
    }

    return () => {
      cancelled = true;
    };
  }, [matchId]);

  // Normalize API naming in case the serializer returns camelCase
  // or PascalCase properties.
  const read = (object, camel, pascal) =>
    object?.[camel] ?? object?.[pascal];

  const inningsList = useMemo(() => {
    const innings = read(matchState, 'innings', 'Innings') ?? [];

    return [...innings].sort(
      (a, b) =>
        (read(a, 'inningsNumber', 'InningsNumber') ?? 0) -
        (read(b, 'inningsNumber', 'InningsNumber') ?? 0)
    );
  }, [matchState]);

  const teamAName =
    read(matchState, 'teamAName', 'TeamAName') || 'Team A';

  const teamBName =
    read(matchState, 'teamBName', 'TeamBName') || 'Team B';

  const winnerTeam = read(matchState, 'winnerTeam', 'WinnerTeam');
  const resultText = read(matchState, 'resultText', 'ResultText');
  const status = read(matchState, 'status', 'Status') || 'Completed';

const mvpPlayerId = read(matchState, 'mvpUserId', 'MVPUserId');
const mvpPlayerName = read(matchState, 'mvpPlayerName', 'MVPPlayerName');
const mvpScore = read(matchState, 'mvpScore', 'MVPScore');

  const displayTeam = (team) => {
    if (team === 'TeamA') return teamAName;
    if (team === 'TeamB') return teamBName;

    return team || 'Unknown team';
  };

  const getInitials = (name) => {
    const words = name.trim().split(/\s+/).filter(Boolean);

    if (words.length === 0) return '?';
    if (words.length === 1) return words[0].slice(0, 2).toUpperCase();

    return `${words[0][0]}${words[1][0]}`.toUpperCase();
  };

  const formatOvers = (legalBalls = 0) =>
    `${Math.floor(legalBalls / 6)}.${legalBalls % 6}`;

  const getScoreForTeam = (teamName) => {
    // The latest innings for a team is the one displayed.
    const teamInnings = inningsList.filter(
      (innings) =>
        displayTeam(read(innings, 'battingTeam', 'BattingTeam')) ===
        teamName
    );

    if (teamInnings.length === 0) return null;

    return teamInnings[teamInnings.length - 1];
  };

  const teamAScore = getScoreForTeam(teamAName);
  const teamBScore = getScoreForTeam(teamBName);

  const totalOvers =
    read(matchState, 'overs', 'Overs') ?? null;

  const playersPerTeam =
    read(matchState, 'playersPerTeam', 'PlayersPerTeam') ?? null;

  const isTie =
    !winnerTeam &&
    (resultText?.toLowerCase().includes('tie') ||
      resultText?.toLowerCase().includes('draw'));

  const isNoResult =
    !winnerTeam &&
    !isTie &&
    resultText?.toLowerCase().includes('no result');

  const isCompleted =
    status.toLowerCase() === 'completed';

  // Aggregate each player's records across innings.
  // Only actual backend records are used.
  const battingHighlights = useMemo(() => {
    const totals = new Map();

    inningsList.forEach((innings) => {
      const records =
        read(innings, 'battingRecords', 'BattingRecords') ?? [];

      records.forEach((record) => {
        const id = read(record, 'matchPlayerId', 'MatchPlayerId');
        const name = read(record, 'playerName', 'PlayerName');

        if (!id || !name) return;

        const existing = totals.get(id) ?? {
          id,
          name,
          runs: 0,
          balls: 0,
          fours: 0,
          sixes: 0,
        };

        existing.runs += read(record, 'runs', 'Runs') ?? 0;
        existing.balls += read(record, 'balls', 'Balls') ?? 0;
        existing.fours += read(record, 'fours', 'Fours') ?? 0;
        existing.sixes += read(record, 'sixes', 'Sixes') ?? 0;

        totals.set(id, existing);
      });
    });

    return [...totals.values()].sort(
      (a, b) => b.runs - a.runs || a.balls - b.balls
    );
  }, [inningsList]);

  const bowlingHighlights = useMemo(() => {
    const totals = new Map();

    inningsList.forEach((innings) => {
      const records =
        read(innings, 'bowlingRecords', 'BowlingRecords') ?? [];

      records.forEach((record) => {
        const id = read(record, 'matchPlayerId', 'MatchPlayerId');
        const name = read(record, 'playerName', 'PlayerName');

        if (!id || !name) return;

        const existing = totals.get(id) ?? {
          id,
          name,
          wickets: 0,
          runsConceded: 0,
          legalBalls: 0,
        };

        existing.wickets += read(record, 'wickets', 'Wickets') ?? 0;
        existing.runsConceded +=
          read(record, 'runsConceded', 'RunsConceded') ?? 0;

        // The backend's decimal Overs uses cricket notation
        // (e.g. 2.4 means 2 overs and 4 balls), not decimal time.
        const overs = Number(
          read(record, 'overs', 'Overs') ?? 0
        );

        existing.legalBalls +=
          Math.floor(overs) * 6 +
          Math.round((overs - Math.floor(overs)) * 10);

        totals.set(id, existing);
      });
    });

    return [...totals.values()].sort(
      (a, b) =>
        b.wickets - a.wickets ||
        a.runsConceded - b.runsConceded
    );
  }, [inningsList]);

  const topBatter = battingHighlights[0];
  const topBowler = bowlingHighlights[0];

  const goToDashboard = () => {
    navigate('/umpire', { replace: true });
  };

  if (loading) {
    return (
      <main className="match-result-page">
        <div className="match-result-loading">
          <span className="match-result-spinner" />
          <p>Loading match result...</p>
        </div>
      </main>
    );
  }

  if (pageError) {
    return (
      <main className="match-result-page">
        <section className="match-result-card match-result-error">
          <span className="match-result-error-icon">!</span>
          <h1>Unable to load result</h1>
          <p role="alert">{pageError}</p>

          <button
            type="button"
            className="match-result-dashboard-button"
            onClick={goToDashboard}
          >
            Back to Dashboard
          </button>
        </section>
      </main>
    );
  }

  return (
    <main className="match-result-page">
      <div className="match-result-container">
        <header className="match-result-header">
          <button
            type="button"
            className="match-result-back-button"
            onClick={goToDashboard}
          >
            <span aria-hidden="true">←</span> Back to Dashboard
          </button>

          <span className="match-result-brand">
            CRIC<span>PULSE</span>
          </span>
        </header>

        <section className="match-result-card">
          <div className="match-result-topline">
            <span className="match-result-status">
              <span className="match-result-status-dot" />
              {isCompleted ? 'MATCH COMPLETED' : status.toUpperCase()}
            </span>

            <span className="match-result-format">
              {totalOvers != null
                ? `${totalOvers} overs per innings`
                : 'Match result'}
            </span>
          </div>

          <div className="match-result-title">
            <span className="match-result-trophy" aria-hidden="true">
              🏆
            </span>
            <h1>Match Result</h1>
            <p>That's a wrap!</p>
          </div>

          <div className="match-result-teams">
            <TeamScore
              name={teamAName}
              initials={getInitials(teamAName)}
              score={teamAScore}
              isWinner={
                !!winnerTeam &&
                displayTeam(winnerTeam) === teamAName
              }
              read={read}
              formatOvers={formatOvers}
            />

            <div className="match-result-versus">
              <span>VS</span>
            </div>

            <TeamScore
              name={teamBName}
              initials={getInitials(teamBName)}
              score={teamBScore}
              isWinner={
                !!winnerTeam &&
                displayTeam(winnerTeam) === teamBName
              }
              read={read}
              formatOvers={formatOvers}
            />
          </div>

          <div className="match-result-outcome">
            <span className="match-result-outcome-label">
              {isTie
                ? 'MATCH TIED'
                : isNoResult
                  ? 'NO RESULT'
                  : winnerTeam
                    ? 'WINNER'
                    : 'FINAL RESULT'}
            </span>

            <p>
              {resultText ||
                (winnerTeam
                  ? `${displayTeam(winnerTeam)} won.`
                  : isTie
                    ? 'The match ended in a tie.'
                    : 'The match has ended.')}
            </p>
          </div>
            
            
          {mvpPlayerName && (
            <section className="match-result-mvp">
              <div className="match-result-mvp-trophy" aria-hidden="true">
                🏆
              </div>

              <div className="match-result-mvp-content">
                <span className="match-result-mvp-label">
                  PLAYER OF THE MATCH
                </span>

                <h2>{mvpPlayerName}</h2>

                <p>Outstanding performance</p>
              </div>

              {mvpScore != null && (
                <div className="match-result-mvp-score">
                  <strong>{mvpScore}</strong>
                  <span>MVP POINTS</span>
                </div>
              )}
            </section>
          )}

          <div className="match-result-meta">
            {playersPerTeam != null && (
              <div className="match-result-meta-item">
                <span className="match-result-meta-icon">♟</span>
                <div>
                  <strong>{playersPerTeam} a side</strong>
                  <span>Players per team</span>
                </div>
              </div>
            )}

            {totalOvers != null && (
              <div className="match-result-meta-item">
                <span className="match-result-meta-icon">◷</span>
                <div>
                  <strong>{totalOvers} overs</strong>
                  <span>Match format</span>
                </div>
              </div>
            )}

            <div className="match-result-meta-item">
              <span className="match-result-meta-icon">✓</span>
              <div>
                <strong>{status}</strong>
                <span>Match status</span>
              </div>
            </div>
          </div>
        </section>

        {(topBatter || topBowler) && (
          <section className="match-result-highlights">
            <div className="match-result-section-heading">
              <div>
                <span className="match-result-eyebrow">
                  MATCH HIGHLIGHTS
                </span>
                <h2>Standout Performances</h2>
              </div>
            </div>

            <div className="match-result-highlight-grid">
              {topBatter && (
                <article className="match-result-highlight-card">
                  <div className="match-result-highlight-icon batting-icon">
                    <span aria-hidden="true">🏏</span>
                  </div>

                  <div className="match-result-highlight-copy">
                    <span className="match-result-highlight-label">
                      HIGHEST RUN SCORER
                    </span>
                    <h3>{topBatter.name}</h3>
                    <p>
                      {topBatter.runs} runs
                      <span className="match-result-highlight-separator">
                        ·
                      </span>
                      {topBatter.balls} balls
                    </p>
                    <span className="match-result-highlight-detail">
                      {topBatter.fours} fours · {topBatter.sixes} sixes
                    </span>
                  </div>

                  <div className="match-result-highlight-value">
                    {topBatter.runs}
                    <small>RUNS</small>
                  </div>
                </article>
              )}

              {topBowler && (
                <article className="match-result-highlight-card">
                  <div className="match-result-highlight-icon bowling-icon">
                    <span aria-hidden="true">🎯</span>
                  </div>

                  <div className="match-result-highlight-copy">
                    <span className="match-result-highlight-label">
                      HIGHEST WICKET TAKER
                    </span>
                    <h3>{topBowler.name}</h3>
                    <p>
                      {topBowler.wickets} wickets
                      <span className="match-result-highlight-separator">
                        ·
                      </span>
                      {topBowler.runsConceded} runs conceded
                    </p>
                    <span className="match-result-highlight-detail">
                      {formatOvers(topBowler.legalBalls)} overs bowled
                    </span>
                  </div>

                  <div className="match-result-highlight-value">
                    {topBowler.wickets}
                    <small>WICKETS</small>
                  </div>
                </article>
              )}
            </div>
          </section>
        )}

        <footer className="match-result-footer">
          <p>Good game. Great memories. That's CricPulse.</p>

          <button
            type="button"
            className="match-result-dashboard-button"
            onClick={goToDashboard}
          >
            Back to Dashboard <span aria-hidden="true">→</span>
          </button>
        </footer>
      </div>
    </main>
  );
}

function TeamScore({
  name,
  initials,
  score,
  isWinner,
  read,
  formatOvers,
}) {
  const runs = score
    ? read(score, 'totalRuns', 'TotalRuns')
    : null;

  const wickets = score
    ? read(score, 'wickets', 'Wickets')
    : null;

  const legalBalls = score
    ? read(score, 'legalBalls', 'LegalBalls')
    : null;

  return (
    <article
      className={`match-result-team ${
        isWinner ? 'match-result-team-winner' : ''
      }`}
    >
      <div className="match-result-team-logo">
        {initials}
      </div>

      <h2>{name}</h2>

      <div className="match-result-team-score">
        {score ? (
          <>
            <strong>
              {runs}/{wickets}
            </strong>
            <span>
              {formatOvers(legalBalls ?? 0)} overs
            </span>
          </>
        ) : (
          <>
            <strong>—</strong>
            <span>No innings recorded</span>
          </>
        )}
      </div>

      {isWinner && (
        <span className="match-result-winner-tag">
          <span aria-hidden="true">★</span> Winner
        </span>
      )}
    </article>
  );
}
