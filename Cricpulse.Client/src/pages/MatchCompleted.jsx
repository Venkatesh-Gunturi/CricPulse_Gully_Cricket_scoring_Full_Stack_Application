import React from "react";

const MatchCompleted = ({
  match,
  onBack
}) => {
  const firstInningsTotalRuns =
    match.firstInningsTotalRuns ?? 0;

  const secondInningsTotalRuns =
    match.totalRuns ?? 0;

  const firstInningsBattingTeam =
    match.battingFirstTeam ??
    match.firstInningsBattingTeam ??
    "Team 1";

  const secondInningsBattingTeam =
    firstInningsBattingTeam === match.team1Name
      ? match.team2Name
      : match.team1Name;

  const winner =
    match.result === "Team1Won"
      ? match.team1Name
      : match.result === "Team2Won"
      ? match.team2Name
      : null;

  const secondInningsLegalBalls =
    match.legalBalls ?? 0;

  const secondInningsOvers =
    `${Math.floor(
      secondInningsLegalBalls / 6
    )}.${secondInningsLegalBalls % 6}`;

  return (
    <div className="container mt-5">
      <div className="card shadow-sm">
        <div className="card-body text-center">

          <h2 className="mb-2">
            🏏 Match Completed
          </h2>

          <p className="text-muted mb-4">
            {match.team1Name} vs{" "}
            {match.team2Name}
          </p>

          <div className="mb-4">
            {winner ? (
              <h3 className="text-success">
                {winner} won the match
              </h3>
            ) : (
              <h3 className="text-warning">
                Match Tied
              </h3>
            )}
          </div>

          <div className="row g-3 mb-4">

            <div className="col-md-6">
              <div className="border rounded p-4 h-100">

                <h5 className="mb-3">
                  {firstInningsBattingTeam}
                </h5>

                <h2>
                  {firstInningsTotalRuns}
                </h2>

                <small className="text-muted">
                  First Innings
                </small>

              </div>
            </div>

            <div className="col-md-6">
              <div className="border rounded p-4 h-100">

                <h5 className="mb-3">
                  {secondInningsBattingTeam}
                </h5>

                <h2>
                  {secondInningsTotalRuns}
                </h2>

                <small className="text-muted">
                  Second Innings
                  {" • "}
                  {secondInningsOvers} overs
                </small>

              </div>
            </div>

          </div>

          <button
            className="btn btn-primary px-4"
            onClick={onBack}
          >
            Back to Dashboard
          </button>

        </div>
      </div>
    </div>
  );
};

export default MatchCompleted;

