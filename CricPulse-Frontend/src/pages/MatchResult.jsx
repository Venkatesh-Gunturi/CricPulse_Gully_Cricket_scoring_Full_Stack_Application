import { useEffect, useState } from 'react';
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

    if (matchId) {
      loadResult();
    } else {
      setPageError('Match ID is missing from the URL.');
      setLoading(false);
    }

    return () => {
      cancelled = true;
    };
  }, [matchId]);

  const inningsList = [...(matchState?.innings ?? [])].sort(
    (a, b) => a.inningsNumber - b.inningsNumber
  );

  const displayTeam = (team) => {
    if (team === 'TeamA') return matchState?.teamAName ?? 'Team A';
    if (team === 'TeamB') return matchState?.teamBName ?? 'Team B';
    return team || '—';
  };

  const formatOvers = (legalBalls = 0) =>
    `${Math.floor(legalBalls / 6)}.${legalBalls % 6}`;

  const handleBackToDashboard = () => {
    navigate('/umpire', { replace: true });
  };

  if (loading) {
    return <main className="match-result-page">Loading match result…</main>;
  }

  if (pageError) {
    return (
      <main className="match-result-page">
        <p role="alert">{pageError}</p>
        <button type="button" onClick={handleBackToDashboard}>
          Back to Dashboard
        </button>
      </main>
    );
  }

  const isTie =
    matchState?.resultText?.toLowerCase().includes('tied') ||
    !matchState?.winnerTeam;

  return (
    <main className="match-result-page">
      <section className="match-result-card">
        <h1>Match Completed</h1>

        <p className="match-result-summary">
          {isTie
            ? 'Match Tied'
            : matchState?.resultText || `${displayTeam(matchState?.winnerTeam)} won.`}
        </p>

        <div className="match-result-innings">
          {inningsList.map((innings) => (
            <article
              className="match-result-innings-card"
              key={innings.inningsId}
            >
              <h2>{displayTeam(innings.battingTeam)}</h2>

              <p className="match-result-score">
                {innings.totalRuns}/{innings.wickets}
              </p>

              <p>
                Overs: {formatOvers(innings.legalBalls)}
                {innings.totalOvers ? ` / ${innings.totalOvers}` : ''}
              </p>
            </article>
          ))}
        </div>

        <button
          type="button"
          className="match-result-dashboard-button"
          onClick={handleBackToDashboard}
        >
          OK / Back to Dashboard
        </button>
      </section>
    </main>
  );
}