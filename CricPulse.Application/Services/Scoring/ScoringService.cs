using CricPulse.Application.DTOs.Scoring;
using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Application.Interfaces.Services;
using CricPulse.Domain.Entities;
using MatchEntity = CricPulse.Domain.Entities.Match;

namespace CricPulse.Application.Services.Scoring;

public class ScoringService : IScoringService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IInningsRepository _inningsRepository;
    private readonly IBallRepository _ballRepository;
    private readonly IWicketRepository _wicketRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMatchCompletionService _matchCompletionService;

    public ScoringService(
        IMatchRepository matchRepository,
        IInningsRepository inningsRepository,
        IBallRepository ballRepository,
        IWicketRepository wicketRepository,
        IUnitOfWork unitOfWork,
        IMatchCompletionService matchCompletionService)
    {
        _matchRepository = matchRepository;
        _inningsRepository = inningsRepository;
        _ballRepository = ballRepository;
        _wicketRepository = wicketRepository;
        _unitOfWork = unitOfWork;
        _matchCompletionService = matchCompletionService;
    }

    // ============================================================
    // START MATCH
    // ============================================================

    public async Task<MatchStateDto> StartMatchAsync(
        StartMatchDto dto)
    {
        ValidateCoordinates(
            dto.Latitude,
            dto.Longitude);

        var match =
            await GetMatchAsync(dto.MatchId);

        ValidateOrganizer(
            match,
            dto.CreatedByUserId);

        var existingLiveMatches =
        await _matchRepository.GetByCreatorAsync(
        dto.CreatedByUserId,
        "Live");

        if (existingLiveMatches.Any(x => x.Id != match.Id))
        {
            throw new InvalidOperationException(
                "You already have a live match. Complete or cancel it before starting another.");
        }

        if (match.Status != "Upcoming")
        {
            throw new InvalidOperationException(
                "Only an upcoming match can be started.");
        }


        ValidateMatchPlayers(match);

        match.ActualStartLatitude =
            dto.Latitude;

        match.ActualStartLongitude =
            dto.Longitude;

        match.ActualStartedAt =
            DateTime.UtcNow;

        match.Status = "Live";
        match.UpdatedAt = DateTime.UtcNow;

        await _matchRepository.UpdateAsync(match);

        await _unitOfWork.SaveChangesAsync();

        return MapState(match);
    }

    // ============================================================
    // TOSS
    // ============================================================

    public async Task<MatchStateDto> RecordTossAsync(
        Guid matchId,
        TossDto dto)
    {
        var match =
            await GetMatchAsync(matchId);

        ValidateOrganizer(
            match,
            dto.CreatedByUserId);

        if (match.Status != "Live")
        {
            throw new InvalidOperationException(
                "Match is not live.");
        }

        if (match.TossAgreed)
        {
            throw new InvalidOperationException(
                "The toss has already been agreed.");
        }

        var winner =
            NormalizeTeam(
                dto.TossWinnerTeam);

        var decision =
            NormalizeDecision(
                dto.TossDecision);

        match.TossWinnerTeam = winner;
        match.TossDecision = decision;
        match.TossAgreed = dto.Agree;
        match.UpdatedAt = DateTime.UtcNow;

        await _matchRepository.UpdateAsync(match);

        await _unitOfWork.SaveChangesAsync();

        return await GetCurrentMatchStateAsync(
            match.Id);
    }

    // ============================================================
    // INNINGS SETUP
    // ============================================================

    public async Task<MatchStateDto> SetupInningsAsync(
        Guid matchId,
        SetupInningsDto dto)
    {
        var match =
            await GetMatchAsync(matchId);

        ValidateOrganizer(
            match,
            dto.CreatedByUserId);

        if (match.Status != "Live")
        {
            throw new InvalidOperationException(
                "Match is not live.");
        }

        if (!match.TossAgreed)
        {
            throw new InvalidOperationException(
                "The toss must be agreed before innings setup.");
        }

        var inningsList =
            await _inningsRepository
                .GetByMatchIdAsync(matchId);

        var liveInnings =
            inningsList.FirstOrDefault(
                x => x.Status == "Live");

        if (liveInnings is not null)
        {
            // An innings is already in progress.
            // Return the current state instead of creating another one.
            return await GetCurrentMatchStateAsync(matchId);
        }

        var inningsNumber =
            inningsList.Count + 1;

        if (inningsNumber > 2)
        {
            throw new InvalidOperationException(
                "Both innings have already been created.");
        }

        string battingTeam;
        string bowlingTeam;
        int? targetRuns = null;

        if (inningsNumber == 1)
        {
            battingTeam =
                GetFirstBattingTeam(match);

            bowlingTeam =
                battingTeam == "TeamA"
                    ? "TeamB"
                    : "TeamA";
        }
        else
        {
            var firstInnings =
                inningsList.FirstOrDefault(
                    x => x.InningsNumber == 1);

            if (firstInnings is null ||
                firstInnings.Status != "Completed")
            {
                throw new InvalidOperationException(
                    "First innings must be completed before starting the second innings.");
            }

            battingTeam =
                firstInnings.BowlingTeam;

            bowlingTeam =
                firstInnings.BattingTeam;

            targetRuns =
                firstInnings.TotalRuns + 1;
        }

        var striker =
            GetMatchPlayer(
                match,
                dto.StrikerMatchPlayerId);

        var nonStriker =
            GetMatchPlayer(
                match,
                dto.NonStrikerMatchPlayerId);

        var bowler =
            GetMatchPlayer(
                match,
                dto.BowlerMatchPlayerId);

        if (striker.Id == nonStriker.Id)
        {
            throw new InvalidOperationException(
                "Striker and non-striker must be different.");
        }

        if (striker.Team != battingTeam ||
            nonStriker.Team != battingTeam)
        {
            throw new InvalidOperationException(
                "Both batters must belong to the batting team.");
        }

        if (bowler.Team != bowlingTeam)
        {
            throw new InvalidOperationException(
                "Bowler must belong to the bowling team.");
        }

        if (bowler.Id == striker.Id ||
            bowler.Id == nonStriker.Id)
        {
            throw new InvalidOperationException(
                "Bowler cannot be one of the current batters.");
        }

        if (await HasPlayerBeenDismissedAsync(
                inningsList,
                striker.Id))
        {
            throw new InvalidOperationException(
                "Striker has already been dismissed.");
        }

        if (await HasPlayerBeenDismissedAsync(
                inningsList,
                nonStriker.Id))
        {
            throw new InvalidOperationException(
                "Non-striker has already been dismissed.");
        }

        var innings = new Innings
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            InningsNumber = inningsNumber,

            BattingTeam = battingTeam,
            BowlingTeam = bowlingTeam,

            TotalRuns = 0,
            Wickets = 0,
            LegalBalls = 0,

            TotalOvers = match.Overs,
            TargetRuns = targetRuns,

            Status = "Live",

            CompletionPending = false,
            IsFreeHit = false,
            UndoUsed = false,

            CurrentStrikerMatchPlayerId =
                striker.Id,

            CurrentNonStrikerMatchPlayerId =
                nonStriker.Id,

            CurrentBowlerMatchPlayerId =
                bowler.Id,

            StartedAt = DateTime.UtcNow
        };

        await _inningsRepository.AddAsync(
            innings);

        await _unitOfWork.SaveChangesAsync();

        return await GetCurrentMatchStateAsync(
            matchId);
    }

    // ============================================================
    // NORMAL RUNS 0 - 6
    // ============================================================

    public async Task<MatchStateDto> ScoreRunsAsync(
        Guid inningsId,
        ScoreRunDto dto)
    {
        if (dto.Runs < 0 ||
            dto.Runs > 6)
        {
            throw new ArgumentException(
                "Runs must be between 0 and 6.");
        }

        var innings =
            await GetLiveInningsAsync(
                inningsId);

        var match =
            await GetMatchAsync(
                innings.MatchId);

        ValidateOrganizer(
            match,
            dto.CreatedByUserId);

        EnsureScoringAllowed(innings);

        var ball =
            CreateBaseBall(innings);

        ball.Runs =
            dto.Runs;

        ball.BatterRuns =
            dto.Runs;

        ball.IsLegalDelivery = true;
        ball.Notation = dto.Runs == 0 ? "0" : dto.Runs.ToString();

        await RecordBallAsync(
            innings,
            ball,
            false);

        return await GetCurrentMatchStateAsync(
            match.Id);
    }

    // ============================================================
    // EXTRAS
    // ============================================================


    public async Task<MatchStateDto> ScoreExtraAsync(
        Guid inningsId,
        ScoreExtraDto dto)
    {
        var innings = await GetLiveInningsAsync(inningsId);
        var match = await GetMatchAsync(innings.MatchId);

        ValidateOrganizer(match, dto.CreatedByUserId);
        EnsureScoringAllowed(innings);

        if (string.IsNullOrWhiteSpace(dto.ExtraType))
        {
            throw new ArgumentException("Extra type is required.");
        }

        var extraType = dto.ExtraType.Trim().ToUpperInvariant();

        if (extraType is not ("WIDE" or "NO BALL" or "BYE" or "LEG BYE"))
        {
            throw new ArgumentException("Invalid extra type.");
        }

        if (dto.TotalRuns < 0)
        {
            throw new ArgumentException(
                "Extra total cannot be negative.");
        }

        if (extraType is "WIDE" or "NO BALL" &&
            dto.TotalRuns < 1)
        {
            throw new ArgumentException(
                "Wide and no-ball totals must be at least 1.");
        }

        if (dto.BatterRuns < 0 || dto.BatterRuns > 6)
        {
            throw new ArgumentException(
                "Batter runs must be between 0 and 6.");
        }

        if (dto.RunsCompleted < 0 || dto.RunsCompleted > 6)
        {
            throw new ArgumentException(
                "Completed runs must be between 0 and 6.");
        }

        switch (extraType)
        {
            case "WIDE":
                if (dto.BatterRuns != 0)
                {
                    throw new ArgumentException(
                        "Wide cannot contain batter runs.");
                }

                if (dto.RunsCompleted > 5 ||
                    dto.TotalRuns != dto.RunsCompleted + 1)
                {
                    throw new ArgumentException(
                        "Wide total must equal one penalty run plus 0–5 completed runs.");
                }
                break;

            case "NO BALL":
                if (dto.TotalRuns != dto.RunsCompleted + 1)
                {
                    throw new ArgumentException(
                        "No-ball total must equal one penalty run plus completed runs.");
                }

                if (dto.BatterRuns > dto.RunsCompleted)
                {
                    throw new ArgumentException(
                        "Batter runs cannot exceed completed runs.");
                }
                break;

            case "BYE":
            case "LEG BYE":
                if (dto.BatterRuns != 0)
                {
                    throw new ArgumentException(
                        "Bye and leg-bye cannot contain batter runs.");
                }

                if (dto.TotalRuns != dto.RunsCompleted)
                {
                    throw new ArgumentException(
                        "Bye and leg-bye totals must equal completed runs.");
                }
                break;
        }

        if (dto.DismissedMatchPlayerId.HasValue)
        {
            return await ScoreExtraRunOutAsync(
                innings,
                match,
                dto,
                extraType,
                isLegalDelivery: extraType is "BYE" or "LEG BYE");
        }

        var ball = CreateBaseBall(innings);

        ball.Runs = dto.TotalRuns;
        ball.BatterRuns = dto.BatterRuns;
        ball.ExtraType = extraType;

        ball.ExtraRuns = extraType switch
        {
            "WIDE" => dto.TotalRuns,
            "NO BALL" => dto.TotalRuns - dto.BatterRuns,
            "BYE" or "LEG BYE" => dto.TotalRuns,
            _ => 0
        };

        ball.IsLegalDelivery = extraType is "BYE" or "LEG BYE";
        ball.Notation = BuildExtraNotation(
            extraType,
            dto.TotalRuns);

        if (extraType == "NO BALL")
        {
            await RecordBallAsync(
                innings,
                ball,
                preserveFreeHit: false,
                freeHitAfterBall: true);
        }
        else
        {
            await RecordBallAsync(
                innings,
                ball,
                preserveFreeHit: extraType == "WIDE");
        }

        return await GetCurrentMatchStateAsync(match.Id);
    }


    // ============================================================
    // EXTRA + RUN OUT
    // ============================================================




    private async Task<MatchStateDto> ScoreExtraRunOutAsync(
        Innings innings,
        MatchEntity match,
        ScoreExtraDto dto,
        string extraType,
        bool isLegalDelivery)
    {
        if (!dto.DismissedMatchPlayerId.HasValue)
        {
            throw new ArgumentException(
                "Dismissed player is required for a run out.");
        }

        if (!dto.RunOutByMatchPlayerId.HasValue)
        {
            throw new ArgumentException(
                "Run-out player is required.");
        }

        if (!IsCurrentBatter(
                innings,
                dto.DismissedMatchPlayerId.Value))
        {
            throw new ArgumentException(
                "Dismissed player must be one of the current batters.");
        }

        var dismissed = GetMatchPlayer(
            match,
            dto.DismissedMatchPlayerId.Value);

        if (dismissed.Team != innings.BattingTeam)
        {
            throw new ArgumentException(
                "Dismissed player must belong to the batting team.");
        }

        if (await HasPlayerBeenDismissedAsync(
                new[] { innings },
                dismissed.Id))
        {
            throw new ArgumentException(
                "This player has already been dismissed in this innings.");
        }

        if (dto.RunsCompleted < 0 || dto.RunsCompleted > 5)
        {
            throw new ArgumentException(
                "Completed runs for an extra run-out must be between 0 and 5.");
        }

        ValidateBowlingPlayer(
            match,
            innings,
            dto.RunOutByMatchPlayerId.Value,
            "Invalid run-out player.");

        /*
         * Project the state after this delivery to determine
         * whether an incoming batter is required.
         */
        var previousTotalRuns = innings.TotalRuns;
        var previousWickets = innings.Wickets;
        var previousLegalBalls = innings.LegalBalls;

        innings.TotalRuns += dto.TotalRuns;
        innings.Wickets++;

        if (isLegalDelivery)
        {
            innings.LegalBalls++;
        }

        var inningsFinished = IsInningsFinished(innings, match);

        innings.TotalRuns = previousTotalRuns;
        innings.Wickets = previousWickets;
        innings.LegalBalls = previousLegalBalls;

        Guid? newBatterId = null;

        if (!inningsFinished)
        {
            if (!dto.NewBatterMatchPlayerId.HasValue)
            {
                throw new ArgumentException(
                    "A new batter is required after a wicket.");
            }

            var newBatter = GetMatchPlayer(
                match,
                dto.NewBatterMatchPlayerId.Value);

            ValidateNewBatter(
                match,
                innings,
                dismissed.Id,
                newBatter.Id);

            if (await HasPlayerBeenDismissedAsync(
                    new[] { innings },
                    newBatter.Id))
            {
                throw new ArgumentException(
                    "This player has already been dismissed in this innings.");
            }

            newBatterId = newBatter.Id;
        }

        var ball = CreateBaseBall(innings);

        ball.Runs = dto.TotalRuns;
        ball.BatterRuns = dto.BatterRuns;
        ball.ExtraType = extraType;

        ball.ExtraRuns = extraType switch
        {
            "WIDE" => dto.TotalRuns,
            "NO BALL" => dto.TotalRuns - dto.BatterRuns,
            "BYE" or "LEG BYE" => dto.TotalRuns,
            _ => 0
        };

        ball.IsLegalDelivery = isLegalDelivery;
        ball.Notation = BuildExtraNotation(
            extraType,
            dto.TotalRuns,
            isWicket: true);

        CapturePreviousState(innings, ball);

        await _ballRepository.AddAsync(ball);

        innings.TotalRuns += ball.Runs;

        if (ball.IsLegalDelivery)
        {
            innings.LegalBalls++;
        }

        innings.Wickets++;

        var wicket = new Wicket
        {
            Id = Guid.NewGuid(),
            BallId = ball.Id,
            DismissedMatchPlayerId = dismissed.Id,
            WicketType = "RUN OUT",
            RunOutByMatchPlayerId = dto.RunOutByMatchPlayerId,
            RunsCompleted = dto.RunsCompleted
        };

        await _wicketRepository.AddAsync(wicket);

        // Preserve the existing DTO and strike-rotation behavior.
        var runsAreOdd = dto.RunsCompleted % 2 != 0;
        var shouldSwapEnds = runsAreOdd != dto.BattersCrossed;

        if (shouldSwapEnds)
        {
            RotateStrike(innings);
        }

        if (isLegalDelivery && innings.LegalBalls % 6 == 0)
        {
            RotateStrike(innings);
        }

        /*
         * Free-hit behavior:
         * - No-ball creates/retains a free hit.
         * - Wide preserves the previous free-hit state.
         * - Legal bye/leg-bye consumes a free hit.
         */
        if (extraType == "NO BALL")
        {
            innings.IsFreeHit = true;
        }
        else if (extraType is "BYE" or "LEG BYE")
        {
            innings.IsFreeHit = false;
        }

        inningsFinished = IsInningsFinished(innings, match);

        if (inningsFinished)
        {
            innings.CompletionPending = true;
        }
        else
        {
            ReplaceDismissedBatter(
                innings,
                dismissed.Id,
                newBatterId!.Value);

            innings.CompletionPending = false;
        }

        innings.UndoUsed = false;

        await _inningsRepository.UpdateAsync(innings);
        await _unitOfWork.SaveChangesAsync();

        return await GetCurrentMatchStateAsync(match.Id);
    }




    // ============================================================
    // WICKET
    // ============================================================


    public async Task<MatchStateDto> ScoreWicketAsync(
        Guid inningsId,
        ScoreWicketDto dto)
    {
        var innings = await GetLiveInningsAsync(inningsId);
        var match = await GetMatchAsync(innings.MatchId);

        ValidateOrganizer(match, dto.CreatedByUserId);
        EnsureScoringAllowed(innings);

        if (string.IsNullOrWhiteSpace(dto.WicketType))
        {
            throw new ArgumentException("Wicket type is required.");
        }

        var wicketType = dto.WicketType.Trim().ToUpperInvariant();

        var validTypes = new[]
        {
        "BOWLED",
        "CAUGHT",
        "RUN OUT",
        "LBW",
        "STUMPED",
        "HIT WICKET"
    };

        if (!validTypes.Contains(wicketType))
        {
            throw new ArgumentException("Invalid wicket type.");
        }

        if (innings.IsFreeHit && wicketType != "RUN OUT")
        {
            throw new InvalidOperationException(
                "Only run out is allowed on a free hit.");
        }

        var completedRuns = wicketType == "RUN OUT"
            ? dto.RunsCompleted
            : 0;

        if (wicketType == "RUN OUT" &&
            (completedRuns < 0 || completedRuns > 3))
        {
            throw new ArgumentException(
                "Completed runs for a run out must be between 0 and 3.");
        }

        if (wicketType != "RUN OUT" && dto.RunsCompleted != 0)
        {
            throw new ArgumentException(
                "Completed runs can only be supplied for a run out.");
        }

        var dismissed = GetMatchPlayer(
            match,
            dto.DismissedMatchPlayerId);

        if (dismissed.Team != innings.BattingTeam)
        {
            throw new ArgumentException(
                "Dismissed player must belong to the batting team.");
        }

        if (!IsCurrentBatter(innings, dismissed.Id))
        {
            throw new ArgumentException(
                "Dismissed player must be one of the current batters.");
        }

        if (await HasPlayerBeenDismissedAsync(
                new[] { innings },
                dismissed.Id))
        {
            throw new ArgumentException(
                "This player has already been dismissed.");
        }

        ValidateWicketParticipants(
            match,
            innings,
            wicketType,
            dto);

        // Project the state after this legal wicket delivery,
        // then restore it before making any persistent changes.
        var previousTotalRuns = innings.TotalRuns;
        var previousWickets = innings.Wickets;
        var previousLegalBalls = innings.LegalBalls;

        innings.TotalRuns += completedRuns;
        innings.Wickets++;
        innings.LegalBalls++;

        var inningsFinished = IsInningsFinished(innings, match);

        innings.TotalRuns = previousTotalRuns;
        innings.Wickets = previousWickets;
        innings.LegalBalls = previousLegalBalls;

        // Validate the incoming batter before adding records.
        Guid? newBatterId = null;

        if (!inningsFinished)
        {
            if (!dto.NewBatterMatchPlayerId.HasValue)
            {
                throw new ArgumentException(
                    "A new batter is required after a wicket.");
            }

            var newBatter = GetMatchPlayer(
                match,
                dto.NewBatterMatchPlayerId.Value);

            ValidateNewBatter(
                match,
                innings,
                dismissed.Id,
                newBatter.Id);

            if (await HasPlayerBeenDismissedAsync(
                    new[] { innings },
                    newBatter.Id))
            {
                throw new ArgumentException(
                    "This player has already been dismissed in this innings.");
            }

            newBatterId = newBatter.Id;
        }

        // All validations have passed. Record the delivery.
        var ball = CreateBaseBall(innings);

        ball.Runs = completedRuns;
        ball.BatterRuns = completedRuns;
        ball.IsLegalDelivery = true;
        ball.Notation = wicketType == "RUN OUT" && completedRuns > 0
            ? $"W+{completedRuns}"
            : "W";

        CapturePreviousState(innings, ball);

        await _ballRepository.AddAsync(ball);

        innings.TotalRuns += ball.Runs;
        innings.LegalBalls++;
        innings.Wickets++;

        var wicket = new Wicket
        {
            Id = Guid.NewGuid(),
            BallId = ball.Id,
            DismissedMatchPlayerId = dismissed.Id,
            WicketType = wicketType,

            CaughtByMatchPlayerId =
                wicketType == "CAUGHT"
                    ? dto.CaughtByMatchPlayerId
                    : null,

            RunOutByMatchPlayerId =
                wicketType == "RUN OUT"
                    ? dto.RunOutByMatchPlayerId
                    : null,

            StumpedByMatchPlayerId =
                wicketType == "STUMPED"
                    ? dto.StumpedByMatchPlayerId
                    : null,

            RunsCompleted = completedRuns
        };

        await _wicketRepository.AddAsync(wicket);

        // Apply completed-run strike rotation before replacing the batter.
        if (wicketType == "RUN OUT")
        {
            var runsAreOdd = completedRuns % 2 != 0;
            var shouldSwapEnds = runsAreOdd != dto.BattersCrossed;

            if (shouldSwapEnds)
            {
                RotateStrike(innings);
            }
        }
        else
        {
            ApplyStrikeAfterRuns(innings, completedRuns);
        }

        innings.IsFreeHit = false;

        // Keep the existing completion-pending flow.
        if (inningsFinished)
        {
            innings.CompletionPending = true;
        }
        else
        {
            ReplaceDismissedBatter(
                innings,
                dismissed.Id,
                newBatterId!.Value);

            innings.CompletionPending = false;
        }

        if (!inningsFinished && innings.LegalBalls % 6 == 0)
        {
            RotateStrike(innings);
        }

        innings.UndoUsed = false;

        await _inningsRepository.UpdateAsync(innings);
        await _unitOfWork.SaveChangesAsync();

        return await GetCurrentMatchStateAsync(match.Id);
    }


    // ============================================================
    // COMPLETE INNINGS
    // ============================================================


    public async Task<MatchStateDto> CompleteInningsAsync(
        Guid inningsId,
        CompleteInningsDto dto)
    {
        var innings = await _inningsRepository.GetByIdAsync(inningsId);

        if (innings is null)
        {
            throw new InvalidOperationException("Innings was not found.");
        }

        if (innings.Status != "Live")
        {
            throw new InvalidOperationException("Innings is not live.");
        }

        var match = await GetMatchAsync(innings.MatchId);

        ValidateOrganizer(match, dto.CreatedByUserId);

        // Do not allow an innings to be completed prematurely.
        if (!IsInningsFinished(innings, match))
        {
            throw new InvalidOperationException(
                "The innings cannot be completed until the overs are exhausted, " +
                "the batting team is all out, or the chasing team reaches its target.");
        }

        // Preserve the existing confirmation flow.
        innings.Status = "Completed";
        innings.CompletionPending = false;
        innings.CompletedAt = DateTime.UtcNow;

        await _inningsRepository.UpdateAsync(innings);

        var inningsList = await _inningsRepository.GetByMatchIdAsync(match.Id);

        if (innings.InningsNumber == 2)
        {
            CompleteMatchIfRequired(match, innings, inningsList);

            await _matchRepository.UpdateAsync(match);
            await _unitOfWork.SaveChangesAsync();

            // Keep statistics finalization idempotent.
            // The lifecycle worker can retry if finalization fails.
            await _matchCompletionService.FinalizeCompletedMatchAsync(match.Id);
        }
        else
        {
            // Completing the first innings must not finish the match.
            await _unitOfWork.SaveChangesAsync();
        }

        return await GetCurrentMatchStateAsync(match.Id);
    }


    // ============================================================
    // CHANGE BOWLER
    // ============================================================

    public async Task<MatchStateDto> ChangeBowlerAsync(
        Guid inningsId,
        ChangeBowlerDto dto)
    {
        var innings =
            await GetLiveInningsAsync(
                inningsId);

        var match =
            await GetMatchAsync(
                innings.MatchId);

        ValidateOrganizer(
            match,
            dto.CreatedByUserId);

        if (innings.CompletionPending)
        {
            throw new InvalidOperationException(
                "Innings completion is pending. Complete the innings first.");
        }

        if (innings.LegalBalls == 0 ||
            innings.LegalBalls % 6 != 0)
        {
            throw new InvalidOperationException(
                "Bowler can be changed only after an over is completed.");
        }

        var newBowler =
            GetMatchPlayer(
                match,
                dto.BowlerMatchPlayerId);

        if (newBowler.Team != innings.BowlingTeam)
        {
            throw new ArgumentException(
                "Bowler must belong to the bowling team.");
        }

        if (newBowler.Id ==
            innings.CurrentStrikerMatchPlayerId ||
            newBowler.Id ==
            innings.CurrentNonStrikerMatchPlayerId)
        {
            throw new ArgumentException(
                "Bowler cannot be one of the current batters.");
        }

        if (innings.CurrentBowlerMatchPlayerId.HasValue &&
            innings.CurrentBowlerMatchPlayerId.Value ==
            newBowler.Id)
        {
            throw new ArgumentException(
                "The same bowler cannot bowl consecutive overs.");
        }

        innings.CurrentBowlerMatchPlayerId =
            newBowler.Id;

        await _inningsRepository.UpdateAsync(
            innings);

        await _unitOfWork.SaveChangesAsync();

        return await GetCurrentMatchStateAsync(
            match.Id);
    }

    // ============================================================
    // UNDO
    // ========================================
    // ====================

    public async Task<MatchStateDto> UndoLastBallAsync(
        Guid inningsId,
        Guid createdByUserId)
    {
        var innings =
            await _inningsRepository
                .GetByIdAsync(inningsId);

        if (innings is null)
        {
            throw new InvalidOperationException(
                "Innings was not found.");
        }
        if (string.Equals(
         innings.Status?.ToString(),
         "Completed",
         StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Cannot undo a ball after the innings has been completed.");
        }

        var match =
            await GetMatchAsync(
                innings.MatchId);

        ValidateOrganizer(
            match,
            createdByUserId);

        var lastBall =
            await _ballRepository
                .GetLastBallAsync(inningsId);

        if (lastBall is null)
        {
            throw new InvalidOperationException(
                "There is no ball to undo.");
        }

        if (innings.UndoUsed)
        {
            throw new InvalidOperationException(
                "Last ball has already been undone. You can only undo one ball at a time, to undo you have to score new ball.");
        }

        var wicket =
            await _wicketRepository
                .GetByBallIdAsync(lastBall.Id);

        if (wicket is not null)
        {
            await _wicketRepository
                .DeleteAsync(wicket);
        }

        /*
         * Restore every piece of scoring state captured
         * before the delivery.
         */
        innings.TotalRuns =
            lastBall.PreviousTotalRuns;

        innings.Wickets =
            lastBall.PreviousWickets;

        innings.LegalBalls =
            lastBall.PreviousLegalBalls;

        innings.CurrentStrikerMatchPlayerId =
            lastBall.PreviousStrikerMatchPlayerId;

        innings.CurrentNonStrikerMatchPlayerId =
            lastBall.PreviousNonStrikerMatchPlayerId;

        innings.CurrentBowlerMatchPlayerId =
            lastBall.PreviousBowlerMatchPlayerId;

        innings.Status =
            lastBall.PreviousInningsStatus;

        innings.CompletionPending =
            lastBall.PreviousCompletionPending;

        innings.IsFreeHit =
            lastBall.PreviousIsFreeHit;

        innings.CompletedAt = null;

        /*
         * Exactly one undo is permitted.
         */
        innings.UndoUsed = true;

        await _ballRepository
            .DeleteAsync(lastBall);

        await _inningsRepository
            .UpdateAsync(innings);

        await _unitOfWork.SaveChangesAsync();

        return await GetCurrentMatchStateAsync(
            match.Id);
    }

    // ============================================================
    // RECORD BALL
    // ============================================================

    private async Task RecordBallAsync(
        Innings innings,
        Ball ball,
        bool preserveFreeHit,
        bool? freeHitAfterBall = null)
    {
        /*
         * IMPORTANT:
         *
         * Previous state MUST be captured before changing
         * IsFreeHit, runs, wickets, strike, etc.
         */
        CapturePreviousState(
            innings,
            ball);

        await _ballRepository.AddAsync(
            ball);

        innings.TotalRuns +=
            ball.Runs;

        if (ball.IsLegalDelivery)
        {
            innings.LegalBalls++;
        }

        /*
         * Completed runs rotate strike.
         */
        ApplyStrikeAfterRuns(
            innings,
            GetCompletedRunsForStrike(
                ball));

        /*
         * Six legal balls means the over is complete.
         */
        if (ball.IsLegalDelivery &&
            innings.LegalBalls % 6 == 0)
        {
            RotateStrike(innings);
        }

        /*
         * Explicit free-hit state.
         */
        if (freeHitAfterBall.HasValue)
        {
            innings.IsFreeHit =
                freeHitAfterBall.Value;
        }
        else if (ball.IsLegalDelivery)
        {
            /*
             * A legal delivery consumes free hit.
             */
            innings.IsFreeHit = false;
        }
        else if (!preserveFreeHit)
        {
            /*
             * Used only when caller explicitly wants to clear it.
             */
            innings.IsFreeHit = false;
        }

        var match =
            await GetMatchAsync(
                innings.MatchId);

        if (IsInningsFinished(
                innings,
                match))
        {
            innings.CompletionPending = true;
        }

        /*
         * A successful new delivery resets the one-undo lock.
         */
        innings.UndoUsed = false;

        await _inningsRepository
            .UpdateAsync(innings);

        await _unitOfWork.SaveChangesAsync();
    }

    private static int GetCompletedRunsForStrike(
        Ball ball)
    {
        /*
         * For normal balls:
         * BatterRuns == completed runs.
         *
         * For Wide:
         * TotalRuns includes the mandatory wide penalty.
         *
         * For No Ball:
         * TotalRuns includes the mandatory no-ball penalty.
         *
         * For Bye / Leg Bye:
         * all runs are completed running.
         */
        return ball.ExtraType switch
        {
            "WIDE" =>
                Math.Max(
                    0,
                    ball.Runs - 1),

            "NO BALL" =>
                Math.Max(
                    0,
                    ball.Runs - 1),

            "BYE" =>
                ball.Runs,

            "LEG BYE" =>
                ball.Runs,

            _ =>
                ball.BatterRuns
        };
    }

    // ============================================================
    // PREVIOUS STATE
    // ============================================================

    private static void CapturePreviousState(
        Innings innings,
        Ball ball)
    {
        if (!innings.CurrentStrikerMatchPlayerId.HasValue ||
            !innings.CurrentNonStrikerMatchPlayerId.HasValue ||
            !innings.CurrentBowlerMatchPlayerId.HasValue)
        {
            throw new InvalidOperationException(
                "Current players are not configured.");
        }

        ball.PreviousStrikerMatchPlayerId =
            innings.CurrentStrikerMatchPlayerId.Value;

        ball.PreviousNonStrikerMatchPlayerId =
            innings.CurrentNonStrikerMatchPlayerId.Value;

        ball.PreviousBowlerMatchPlayerId =
            innings.CurrentBowlerMatchPlayerId.Value;

        ball.PreviousTotalRuns =
            innings.TotalRuns;

        ball.PreviousWickets =
            innings.Wickets;

        ball.PreviousLegalBalls =
            innings.LegalBalls;

        ball.PreviousInningsStatus =
            innings.Status;

        ball.PreviousCompletionPending =
            innings.CompletionPending;

        ball.PreviousIsFreeHit =
            innings.IsFreeHit;
    }

    // ============================================================
    // NEW BATTER
    // ============================================================

    private static void ValidateNewBatter(
     MatchEntity match,
     Innings innings,
     Guid dismissedPlayerId,
     Guid newBatterId)
    {
        if (dismissedPlayerId == newBatterId)
        {
            throw new ArgumentException(
                "Dismissed player cannot return as the new batter.");
        }

        var newBatter = GetMatchPlayer(
            match,
            newBatterId);

        if (newBatter.Team != innings.BattingTeam)
        {
            throw new ArgumentException(
                "New batter must belong to the batting team.");
        }

        if (newBatter.Id == innings.CurrentStrikerMatchPlayerId ||
            newBatter.Id == innings.CurrentNonStrikerMatchPlayerId)
        {
            throw new ArgumentException(
                "New batter is already on the field.");
        }
    }

    private static void ReplaceDismissedBatter(
        Innings innings,
        Guid dismissedPlayerId,
        Guid newBatterId)
    {
        if (innings.CurrentStrikerMatchPlayerId ==
            dismissedPlayerId)
        {
            innings.CurrentStrikerMatchPlayerId =
                newBatterId;

            return;
        }

        if (innings.CurrentNonStrikerMatchPlayerId ==
            dismissedPlayerId)
        {
            innings.CurrentNonStrikerMatchPlayerId =
                newBatterId;

            return;
        }

        throw new InvalidOperationException(
            "Dismissed player is not one of the current batters.");
    }

    // ============================================================
    // STRIKE
    // ============================================================

    private static void ApplyStrikeAfterRuns(
        Innings innings,
        int runs)
    {
        if (runs % 2 == 1)
        {
            RotateStrike(innings);
        }
    }

    private static void RotateStrike(
        Innings innings)
    {
        var striker =
            innings.CurrentStrikerMatchPlayerId;

        innings.CurrentStrikerMatchPlayerId =
            innings.CurrentNonStrikerMatchPlayerId;

        innings.CurrentNonStrikerMatchPlayerId =
            striker;
    }

    // ============================================================
    // DISMISSAL VALIDATION
    // ============================================================

    private static void ValidateWicketParticipants(
        MatchEntity match,
        Innings innings,
        string wicketType,
        ScoreWicketDto dto)
    {
        if (wicketType == "CAUGHT")
        {
            if (!dto.CaughtByMatchPlayerId.HasValue)
            {
                throw new ArgumentException(
                    "Caught by player is required.");
            }

            ValidateBowlingPlayer(
                match,
                innings,
                dto.CaughtByMatchPlayerId.Value,
                "Invalid catcher.");
        }

        if (wicketType == "STUMPED")
        {
            if (!dto.StumpedByMatchPlayerId.HasValue)
            {
                throw new ArgumentException(
                    "Stumping player is required.");
            }

            ValidateBowlingPlayer(
                match,
                innings,
                dto.StumpedByMatchPlayerId.Value,
                "Invalid stumping player.");
        }

        if (wicketType == "RUN OUT")
        {
            if (!dto.RunOutByMatchPlayerId.HasValue)
            {
                throw new ArgumentException(
                    "Run-out player is required.");
            }

            ValidateBowlingPlayer(
                match,
                innings,
                dto.RunOutByMatchPlayerId.Value,
                "Invalid run-out player.");
        }

        if (wicketType != "CAUGHT" &&
            dto.CaughtByMatchPlayerId.HasValue)
        {
            throw new ArgumentException(
                "Caught-by player is only valid for a caught dismissal.");
        }

        if (wicketType != "STUMPED" &&
            dto.StumpedByMatchPlayerId.HasValue)
        {
            throw new ArgumentException(
                "Stumping player is only valid for a stumped dismissal.");
        }

        if (wicketType != "RUN OUT" &&
            dto.RunOutByMatchPlayerId.HasValue)
        {
            throw new ArgumentException(
                "Run-out player is only valid for a run-out dismissal.");
        }
    }

    private static void ValidateBowlingPlayer(
        MatchEntity match,
        Innings innings,
        Guid playerId,
        string error)
    {
        var player =
            match.MatchPlayers.FirstOrDefault(
                x => x.Id == playerId);

        if (player is null ||
            player.Team != innings.BowlingTeam)
        {
            throw new ArgumentException(error);
        }
    }

    // ============================================================
    // MATCH / PLAYER HELPERS
    // ============================================================

    private static MatchPlayer GetMatchPlayer(
        MatchEntity match,
        Guid id)
    {
        var player =
            match.MatchPlayers.FirstOrDefault(
                x => x.Id == id);

        if (player is null)
        {
            throw new ArgumentException(
                "Player does not belong to this match.");
        }

        return player;
    }

    private static bool IsCurrentBatter(
        Innings innings,
        Guid playerId)
    {
        return
            innings.CurrentStrikerMatchPlayerId ==
                playerId ||
            innings.CurrentNonStrikerMatchPlayerId ==
                playerId;
    }

    private static string GetFirstBattingTeam(
        MatchEntity match)
    {
        if (match.TossWinnerTeam is null ||
            match.TossDecision is null)
        {
            throw new InvalidOperationException(
                "Toss information is incomplete.");
        }

        if (match.TossDecision == "BAT")
        {
            return match.TossWinnerTeam;
        }

        return match.TossWinnerTeam == "TeamA"
            ? "TeamB"
            : "TeamA";
    }

    private static string NormalizeTeam(
        string value)
    {
        return value
            .Trim()
            .ToLowerInvariant() switch
        {
            "teama" or "team a" =>
                "TeamA",

            "teamb" or "team b" =>
                "TeamB",

            _ =>
                throw new ArgumentException(
                    "Team must be TeamA or TeamB.")
        };
    }

    private static string NormalizeDecision(
        string value)
    {
        return value
            .Trim()
            .ToUpperInvariant() switch
        {
            "BAT" or "BATTING" =>
                "BAT",

            "BOWL" or "BOWLING" =>
                "BOWL",

            _ =>
                throw new ArgumentException(
                    "Toss decision must be Bat or Bowl.")
        };
    }

    private static void ValidateOrganizer(
        MatchEntity match,
        Guid createdByUserId)
    {
        if (match.CreatedByUserId !=
            createdByUserId)
        {
            throw new InvalidOperationException(
                "Only the match creator/organizer can perform this action.");
        }
    }

    private static void ValidateMatchPlayers(
        MatchEntity match)
    {
        var teamA =
            match.MatchPlayers.Count(
                x => x.Team == "TeamA");

        var teamB =
            match.MatchPlayers.Count(
                x => x.Team == "TeamB");

        if (teamA != match.PlayersPerTeam ||
            teamB != match.PlayersPerTeam)
        {
            throw new InvalidOperationException(
                "Both teams must have the required number of players.");
        }
    }

    // ============================================================
    // INNINGS COMPLETION
    // ============================================================

    private static bool IsInningsFinished(
        Innings innings,
        MatchEntity match)
    {
        /*
         * All but one batter dismissed.
         */
        if (innings.Wickets >=
            match.PlayersPerTeam - 1)
        {
            return true;
        }

        /*
         * Overs exhausted.
         */
        if (innings.LegalBalls >=
            match.Overs * 6)
        {
            return true;
        }

        /*
         * Chasing team reached target.
         */
        if (innings.TargetRuns.HasValue &&
            innings.TotalRuns >=
            innings.TargetRuns.Value)
        {
            return true;
        }

        return false;
    }

    // ============================================================
    // DISMISSED PLAYER CHECK
    // ============================================================

    private static bool HasPlayerBeenDismissed(
        IEnumerable<Innings> inningsList,
        Guid playerId)
    {
        foreach (var innings in inningsList)
        {
            foreach (var ball in innings.Balls)
            {
                if (ball.Wicket is null)
                    continue;

                if (ball.Wicket
                        .DismissedMatchPlayerId ==
                    playerId)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static Task<bool> HasPlayerBeenDismissedAsync(
        IEnumerable<Innings> inningsList,
        Guid playerId)
    {
        return Task.FromResult(
            HasPlayerBeenDismissed(
                inningsList,
                playerId));
    }

    // ============================================================
    // MATCH COMPLETION
    // ============================================================

    private static void CompleteMatchIfRequired(
         MatchEntity match,
         Innings secondInnings,
         IReadOnlyList<Innings> inningsList)
    {
        var firstInnings =
            inningsList.FirstOrDefault(
                x => x.InningsNumber == 1);

        if (firstInnings is null)
            return;

        /*
         * Only the second innings can finish the match.
         */
        match.Status = "Finished";
        match.FinishedAt = DateTime.UtcNow;

        if (secondInnings.TotalRuns >
            firstInnings.TotalRuns)
        {
            match.WinnerTeam =
                secondInnings.BattingTeam;

            match.ResultText =
                $"{GetTeamDisplayName(match, secondInnings.BattingTeam)} won by {secondInnings.Wickets} wickets.";
        }
        else if (secondInnings.TotalRuns <
                 firstInnings.TotalRuns)
        {
            var runs =
                firstInnings.TotalRuns -
                secondInnings.TotalRuns;

            match.WinnerTeam =
                firstInnings.BattingTeam;

            match.ResultText =
                $"{GetTeamDisplayName(match, firstInnings.BattingTeam)} won by {runs} runs.";
        }
        else
        {
            match.WinnerTeam = null;

            match.ResultText =
                "Match tied.";
        }

        match.UpdatedAt =
            DateTime.UtcNow;
    }

    private static string GetTeamDisplayName(
        MatchEntity match,
        string team)
    {
        return team == "TeamA"
            ? match.TeamAName
            : match.TeamBName;
    }

    // ============================================================
    // MATCH RETRIEVAL
    // ============================================================

    private async Task<MatchEntity> GetMatchAsync(
        Guid matchId)
    {
        var match =
            await _matchRepository
                .GetByIdAsync(matchId);

        if (match is null)
        {
            throw new InvalidOperationException(
                "Match was not found.");
        }

        return match;
    }

    private async Task<Innings> GetLiveInningsAsync(
        Guid inningsId)
    {
        var innings =
            await _inningsRepository
                .GetByIdAsync(inningsId);

        if (innings is null)
        {
            throw new InvalidOperationException(
                "Innings was not found.");
        }

        if (innings.Status != "Live")
        {
            throw new InvalidOperationException(
                "Innings is not live.");
        }

        return innings;
    }

    private static void EnsureScoringAllowed(
        Innings innings)
    {
        if (innings.CompletionPending)
        {
            throw new InvalidOperationException(
                "Innings completion is pending. Complete or undo the last ball first.");
        }
    }

    // ============================================================
    // BALL NOTATION
    // ============================================================

    private static string BuildExtraNotation(
        string extraType,
        int totalRuns,
        bool isWicket = false)
    {
        var prefix = extraType switch
        {
            "WIDE" => "WD",
            "NO BALL" => "NB",
            "BYE" => "B",
            "LEG BYE" => "LB",
            _ => totalRuns.ToString()
        };

        // Wide and no-ball totals include one mandatory penalty run.
        var additionalRuns = extraType is "WIDE" or "NO BALL"
            ? totalRuns - 1
            : totalRuns;

        var notation = additionalRuns > 0
            ? $"{prefix}+{additionalRuns}"
            : prefix;

        return isWicket ? $"{notation} W" : notation;
    }

    // ============================================================
    // BALL CREATION
    // ============================================================

    private static Ball CreateBaseBall(
        Innings innings)
    {
        if (!innings.CurrentStrikerMatchPlayerId.HasValue ||
            !innings.CurrentNonStrikerMatchPlayerId.HasValue ||
            !innings.CurrentBowlerMatchPlayerId.HasValue)
        {
            throw new InvalidOperationException(
                "Striker, non-striker and bowler must be selected.");
        }

        /*
         * BallNumber represents the legal-ball position
         * within the over.
         *
         * Therefore wides/no-balls do not advance it.
         */
        var ballNumber =
            innings.LegalBalls % 6 + 1;

        return new Ball
        {
            Id = Guid.NewGuid(),

            InningsId =
                innings.Id,

            OverNumber =
                innings.LegalBalls / 6 + 1,

            BallNumber =
                ballNumber,

            StrikerMatchPlayerId =
                innings.CurrentStrikerMatchPlayerId.Value,

            NonStrikerMatchPlayerId =
                innings.CurrentNonStrikerMatchPlayerId.Value,

            BowlerMatchPlayerId =
                innings.CurrentBowlerMatchPlayerId.Value,

            CreatedAt =
                DateTime.UtcNow
        };
    }

    // ============================================================
    // COORDINATES
    // ============================================================

    private static void ValidateCoordinates(
        double latitude,
        double longitude)
    {
        if (double.IsNaN(latitude) ||
            double.IsInfinity(latitude) ||
            latitude is < -90 or > 90)
        {
            throw new ArgumentException(
                "Invalid latitude.");
        }

        if (double.IsNaN(longitude) ||
            double.IsInfinity(longitude) ||
            longitude is < -180 or > 180)
        {
            throw new ArgumentException(
                "Invalid longitude.");
        }
    }

    // ============================================================
    // STATE
    // ============================================================

    private async Task<MatchStateDto> GetCurrentMatchStateAsync(
        Guid matchId)
    {
        var match =
            await GetMatchAsync(
                matchId);

        return MapState(match);
    }


    private static MatchStateDto MapState(MatchEntity match)
    {
        var players = match.MatchPlayers.ToList();

        string GetPlayerName(Guid? matchPlayerId)
        {
            if (!matchPlayerId.HasValue)
                return "—";

            var player = players.FirstOrDefault(
                p => p.Id == matchPlayerId.Value);

            if (player?.User == null)
                return "—";

            return $"{player.User.FirstName} {player.User.LastName}".Trim();
        }

        return new MatchStateDto
        {
            MatchId = match.Id,
            Status = match.Status,
            TeamAName = match.TeamAName,
            TeamBName = match.TeamBName,
            Overs = match.Overs,
            PlayersPerTeam = match.PlayersPerTeam,
            TossWinnerTeam = match.TossWinnerTeam,
            TossDecision = match.TossDecision,
            TossAgreed = match.TossAgreed,
            WinnerTeam = match.WinnerTeam,
            ResultText = match.ResultText,

            Innings = match.Innings
                .OrderBy(x => x.InningsNumber)
                .Select(x =>
                {
                    // Every calculation below uses this innings' balls only.
                    var balls = x.Balls
                        .OrderBy(b => b.OverNumber)
                        .ThenBy(b => b.BallNumber)
                        .ThenBy(b => b.CreatedAt)
                        .ToList();

                    int? runsRequired = x.TargetRuns.HasValue
                        ? Math.Max(0, x.TargetRuns.Value - x.TotalRuns)
                        : null;

                    int? ballsRemaining = x.TargetRuns.HasValue
                        ? Math.Max(0, x.TotalOvers * 6 - x.LegalBalls)
                        : null;

                    decimal? currentRunRate = x.LegalBalls > 0
                        ? Math.Round(
                            (decimal)x.TotalRuns / x.LegalBalls * 6, 2)
                        : null;

                    decimal? requiredRunRate =
                        runsRequired.HasValue &&
                        ballsRemaining.HasValue &&
                        ballsRemaining.Value > 0
                            ? Math.Round(
                                (decimal)runsRequired.Value /
                                ballsRemaining.Value * 6, 2)
                            : null;

                    var strikerId = x.CurrentStrikerMatchPlayerId;
                    var nonStrikerId = x.CurrentNonStrikerMatchPlayerId;
                    var bowlerId = x.CurrentBowlerMatchPlayerId;

                    // Current batter statistics.
                    var strikerBalls = strikerId.HasValue
                        ? balls.Where(b =>
                            b.StrikerMatchPlayerId == strikerId.Value &&
                            b.ExtraType != "WIDE").ToList()
                        : new List<Ball>();

                    var nonStrikerBalls = nonStrikerId.HasValue
                        ? balls.Where(b =>
                            b.StrikerMatchPlayerId == nonStrikerId.Value &&
                            b.ExtraType != "WIDE").ToList()
                        : new List<Ball>();

                    var bowlerBalls = bowlerId.HasValue
                        ? balls.Where(b =>
                            b.BowlerMatchPlayerId == bowlerId.Value).ToList()
                        : new List<Ball>();

                    // Players who have participated in a delivery as a batter.
                    var playersWhoHaveBatted = balls
                        .SelectMany(b => new[]
                        {
                        b.StrikerMatchPlayerId,
                        b.NonStrikerMatchPlayerId
                        })
                        .ToHashSet();

                    // Players dismissed during this innings.
                    var dismissedPlayerIds = balls
                        .Where(b => b.Wicket != null)
                        .Select(b => b.Wicket!.DismissedMatchPlayerId)
                        .ToHashSet();

                    // Yet-to-bat players must not include either current batter,
                    // a player who has already batted, or a dismissed player.
                    var yetToBatPlayers = players
                        .Where(p =>
                            p.Team == x.BattingTeam &&
                            p.Id != strikerId &&
                            p.Id != nonStrikerId &&
                            !playersWhoHaveBatted.Contains(p.Id) &&
                            !dismissedPlayerIds.Contains(p.Id))
                        .Select(p => new YetToBatPlayerDto
                        {
                            MatchPlayerId = p.Id,
                            Name = GetPlayerName(p.Id)
                        })
                        .Where(p => p.Name != "—")
                        .ToList();

                    var yetToBat = yetToBatPlayers
                        .Select(p => p.Name)
                        .ToList();

                    // Player options used by the scoring controls.
                    var battingPlayers = players
                        .Where(p => p.Team == x.BattingTeam)
                        .Select(p => new MatchPlayerOptionDto
                        {
                            MatchPlayerId = p.Id,
                            Name = GetPlayerName(p.Id)
                        })
                        .Where(p => p.Name != "—")
                        .ToList();

                    var fieldingPlayers = players
                        .Where(p => p.Team == x.BowlingTeam)
                        .Select(p => new MatchPlayerOptionDto
                        {
                            MatchPlayerId = p.Id,
                            Name = GetPlayerName(p.Id)
                        })
                        .Where(p => p.Name != "—")
                        .ToList();

                    // Batting scorecard:
                    // include players who faced a delivery or were dismissed.
                    // Players who have not batted remain in YetToBat.
                    var battingRecords = players
                        .Where(p =>
                            p.Team == x.BattingTeam &&
                            (playersWhoHaveBatted.Contains(p.Id) ||
                             dismissedPlayerIds.Contains(p.Id)))
                        .Select(p =>
                        {
                            var playerBalls = balls
                                .Where(b => b.StrikerMatchPlayerId == p.Id)
                                .ToList();

                            var playerWicket = balls
                                .Where(b => b.Wicket != null)
                                .Select(b => b.Wicket!)
                                .FirstOrDefault(w =>
                                    w.DismissedMatchPlayerId == p.Id);

                            var runs = playerBalls.Sum(b => b.BatterRuns);

                            // A wide is not a ball faced.
                            // A no-ball is counted as faced under this rule.
                            var facedBalls = playerBalls.Count(b =>
                                b.ExtraType != "WIDE" &&
                                (b.IsLegalDelivery ||
                                 b.ExtraType == "NO BALL"));

                            var fours = playerBalls.Count(
                                b => b.BatterRuns == 4);

                            var sixes = playerBalls.Count(
                                b => b.BatterRuns == 6);

                            return new BatterRecordDto
                            {
                                MatchPlayerId = p.Id,
                                PlayerName = GetPlayerName(p.Id),
                                Runs = runs,
                                Balls = facedBalls,
                                Fours = fours,
                                Sixes = sixes,
                                StrikeRate = facedBalls > 0
                                    ? Math.Round(
                                        (decimal)runs / facedBalls * 100, 2)
                                    : 0,
                                Dismissal = playerWicket == null
                                    ? "Not out"
                                    : playerWicket.WicketType
                            };
                        })
                        .Where(p => p.PlayerName != "—")
                        .ToList();

                    // Bowling scorecard: calculate each bowler independently.
                    var bowlingRecords = players
                       .Where(p =>
                        p.Team == x.BowlingTeam &&
                        balls.Any(b => b.BowlerMatchPlayerId == p.Id))
                        .Select(p =>
                        {
                            var playerBalls = balls
                                .Where(b => b.BowlerMatchPlayerId == p.Id)
                                .ToList();

                            var legalBallsBowled = playerBalls.Count(
                                b => b.IsLegalDelivery);

                            // Byes and leg byes are not charged to the bowler.
                            var runsConceded = playerBalls
                                .Where(b =>
                                    b.ExtraType != "BYE" &&
                                    b.ExtraType != "LEG BYE")
                                .Sum(b => b.Runs);

                            var wickets = playerBalls.Count(b =>
                                b.Wicket != null &&
                                b.Wicket.WicketType != "RUN OUT" &&
                                b.Wicket.WicketType != "RETIRED HURT" &&
                                b.Wicket.WicketType != "OBSTRUCTING THE FIELD");

                            // Cricket overs display as completed overs and
                            // remaining balls: 2.4 means 2 overs and 4 balls.
                            var completedOvers = legalBallsBowled / 6;
                            var remainingBalls = legalBallsBowled % 6;

                            // Economy uses actual legal balls bowled.
                            var economy = legalBallsBowled > 0
                                ? Math.Round(
                                    (decimal)runsConceded * 6 /
                                    legalBallsBowled, 2)
                                : 0;

                            // A maiden is a completed over with zero runs
                            // charged to the bowler. This assumes OverNumber
                            // starts at 1 and increments each over.
                            var maidens = playerBalls
                                .GroupBy(b => b.OverNumber)
                                .Count(over =>
                                    over.Count(b => b.IsLegalDelivery) == 6 &&
                                    over
                                        .Where(b =>
                                            b.ExtraType != "BYE" &&
                                            b.ExtraType != "LEG BYE")
                                        .Sum(b => b.Runs) == 0);

                            return new BowlerRecordDto
                            {
                                MatchPlayerId = p.Id,
                                PlayerName = GetPlayerName(p.Id),
                                Overs = completedOvers +
                                        remainingBalls / 10m,
                                Maidens = maidens,
                                RunsConceded = runsConceded,
                                Wickets = wickets,
                                Economy = economy
                            };
                        })
                        .Where(p => p.PlayerName != "—")
                        .ToList();

                    // Keep the last completed over visible until the first
                    // delivery of the next over is recorded.
                    var currentOverNumber = x.LegalBalls / 6 + 1;

                    var currentOverBalls = balls
                        .Where(b => b.OverNumber == currentOverNumber)
                        .ToList();

                    if (currentOverBalls.Count == 0 && x.LegalBalls > 0)
                    {
                        currentOverNumber = (x.LegalBalls - 1) / 6 + 1;

                        currentOverBalls = balls
                            .Where(b => b.OverNumber == currentOverNumber)
                            .ToList();
                    }

                    var currentOver = currentOverBalls
                        .Select(b =>
                            string.IsNullOrWhiteSpace(b.Notation)
                                ? b.Runs.ToString()
                                : b.Notation)
                        .ToList();

                    // Current bowler figures, independent of other bowlers.
                    var bowlerRunsConceded = bowlerBalls
                        .Where(b =>
                            b.ExtraType != "BYE" &&
                            b.ExtraType != "LEG BYE")
                        .Sum(b => b.Runs);

                    var bowlerWickets = bowlerBalls.Count(b =>
                        b.Wicket != null &&
                        b.Wicket.WicketType != "RUN OUT" &&
                        b.Wicket.WicketType != "RETIRED HURT" &&
                        b.Wicket.WicketType != "OBSTRUCTING THE FIELD");

                    var currentBowlerLegalBalls = bowlerBalls.Count(
                        b => b.IsLegalDelivery);

                    return new InningsStateDto
                    {
                        InningsId = x.Id,
                        InningsNumber = x.InningsNumber,
                        BattingTeam = x.BattingTeam,
                        BowlingTeam = x.BowlingTeam,
                        TotalRuns = x.TotalRuns,
                        Wickets = x.Wickets,
                        LegalBalls = x.LegalBalls,
                        TotalOvers = x.TotalOvers,
                        TargetRuns = x.TargetRuns,
                        RunsRequired = runsRequired,
                        BallsRemaining = ballsRemaining,
                        CurrentRunRate = currentRunRate,
                        RequiredRunRate = requiredRunRate,
                        Status = x.Status,
                        CompletionPending = x.CompletionPending,
                        IsFreeHit = x.IsFreeHit,
                        UndoUsed = x.UndoUsed,

                        StrikerMatchPlayerId = strikerId,
                        NonStrikerMatchPlayerId = nonStrikerId,
                        BowlerMatchPlayerId = bowlerId,

                        StrikerName = GetPlayerName(strikerId),
                        NonStrikerName = GetPlayerName(nonStrikerId),
                        BowlerName = GetPlayerName(bowlerId),

                        BattingPlayers = battingPlayers,
                        FieldingPlayers = fieldingPlayers,
                        YetToBatPlayers = yetToBatPlayers,
                        YetToBat = yetToBat,

                        BattingRecords = battingRecords,
                        BowlingRecords = bowlingRecords,

                        StrikerRuns = strikerBalls.Sum(b => b.BatterRuns),
                        StrikerBalls = strikerBalls.Count(b => b.IsLegalDelivery),
                        StrikerFours = strikerBalls.Count(b => b.BatterRuns == 4),
                        StrikerSixes = strikerBalls.Count(b => b.BatterRuns == 6),

                        NonStrikerRuns = nonStrikerBalls.Sum(b => b.BatterRuns),
                        NonStrikerBalls = nonStrikerBalls.Count(b => b.IsLegalDelivery),
                        NonStrikerFours = nonStrikerBalls.Count(b => b.BatterRuns == 4),
                        NonStrikerSixes = nonStrikerBalls.Count(b => b.BatterRuns == 6),

                        BowlerOvers = Math.Floor(
                            (decimal)currentBowlerLegalBalls / 6) +
                            (currentBowlerLegalBalls % 6) / 10m,

                        BowlerRunsConceded = bowlerRunsConceded,
                        BowlerWickets = bowlerWickets,

                        CurrentOver = currentOver
                    };
                })
                .ToList()
        };
    }


    public async Task<MatchStateDto> GetMatchStateAsync(
    Guid matchId)
    {
        return await GetCurrentMatchStateAsync(matchId);
    }
}