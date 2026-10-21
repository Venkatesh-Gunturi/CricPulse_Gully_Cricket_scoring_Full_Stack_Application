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

        var extraType =
            dto.ExtraType
                .Trim()
                .ToUpperInvariant();

        if (extraType is not
            ("WIDE" or "NO BALL" or "BYE" or "LEG BYE"))
        {
            throw new ArgumentException(
                "Invalid extra type.");
        }

        if (dto.TotalRuns < 1)
        {
            throw new ArgumentException(
                "Extra total must be at least 1.");
        }

        if (dto.BatterRuns < 0)
        {
            throw new ArgumentException(
                "Batter runs cannot be negative.");
        }

        if (dto.RunsCompleted < 0 ||
            dto.RunsCompleted > 6)
        {
            throw new ArgumentException(
                "Completed runs must be between 0 and 6.");
        }

        /*
         * ----------------------------------------------------------
         * WIDE
         * ----------------------------------------------------------
         *
         * TotalRuns = penalty wide + completed runs.
         *
         * Example:
         * 1 wide       => TotalRuns = 1, RunsCompleted = 0
         * 2 wides      => TotalRuns = 2, RunsCompleted = 1
         *
         * Wide is NOT a legal delivery.
         */
        if (extraType == "WIDE")
        {
            if (dto.BatterRuns != 0)
            {
                throw new ArgumentException(
                    "Wide cannot contain batter runs.");
            }

            if (dto.TotalRuns !=
                dto.RunsCompleted + 1)
            {
                throw new ArgumentException(
                    "Wide total must equal one penalty run plus completed runs.");
            }

            if (dto.DismissedMatchPlayerId.HasValue)
            {
                return await ScoreExtraRunOutAsync(
                    innings,
                    match,
                    dto,
                    extraType,
                    isLegalDelivery: false);
            }

            var ball =
                CreateBaseBall(innings);

            ball.Runs =
                dto.TotalRuns;

            ball.BatterRuns = 0;

            ball.ExtraRuns =
                dto.TotalRuns;

            ball.ExtraType = "WIDE";

            ball.IsLegalDelivery = false;

            /*
             * Wide does NOT consume free hit.
             */
            await RecordBallAsync(
                innings,
                ball,
                preserveFreeHit: true);

            return await GetCurrentMatchStateAsync(
                match.Id);
        }

        /*
         * ----------------------------------------------------------
         * NO BALL
         * ----------------------------------------------------------
         *
         * TotalRuns includes the mandatory no-ball penalty.
         *
         * Example:
         *
         * No ball + 0 batter runs
         * TotalRuns = 1
         *
         * No ball + 4 batter runs
         * TotalRuns = 5
         *
         * No ball is NOT a legal delivery.
         *
         * The following delivery becomes a free hit.
         */
        if (extraType == "NO BALL")
        {
            var minimumPenalty =
                1;

            if (dto.TotalRuns < minimumPenalty)
            {
                throw new ArgumentException(
                    "No ball must contain the penalty run.");
            }

            var additionalRuns =
                dto.TotalRuns - 1;

            if (dto.BatterRuns > additionalRuns)
            {
                throw new ArgumentException(
                    "Batter runs cannot exceed the runs completed on the no-ball.");
            }

            if (dto.RunsCompleted != additionalRuns)
            {
                throw new ArgumentException(
                    "Completed runs must equal no-ball runs excluding the penalty.");
            }

            if (dto.DismissedMatchPlayerId.HasValue)
            {
                return await ScoreExtraRunOutAsync(
                    innings,
                    match,
                    dto,
                    extraType,
                    isLegalDelivery: false);
            }

            var ball =
                CreateBaseBall(innings);

            ball.Runs =
                dto.TotalRuns;

            ball.BatterRuns =
                dto.BatterRuns;

            ball.ExtraRuns =
                dto.TotalRuns -
                dto.BatterRuns;

            ball.ExtraType =
                "NO BALL";

            ball.IsLegalDelivery = false;

            /*
             * No ball creates free hit.
             *
             * RecordBallAsync captures the previous state
             * BEFORE changing IsFreeHit.
             */
            await RecordBallAsync(
                innings,
                ball,
                preserveFreeHit: false,
                freeHitAfterBall: true);

            return await GetCurrentMatchStateAsync(
                match.Id);
        }

        /*
         * ----------------------------------------------------------
         * BYE / LEG BYE
         * ----------------------------------------------------------
         *
         * These are legal deliveries.
         * They do not count as batter runs.
         */
        if (dto.BatterRuns != 0)
        {
            throw new ArgumentException(
                "Bye and leg-bye cannot contain batter runs.");
        }

        if (dto.TotalRuns != dto.RunsCompleted)
        {
            throw new ArgumentException(
                "Bye and leg-bye total must equal completed runs.");
        }

        if (dto.DismissedMatchPlayerId.HasValue)
        {
            return await ScoreExtraRunOutAsync(
                innings,
                match,
                dto,
                extraType,
                isLegalDelivery: true);
        }

        var extraBall =
            CreateBaseBall(innings);

        extraBall.Runs =
            dto.TotalRuns;

        extraBall.BatterRuns = 0;

        extraBall.ExtraRuns =
            dto.TotalRuns;

        extraBall.ExtraType =
            extraType;

        extraBall.IsLegalDelivery = true;

        await RecordBallAsync(
            innings,
            extraBall,
            preserveFreeHit: false);

        return await GetCurrentMatchStateAsync(
            match.Id);
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

        var dismissed =
            GetMatchPlayer(
                match,
                dto.DismissedMatchPlayerId.Value);

        if (dismissed.Team != innings.BattingTeam)
        {
            throw new ArgumentException(
                "Dismissed player must belong to the batting team.");
        }

        ValidateBowlingPlayer(
            match,
            innings,
            dto.RunOutByMatchPlayerId.Value,
            "Invalid run-out player.");

        /*
         * On a free hit, run out is the allowed dismissal.
         */
        if (innings.IsFreeHit &&
            extraType == "NO BALL")
        {
            // Allowed.
        }

        var ball =
            CreateBaseBall(innings);

        ball.Runs =
            dto.TotalRuns;

        ball.BatterRuns =
            dto.BatterRuns;

        ball.ExtraType =
            extraType;

        ball.ExtraRuns =
            extraType switch
            {
                "WIDE" =>
                    dto.TotalRuns,

                "NO BALL" =>
                    dto.TotalRuns -
                    dto.BatterRuns,

                "BYE" or "LEG BYE" =>
                    dto.TotalRuns,

                _ => 0
            };

        ball.IsLegalDelivery =
            isLegalDelivery;

        /*
         * Capture state BEFORE modifying innings.
         */
        CapturePreviousState(
            innings,
            ball);

        await _ballRepository.AddAsync(ball);

        innings.TotalRuns +=
            ball.Runs;

        if (ball.IsLegalDelivery)
        {
            innings.LegalBalls++;
        }

        innings.Wickets++;

        var wicket =
            new Wicket
            {
                Id = Guid.NewGuid(),
                BallId = ball.Id,
                DismissedMatchPlayerId =
                    dismissed.Id,
                WicketType = "RUN OUT",
                RunOutByMatchPlayerId =
                    dto.RunOutByMatchPlayerId,
                RunsCompleted =
                    dto.RunsCompleted
            };

        await _wicketRepository.AddAsync(
            wicket);

        /*
         * Apply completed-run strike rotation BEFORE replacing
         * the dismissed batter.
         */
        if (dto.RunsCompleted % 2 == 1)
        {
            RotateStrike(innings);
        }

        /*
         * A completed over changes ends too.
         */
        if (ball.IsLegalDelivery &&
            innings.LegalBalls % 6 == 0)
        {
            RotateStrike(innings);
        }

        innings.IsFreeHit =
            false;

        var inningsFinished =
            IsInningsFinished(
                innings,
                match);

        if (inningsFinished)
        {
            innings.CompletionPending = true;
        }
        else
        {
            if (!dto.NewBatterMatchPlayerId.HasValue)
            {
                throw new ArgumentException(
                    "A new batter is required after a wicket.");
            }

            var newBatter =
                GetMatchPlayer(
                    match,
                    dto.NewBatterMatchPlayerId.Value);

            ValidateNewBatter(
                match,
                innings,
                dismissed.Id,
                newBatter.Id);

            ReplaceDismissedBatter(
                innings,
                dismissed.Id,
                newBatter.Id);
        }

        innings.UndoUsed = false;

        await _inningsRepository.UpdateAsync(
            innings);

        await _unitOfWork.SaveChangesAsync();

        return await GetCurrentMatchStateAsync(
            match.Id);
    }

    // ============================================================
    // WICKET
    // ============================================================

    public async Task<MatchStateDto> ScoreWicketAsync(
        Guid inningsId,
        ScoreWicketDto dto)
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

        EnsureScoringAllowed(innings);

        var wicketType =
            dto.WicketType
                .Trim()
                .ToUpperInvariant();

        var validTypes =
            new[]
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
            throw new ArgumentException(
                "Invalid wicket type.");
        }

        /*
         * Free hit:
         * only RUN OUT is allowed.
         */
        if (innings.IsFreeHit &&
            wicketType != "RUN OUT")
        {
            throw new InvalidOperationException(
                "Only run out is allowed on a free hit.");
        }

        var dismissed =
            GetMatchPlayer(
                match,
                dto.DismissedMatchPlayerId);

        if (dismissed.Team != innings.BattingTeam)
        {
            throw new ArgumentException(
                "Dismissed player must belong to batting team.");
        }

        if (!IsCurrentBatter(
                innings,
                dismissed.Id))
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

        if (dto.RunsCompleted < 0 ||
            dto.RunsCompleted > 6)
        {
            throw new ArgumentException(
                "Completed runs must be between 0 and 6.");
        }

        ValidateWicketParticipants(
            match,
            innings,
            wicketType,
            dto);

        var ball =
            CreateBaseBall(innings);

        /*
         * Normal wicket scoring:
         *
         * BOWLED / CAUGHT / LBW / STUMPED /
         * HIT WICKET:
         *   completed runs belong to batter.
         *
         * RUN OUT:
         *   completed runs are explicitly supplied by the
         *   run-out modal.
         */
        ball.Runs =
            dto.RunsCompleted;

        ball.BatterRuns =
            dto.RunsCompleted;

        ball.IsLegalDelivery = true;

        CapturePreviousState(
            innings,
            ball);

        await _ballRepository.AddAsync(
            ball);

        innings.TotalRuns +=
            ball.Runs;

        innings.LegalBalls++;
        innings.Wickets++;

        var wicket =
            new Wicket
            {
                Id = Guid.NewGuid(),

                BallId =
                    ball.Id,

                DismissedMatchPlayerId =
                    dismissed.Id,

                WicketType =
                    wicketType,

                CaughtByMatchPlayerId =
                    dto.CaughtByMatchPlayerId,

                RunOutByMatchPlayerId =
                    dto.RunOutByMatchPlayerId,

                StumpedByMatchPlayerId =
                    dto.StumpedByMatchPlayerId,

                RunsCompleted =
                    dto.RunsCompleted
            };

        await _wicketRepository.AddAsync(
            wicket);

        /*
         * Apply completed runs first.
         */
        ApplyStrikeAfterRuns(
            innings,
            dto.RunsCompleted);

        /*
         * Wicket delivery consumes free hit.
         */
        innings.IsFreeHit = false;

        /*
         * Check innings completion before requiring
         * a replacement batter.
         */
        var inningsFinished =
            IsInningsFinished(
                innings,
                match);

        if (inningsFinished)
        {
            innings.CompletionPending = true;
        }
        else
        {
            if (!dto.NewBatterMatchPlayerId.HasValue)
            {
                throw new ArgumentException(
                    "A new batter is required after a wicket.");
            }

            var newBatter =
                GetMatchPlayer(
                    match,
                    dto.NewBatterMatchPlayerId.Value);

            ValidateNewBatter(
                match,
                innings,
                dismissed.Id,
                newBatter.Id);

            ReplaceDismissedBatter(
                innings,
                dismissed.Id,
                newBatter.Id);
        }

        /*
         * End-of-over rotation.
         */
        if (innings.LegalBalls % 6 == 0)
        {
            RotateStrike(innings);
        }

        innings.UndoUsed = false;

        await _inningsRepository.UpdateAsync(
            innings);

        await _unitOfWork.SaveChangesAsync();

        return await GetCurrentMatchStateAsync(
            match.Id);
    }

    // ============================================================
    // COMPLETE INNINGS
    // ============================================================

    public async Task<MatchStateDto> CompleteInningsAsync(
        Guid inningsId,
        CompleteInningsDto dto)
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

        var match =
            await GetMatchAsync(
                innings.MatchId);

        ValidateOrganizer(
            match,
            dto.CreatedByUserId);

        innings.Status = "Completed";
        innings.CompletionPending = false;
        innings.CompletedAt = DateTime.UtcNow;

        await _inningsRepository.UpdateAsync(
            innings);

        var inningsList =
            await _inningsRepository
                .GetByMatchIdAsync(match.Id);

        if (innings.InningsNumber == 2)
        {
            CompleteMatchIfRequired(
                match,
                innings,
                inningsList);

            await _matchRepository.UpdateAsync(
                match);

            await _unitOfWork.SaveChangesAsync();

            /*
             * The completion service is idempotent.
             *
             * If statistics finalization fails, the match remains
             * Finished with PlayerStatisticsFinalized = false.
             * The lifecycle worker can retry it later.
             */
            await _matchCompletionService
                .FinalizeCompletedMatchAsync(
                    match.Id);
        }
        else
        {
            await _unitOfWork.SaveChangesAsync();
        }

        return await GetCurrentMatchStateAsync(
            match.Id);
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
    // ============================================================

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

        var newBatter =
            GetMatchPlayer(
                match,
                newBatterId);

        if (newBatter.Team !=
            innings.BattingTeam)
        {
            throw new ArgumentException(
                "New batter must belong to the batting team.");
        }

        if (newBatter.Id ==
            innings.CurrentStrikerMatchPlayerId ||
            newBatter.Id ==
            innings.CurrentNonStrikerMatchPlayerId)
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

            var matchPlayer = players.FirstOrDefault(
                p => p.Id == matchPlayerId.Value);

            if (matchPlayer?.User == null)
                return "—";

            return $"{matchPlayer.User.FirstName} {matchPlayer.User.LastName}".Trim();
        }

        bool IsDismissed(Guid matchPlayerId, Innings innings)
        {
            return innings.Balls.Any(
                b => b.Wicket?.DismissedMatchPlayerId == matchPlayerId);
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
                        ? Math.Round((decimal)x.TotalRuns / x.LegalBalls * 6, 2)
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

                    var strikerBalls = balls
                        .Where(b =>
                            b.StrikerMatchPlayerId == strikerId &&
                            b.ExtraType != "WIDE")
                        .ToList();

                    var nonStrikerBalls = balls
                        .Where(b =>
                            b.StrikerMatchPlayerId == nonStrikerId &&
                            b.ExtraType != "WIDE")
                        .ToList();

                    var bowlerBalls = balls
                        .Where(b => b.BowlerMatchPlayerId == bowlerId)
                        .ToList();

                    var yetToBat = players
                        .Where(p =>
                            p.Team == x.BattingTeam &&
                            p.Id != strikerId &&
                            p.Id != nonStrikerId &&
                            !IsDismissed(p.Id, x))
                        .Select(p => GetPlayerName(p.Id))
                        .Where(name => name != "—")
                        .ToList();

                    // Keep the last completed over visible until the
                    // first delivery of the next over is recorded.
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

                        StrikerRuns = strikerBalls.Sum(b => b.BatterRuns),
                        StrikerBalls = strikerBalls.Count(b => b.IsLegalDelivery),
                        StrikerFours = strikerBalls.Count(b => b.BatterRuns == 4),
                        StrikerSixes = strikerBalls.Count(b => b.BatterRuns == 6),

                        NonStrikerRuns = nonStrikerBalls.Sum(b => b.BatterRuns),
                        NonStrikerBalls = nonStrikerBalls.Count(b => b.IsLegalDelivery),
                        NonStrikerFours = nonStrikerBalls.Count(b => b.BatterRuns == 4),
                        NonStrikerSixes = nonStrikerBalls.Count(b => b.BatterRuns == 6),

                        BowlerOvers = Math.Round(
                            (decimal)bowlerBalls.Count(b => b.IsLegalDelivery) / 6, 1),
                        BowlerRunsConceded = bowlerRunsConceded,
                        BowlerWickets = bowlerWickets,

                        YetToBat = yetToBat,
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