const FirstInningsCompleted = ({
  battingTeam,
  targetRuns,
  wickets,
  overs,
  onUndoLastBall,
  onCompleteInnings
}) => {
  // Purpose:
  // Display the first-innings completion screen and present the umpire
  // with the two available next actions.
  return (
    <div className="container py-5">

      <div className="text-center">

        <h2 className="mb-4">
          First Innings Completed Successfully
        </h2>

        <hr />

        <div className="my-4">

          <h4>
            {battingTeam} needs to score
          </h4>

          <h1 className="my-3">
            {targetRuns} runs
          </h1>

          <h5>
            with {wickets} wickets within {overs} overs
          </h5>

        </div>

        <hr />

        <p className="mt-4 mb-4">
          Do you want to undo the last ball
          or complete innings?
        </p>

        <div className="d-flex justify-content-center gap-3">

          <button
            type="button"
            className="btn btn-secondary px-4"
            onClick={onUndoLastBall}
          >
            Undo Last Ball
          </button>

          <button
            type="button"
            className="btn btn-primary px-4"
            onClick={onCompleteInnings}
          >
            Complete Innings
          </button>

        </div>

      </div>

    </div>
  );
};

export default FirstInningsCompleted;