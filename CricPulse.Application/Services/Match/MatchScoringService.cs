using System.Diagnostics.CodeAnalysis;
using CricPulse.Application.DTOs.Match;
using CricPulse.Application.Interfaces.Match;
using CricPulse.Domain.Entities;
using CricPulse.Domain.Enums;
using MatchEntity = CricPulse.Domain.Entities.Match;

namespace CricPulse.Application.Services
{
    public class MatchScoringService : IMatchScoringService
    {
        private readonly IScoringRepository _scoringRepository;

        private static readonly HashSet<int> ValidBatRuns =
            new() { 0, 1, 2, 3, 4, 6 };

        private static readonly HashSet<string> ValidWicketTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "BOWLED",
                "CAUGHT",
                "RUN OUT",
                "LBW",
                "STUMPED",
                "HIT WICKET"
            };

        private static readonly HashSet<string> ValidExtraTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "WIDE",
                "NO BALL",
                "BYE",
                "LEG BYE"
            };

        public MatchScoringService(IScoringRepository scoringRepository)
        {
            _scoringRepository = scoringRepository;
        }

        // ====================================================================
        // SCORE NORMAL BATTER RUNS
        // ====================================================================

        public async Task<bool> ScoreRunsAsync(
            int umpireId,
            int inningsId,
            int runs)
        {
            if (!ValidBatRuns.Contains(runs))
                return false;

            var innings =
                await _scoringRepository.GetInningsForScoringAsync(inningsId);

            if (!IsValidLiveInnings(innings))
                return false;

            if (!await IsAuthorizedUmpire(innings!, umpireId))
                return false;

            if (!HasCurrentBowler(innings!))
                return false;

            var striker = innings!.StrikerMatchPlayerId;
            var nonStriker = innings.NonStrikerMatchPlayerId;
            var bowler = innings.CurrentBowlerMatchPlayerId ?? 0;

            var ball = CreateBallSnapshot(
                innings,
                striker,
                nonStriker,
                bowler);

            ball.Runs = runs;
            ball.BatterRuns = runs;
            ball.ExtraRuns = 0;
            ball.ExtraType = null;
            ball.IsLegalDelivery = true;
            ball.Notation = runs.ToString();

            innings.TotalRuns += runs;
            innings.LegalBalls++;

            RotateStrikeForRuns(innings, runs);

            innings.IsFreeHit = false;

            await FinalizeDeliveryAsync(innings, ball);

            return true;
        }

        // ====================================================================
        // SCORE WICKET
        // ====================================================================

        public async Task<bool> ScoreWicketAsync(
            int umpireId,
            ScoreWicketDto dto)
        {
            if (dto == null)
                throw new InvalidOperationException("Wicket request is null.");

            var innings =
                await _scoringRepository.GetInningsForScoringAsync(dto.InningsId);

            if (!IsValidLiveInnings(innings))
                throw new InvalidOperationException("Innings is not live or the match is not live.");

            if (!await IsAuthorizedUmpire(innings!, umpireId))
                throw new InvalidOperationException("Current user is not the match umpire.");

            if (!HasCurrentBowler(innings!))
                throw new InvalidOperationException("Current bowler is missing.");

            var wicketType = NormalizeWicketType(dto.WicketType);

            if (!ValidWicketTypes.Contains(wicketType))
                throw new InvalidOperationException($"Invalid wicket type: '{wicketType}'.");

            // Free-hit deliveries only allow the supported dismissal
            // that remains legal here: run out.
            if (innings!.IsFreeHit && wicketType != "RUN OUT")
                throw new InvalidOperationException("Only Run Out is allowed on a free-hit delivery.");

            var strikerAtStart = innings.StrikerMatchPlayerId;
            var nonStrikerAtStart = innings.NonStrikerMatchPlayerId;
            var bowler = innings.CurrentBowlerMatchPlayerId ?? 0;

            var dismissedId =
                wicketType == "RUN OUT"
                    ? dto.DismissedMatchPlayerId
                    : strikerAtStart;

            if (!dismissedId.HasValue)
                throw new InvalidOperationException("Dismissed batter is missing.");

            if (dismissedId.Value != strikerAtStart &&
                dismissedId.Value != nonStrikerAtStart)
                throw new InvalidOperationException("Dismissed batter is not one of the current batters.");

            if (wicketType == "CAUGHT")
            {
                if (!dto.CaughtByMatchPlayerId.HasValue ||
                    !IsFieldingTeamPlayer(
                        innings,
                        dto.CaughtByMatchPlayerId.Value))
                {
                    throw new InvalidOperationException(
                        "Caught by player is missing or is not a fielding-team player.");
                }
            }

            if (wicketType == "STUMPED")
            {
                if (!dto.StumpedByMatchPlayerId.HasValue ||
                    !IsFieldingTeamPlayer(
                        innings,
                        dto.StumpedByMatchPlayerId.Value))
                {
                    throw new InvalidOperationException(
                        "Stumped by player is missing or is not a fielding-team player.");
                }
            }

            if (wicketType != "RUN OUT" && dto.RunsCompleted != 0)
                throw new InvalidOperationException("Runs completed must be 0 for this wicket type.");

            if (dto.RunsCompleted < 0)
                throw new InvalidOperationException("Runs completed cannot be negative.");

            var completedRuns =
                wicketType == "RUN OUT"
                    ? dto.RunsCompleted
                    : 0;

            var dismissedWasStriker =
                dismissedId.Value == strikerAtStart;

            // Validate the incoming batter BEFORE changing innings state.
            var maximumWickets =
                Math.Max(1, innings.Match.PlayersPerTeam - 1);

            var inningsWillEnd =
                innings.Wickets + 1 >= maximumWickets ||
                innings.LegalBalls + 1 >= innings.Match.Overs * 6;

            if (!inningsWillEnd && innings.InningsNumber == 2)
            {
                var firstInnings =
                    innings.Match.Innings.FirstOrDefault(i => i.InningsNumber == 1);

                if (firstInnings != null &&
                    innings.TotalRuns + completedRuns > firstInnings.TotalRuns)
                {
                    inningsWillEnd = true;
                }
            }

            if (!inningsWillEnd)
            {
                if (!dto.NewBatterMatchPlayerId.HasValue)
                {
                    throw new InvalidOperationException(
                        "Incoming batter is required because the innings is not ending.");
                }

                if (!IsEligibleIncomingBatter(
                        innings,
                        dto.NewBatterMatchPlayerId.Value))
                {
                    throw new InvalidOperationException(
                        $"Incoming batter MatchPlayerId {dto.NewBatterMatchPlayerId.Value} is not eligible.");
                }
            }

            var ball = CreateBallSnapshot(
                innings,
                strikerAtStart,
                nonStrikerAtStart,
                bowler);

            ball.Runs = completedRuns;
            ball.BatterRuns = 0;
            ball.ExtraRuns = 0;
            ball.ExtraType = null;
            ball.IsLegalDelivery = true;
            ball.Notation =
                wicketType == "RUN OUT" && completedRuns > 0
                    ? $"{completedRuns}W"
                    : "W";

            var wicket = new Wicket
            {
                Ball = ball,
                DismissedMatchPlayerId = dismissedId.Value,
                WicketType = wicketType,
                CaughtByMatchPlayerId =
                    wicketType == "CAUGHT"
                        ? dto.CaughtByMatchPlayerId
                        : null,
                StumpedByMatchPlayerId =
                    wicketType == "STUMPED"
                        ? dto.StumpedByMatchPlayerId
                        : null,
                RunsCompleted = completedRuns,
                DismissedPlayerWasStriker = dismissedWasStriker,
                DidBattersCross =
                    wicketType == "RUN OUT" && dto.DidBattersCross
            };

            ball.Wicket = wicket;

            innings.TotalRuns += completedRuns;
            innings.LegalBalls++;
            innings.Wickets++;

            if (wicketType == "RUN OUT")
            {
                ApplyRunOutState(
                    innings,
                    strikerAtStart,
                    nonStrikerAtStart,
                    dismissedId.Value,
                    dto.DidBattersCross,
                    completedRuns);
            }
            else
            {
                // The dismissed batter's end is preserved until the
                // incoming batter is inserted.
                innings.StrikerMatchPlayerId = strikerAtStart;
                innings.NonStrikerMatchPlayerId = nonStrikerAtStart;
            }

            innings.IsFreeHit = false;

            if (inningsWillEnd)
            {
                innings.Status = "Completed";
                innings.CurrentBowlerMatchPlayerId = null;
            }
            else
            {
                ApplyIncomingBatter(
                    innings,
                    dismissedId.Value,
                    dto.NewBatterMatchPlayerId!.Value);
            }

            await FinalizeDeliveryAsync(
                innings,
                ball,
                wicket);

            return true;
        }

        // ====================================================================
        // SCORE EXTRAS
        // ====================================================================

        public async Task<bool> ScoreExtraAsync(
            int umpireId,
            ScoreExtraDto dto)
        {
            if (dto == null)
                return false;

            var innings =
                await _scoringRepository.GetInningsForScoringAsync(dto.InningsId);

            if (!IsValidLiveInnings(innings))
                return false;

            if (!await IsAuthorizedUmpire(innings!, umpireId))
                return false;

            if (!HasCurrentBowler(innings!))
                return false;

            var extraType = NormalizeExtraType(dto.ExtraType);

            if (!ValidExtraTypes.Contains(extraType))
                return false;

            if (dto.Runs < 0 ||
                dto.BatterRuns < 0 ||
                dto.RunsCompleted < 0)
            {
                return false;
            }

            // WIDE
            if (extraType == "WIDE")
            {
                if (dto.Runs < 1 ||
                    dto.BatterRuns != 0 ||
                    dto.Runs != dto.RunsCompleted + 1)
                {
                    return false;
                }

                if (dto.DismissedMatchPlayerId.HasValue)
                {
                    if (!IsCurrentBatter(
                            innings!,
                            dto.DismissedMatchPlayerId.Value))
                    {
                        return false;
                    }

                    return await ScoreExtraRunOutAsync(
                        innings,
                        dto,
                        extraType,
                        dto.Runs,
                        dto.RunsCompleted,
                        false);
                }

                return await ScoreExtraDeliveryAsync(
                    innings!,
                    extraType,
                    dto.Runs,
                    0,
                    dto.RunsCompleted,
                    false);
            }

            // NO BALL
            if (extraType == "NO BALL")
            {
                if (dto.Runs < 1)
                    return false;

                // A no-ball contributes one automatic run.
                // Any additional runs on the delivery are the completed runs.
                if (dto.Runs != dto.RunsCompleted + 1)
                    return false;

                // Batter runs are part of the completed runs.
                if (dto.BatterRuns > dto.RunsCompleted)
                    return false;

                if (dto.DismissedMatchPlayerId.HasValue)
                {
                    if (!IsCurrentBatter(
                            innings!,
                            dto.DismissedMatchPlayerId.Value))
                    {
                        return false;
                    }

                    return await ScoreNoBallRunOutAsync(
                        innings,
                        dto,
                        dto.Runs,
                        dto.BatterRuns,
                        dto.RunsCompleted);
                }

                return await ScoreExtraDeliveryAsync(
                    innings!,
                    extraType,
                    dto.Runs,
                    dto.BatterRuns,
                    dto.RunsCompleted,
                    true);
            }

            // BYE / LEG BYE
            if (extraType == "BYE" ||
                extraType == "LEG BYE")
            {
                if (dto.BatterRuns != 0 ||
                    dto.Runs != dto.RunsCompleted)
                {
                    return false;
                }

                if (dto.DismissedMatchPlayerId.HasValue)
                {
                    if (!IsCurrentBatter(
                            innings!,
                            dto.DismissedMatchPlayerId.Value))
                    {
                        return false;
                    }

                    return await ScoreExtraRunOutAsync(
                        innings,
                        dto,
                        extraType,
                        dto.Runs,
                        dto.RunsCompleted,
                        true);
                }

                if (dto.Runs == 0)
                    return false;

                return await ScoreExtraDeliveryAsync(
                    innings!,
                    extraType,
                    dto.Runs,
                    0,
                    dto.RunsCompleted,
                    false);
            }

            return false;
        }



        // ====================================================================
        // CHANGE BOWLER
        // ====================================================================

        public async Task<bool> ChangeBowlerAsync(
            int umpireId,
            int inningsId,
            int newBowlerMatchPlayerId)
        {
            var innings =
                await _scoringRepository.GetInningsForScoringAsync(inningsId);

            if (innings == null)
                return false;

            if (!await IsAuthorizedUmpire(innings, umpireId))
                return false;

            if (!IsValidLiveInnings(innings))
                return false;

            // A new bowler can only be selected after the previous
            // over has been completed.
            if (innings.CurrentBowlerMatchPlayerId.HasValue)
                return false;

            // The selected player must belong to the bowling team.
            if (!IsTeamPlayer(
                    innings.Match,
                    newBowlerMatchPlayerId,
                    innings.BowlingTeam))
            {
                return false;
            }

            // A bowler cannot bowl consecutive overs.
            var previousBall =
                innings.Balls
                    .OrderByDescending(b => b.Id)
                    .FirstOrDefault();

            if (previousBall != null &&
                previousBall.BowlerMatchPlayerId == newBowlerMatchPlayerId)
            {
                return false;
            }

            innings.CurrentBowlerMatchPlayerId =
                newBowlerMatchPlayerId;

            innings.UpdatedAt = DateTime.UtcNow;
            innings.Match.UpdatedAt = DateTime.UtcNow;

            await _scoringRepository.SaveChangesAsync();

            return true;
        }


        // ====================================================================
        // UNDO LAST DELIVERY
        // ====================================================================

        public async Task<bool> UndoLastScoreAsync(
            int umpireId,
            UndoScoreDto dto)
        {
            if (dto == null)
                return false;

            var innings =
                await _scoringRepository.GetInningsForScoringAsync(
                    dto.InningsId);

            if (innings == null)
                return false;

            if (innings.Match.UmpireId != umpireId)
                return false;

            var lastBall = innings.Balls
                .OrderByDescending(b => b.Id)
                .FirstOrDefault();

            if (lastBall == null)
                return false;

            // Only the single most recent scoring action may be undone.
            // Once a ball has been superseded by another scoring action,
            // it is permanently locked and can never become undoable again.
            if (dto.BallId <= 0 ||
                lastBall.Id != dto.BallId ||
                    !lastBall.CanUndo)
            {
                return false;
            }

            // Restore the exact pre-delivery state.
            innings.StrikerMatchPlayerId =
                lastBall.PreviousStrikerMatchPlayerId;

            innings.NonStrikerMatchPlayerId =
                lastBall.PreviousNonStrikerMatchPlayerId;

            innings.CurrentBowlerMatchPlayerId =
                lastBall.PreviousBowlerMatchPlayerId;

            innings.TotalRuns =
                lastBall.PreviousTotalRuns;

            innings.Wickets =
                lastBall.PreviousWickets;

            innings.LegalBalls =
                lastBall.PreviousLegalBalls;

            innings.Status =
                lastBall.PreviousInningsStatus;

            innings.IsFreeHit =
                lastBall.PreviousFreeHit;

            // Restore match-level state too.
            if (Enum.TryParse<MatchStatus>(
                    lastBall.PreviousMatchStatus,
                    true,
                    out var previousStatus))
            {
                innings.Match.Status = previousStatus;
            }

            if (Enum.TryParse<MatchResult>(
                    lastBall.PreviousMatchResult,
                    true,
                    out var previousResult))
            {
                innings.Match.Result = previousResult;
            }

            innings.Match.CompletionDeadline =
                lastBall.PreviousCompletionDeadline;

            // Remove associated wicket first.
            if (lastBall.Wicket != null)
            {
                innings.Match.Innings
                    .SelectMany(i => i.Balls)
                    .ToList();

                await _scoringRepository.RemoveBallAsync(lastBall);
            }
            else
            {
                await _scoringRepository.RemoveBallAsync(lastBall);
            }

            await _scoringRepository.SaveChangesAsync();

            return true;
        }

        // ====================================================================
        // RECORD TOSS
        // ====================================================================

        public async Task<bool> RecordTossAsync(
            int umpireId,
            RecordTossDto dto)
        {
            if (dto == null)
                return false;

            var match =
                await _scoringRepository.GetMatchForTossAsync(dto.MatchId);

            if (match == null)
                return false;

            if (match.UmpireId != umpireId)
                return false;

            if (match.Status != MatchStatus.Scheduled &&
      match.Status != MatchStatus.Live)
            {
                return false;
            }

            if (match.Innings.Any())
                return false;

            if (string.IsNullOrWhiteSpace(dto.TossWinnerTeam))
                return false;

            if (string.IsNullOrWhiteSpace(dto.TossDecision))
                return false;

            if (!IsTeam(match, dto.TossWinnerTeam))
                return false;

            var decision =
     dto.TossDecision.Trim().ToUpperInvariant();

            if (decision != "BAT" &&
                decision != "BOWL" &&
                decision != "FIELD")
            {
                return false;
            }

            match.TossWinnerTeam =
                dto.TossWinnerTeam.Trim();

            match.TossDecision =
                decision == "FIELD"
                    ? "BOWL"
                    : decision;

            match.BattingFirstTeam =
                decision == "BAT"
                    ? dto.TossWinnerTeam.Trim()
                    : GetOtherTeam(match, dto.TossWinnerTeam);

            match.UpdatedAt = DateTime.UtcNow;

            await _scoringRepository.SaveChangesAsync();

            return true;
        }

        // ====================================================================
        // START INNINGS
        // ====================================================================

        public async Task<bool> StartInningsAsync(
    int umpireId,
    StartInningsDto dto)
        {
            if (dto == null)
                throw new InvalidOperationException("DTO is null.");

            var match =
                await _scoringRepository.GetMatchForInningsAsync(
                    dto.MatchId);

            if (match == null)
                throw new InvalidOperationException("Match not found.");

            if (match.UmpireId != umpireId)
                throw new UnauthorizedAccessException(
                    "Current user is not the match umpire.");

            if (match.Status != MatchStatus.Live)
                throw new InvalidOperationException(
                    $"Match status is '{match.Status}', expected 'Live'.");

            if (dto.InningsNumber != 1 &&
                dto.InningsNumber != 2)
            {
                throw new InvalidOperationException(
                    $"Invalid innings number: {dto.InningsNumber}.");
            }

            var existing =
                match.Innings.FirstOrDefault(i =>
                    i.InningsNumber == dto.InningsNumber);

            if (existing != null)
                throw new InvalidOperationException(
                    $"Innings {dto.InningsNumber} already exists.");

            var battingTeam =
                dto.InningsNumber == 1
                    ? match.BattingFirstTeam
                    : GetOtherTeam(
                        match,
                        match.BattingFirstTeam!);

            if (string.IsNullOrWhiteSpace(battingTeam))
                throw new InvalidOperationException(
                    "Batting first team has not been recorded.");

            var bowlingTeam =
                GetOtherTeam(match, battingTeam);

            if (dto.StrikerMatchPlayerId ==
                dto.NonStrikerMatchPlayerId)
            {
                throw new InvalidOperationException(
                    "Striker and non-striker cannot be the same player.");
            }

            if (!IsTeamPlayer(
                    match,
                    dto.StrikerMatchPlayerId,
                    battingTeam))
            {
                throw new InvalidOperationException(
                    $"Striker MatchPlayerId {dto.StrikerMatchPlayerId} does not belong to batting team '{battingTeam}'.");
            }

            if (!IsTeamPlayer(
                    match,
                    dto.NonStrikerMatchPlayerId,
                    battingTeam))
            {
                throw new InvalidOperationException(
                    $"Non-striker MatchPlayerId {dto.NonStrikerMatchPlayerId} does not belong to batting team '{battingTeam}'.");
            }

            if (!IsTeamPlayer(
                    match,
                    dto.BowlerMatchPlayerId,
                    bowlingTeam))
            {
                throw new InvalidOperationException(
                    $"Bowler MatchPlayerId {dto.BowlerMatchPlayerId} does not belong to bowling team '{bowlingTeam}'.");
            }

            if (dto.InningsNumber == 2)
            {
                var firstInnings =
                    match.Innings.FirstOrDefault(
                        i => i.InningsNumber == 1);

                if (firstInnings == null)
                    throw new InvalidOperationException(
                        "First innings does not exist.");

                if (firstInnings.Status != "Completed")
                    throw new InvalidOperationException(
                        $"First innings status is '{firstInnings.Status}', expected 'Completed'.");
            }

            var innings = new Innings
            {
                MatchId = match.Id,
                InningsNumber = dto.InningsNumber,
                BattingTeam = battingTeam,
                BowlingTeam = bowlingTeam,
                StrikerMatchPlayerId =
                    dto.StrikerMatchPlayerId,
                NonStrikerMatchPlayerId =
                    dto.NonStrikerMatchPlayerId,
                CurrentBowlerMatchPlayerId =
                    dto.BowlerMatchPlayerId,
                TotalRuns = 0,
                Wickets = 0,
                LegalBalls = 0,
                Status = "Live",
                IsFreeHit = false,
                CreatedAt = DateTime.UtcNow
            };

            match.Innings.Add(innings);
            match.UpdatedAt = DateTime.UtcNow;

            await _scoringRepository.SaveChangesAsync();

            return true;
        }

        // ====================================================================
        // COMPLETE MATCH
        // ====================================================================

        public async Task<bool> CompleteMatchAsync(
            int umpireId,
            int matchId)
        {
            var match =
                await _scoringRepository.GetMatchForStatisticsAsync(
                    matchId);

            if (match == null)
                return false;

            if (match.UmpireId != umpireId)
                return false;

            if (match.Status != MatchStatus.PendingCompletion)
                return false;

            match.Status = MatchStatus.Completed;
            match.CompletionDeadline = null;
            match.UpdatedAt = DateTime.UtcNow;

            await _scoringRepository.SaveChangesAsync();

            return true;
        }

        // ====================================================================
        // EXTRA DELIVERY
        // ====================================================================

        private async Task<bool> ScoreExtraDeliveryAsync(
            Innings innings,
            string extraType,
            int totalRuns,
            int batterRuns,
            int completedRuns,
            bool isNoBall)
        {
            if (totalRuns <= 0)
                return false;

            var striker = innings.StrikerMatchPlayerId;
            var nonStriker = innings.NonStrikerMatchPlayerId;
            var bowler = innings.CurrentBowlerMatchPlayerId ?? 0;

            var ball = CreateBallSnapshot(
                innings,
                striker,
                nonStriker,
                bowler);

            ball.Runs = totalRuns;
            ball.BatterRuns = batterRuns;
            ball.ExtraType = extraType;
            ball.ExtraRuns = totalRuns - batterRuns;
            ball.IsLegalDelivery =
                !isNoBall &&
                extraType != "WIDE";

            ball.Notation =
                BuildExtraNotation(
                    extraType,
                    totalRuns);

            innings.TotalRuns += totalRuns;

            if (ball.IsLegalDelivery)
                innings.LegalBalls++;

            RotateStrikeForRuns(
                innings,
                completedRuns);

            // A no-ball creates a free hit.
            // A wide does NOT consume an existing free hit.
            // Any legal delivery (bye/leg-bye) consumes it.
            if (isNoBall)
            {
                innings.IsFreeHit = true;
            }
            else if (ball.IsLegalDelivery)
            {
                innings.IsFreeHit = false;
            }

            await FinalizeDeliveryAsync(
                innings,
                ball);

            return true;
        }

        // ====================================================================
        // NO-BALL + RUN OUT
        // ====================================================================

        private async Task<bool> ScoreNoBallRunOutAsync(
            Innings innings,
            ScoreExtraDto dto,
            int totalRuns,
            int batterRuns,
            int completedRuns)
        {
            if (totalRuns < 1)
                return false;

            var dismissedId = dto.DismissedMatchPlayerId!.Value;
            var strikerAtStart = innings.StrikerMatchPlayerId;
            var nonStrikerAtStart = innings.NonStrikerMatchPlayerId;
            var bowler = innings.CurrentBowlerMatchPlayerId ?? 0;

            if (dismissedId != strikerAtStart &&
                dismissedId != nonStrikerAtStart)
            {
                return false;
            }

            var maximumWickets =
                Math.Max(1, innings.Match.PlayersPerTeam - 1);

            var inningsWillEnd =
                innings.Wickets + 1 >= maximumWickets;

            if (!inningsWillEnd && innings.InningsNumber == 2)
            {
                var firstInnings =
                    innings.Match.Innings.FirstOrDefault(
                        i => i.InningsNumber == 1);

                if (firstInnings != null &&
                    innings.TotalRuns + totalRuns > firstInnings.TotalRuns)
                {
                    inningsWillEnd = true;
                }
            }

            if (!inningsWillEnd)
            {
                if (!dto.NewBatterMatchPlayerId.HasValue ||
                    !IsEligibleIncomingBatter(
                        innings,
                        dto.NewBatterMatchPlayerId.Value))
                {
                    return false;
                }
            }

            var ball = CreateBallSnapshot(
                innings,
                strikerAtStart,
                nonStrikerAtStart,
                bowler);

            ball.Runs = totalRuns;
            ball.BatterRuns = batterRuns;
            ball.ExtraType = "NO BALL";
            ball.ExtraRuns = totalRuns - batterRuns;
            ball.IsLegalDelivery = false;
            ball.Notation =
                completedRuns > 0
                    ? $"NB{totalRuns}W"
                    : "NBW";

            var wicket = new Wicket
            {
                Ball = ball,
                DismissedMatchPlayerId = dismissedId,
                WicketType = "RUN OUT",
                RunsCompleted = completedRuns,
                DismissedPlayerWasStriker =
                    dismissedId == strikerAtStart,
                DidBattersCross = dto.DidBattersCross
            };

            ball.Wicket = wicket;

            innings.TotalRuns += totalRuns;
            innings.Wickets++;

            ApplyRunOutState(
                innings,
                strikerAtStart,
                nonStrikerAtStart,
                dismissedId,
                dto.DidBattersCross,
                completedRuns);

            // The no-ball itself creates a free hit for the next delivery.
            innings.IsFreeHit = true;

            if (inningsWillEnd)
            {
                innings.Status = "Completed";
                innings.CurrentBowlerMatchPlayerId = null;
            }
            else
            {
                ApplyIncomingBatter(
                    innings,
                    dismissedId,
                    dto.NewBatterMatchPlayerId!.Value);
            }

            await FinalizeDeliveryAsync(
                innings,
                ball,
                wicket);

            return true;
        }

        // ====================================================================
        // EXTRA + RUN OUT
        // ====================================================================

        private async Task<bool> ScoreExtraRunOutAsync(
            Innings innings,
            ScoreExtraDto dto,
            string extraType,
            int totalRuns,
            int completedRuns,
            bool isLegalDelivery)
        {
            if (totalRuns < 0 ||
                completedRuns < 0)
            {
                return false;
            }

            var dismissedId = dto.DismissedMatchPlayerId!.Value;
            var strikerAtStart = innings.StrikerMatchPlayerId;
            var nonStrikerAtStart = innings.NonStrikerMatchPlayerId;
            var bowler = innings.CurrentBowlerMatchPlayerId ?? 0;

            if (dismissedId != strikerAtStart &&
                dismissedId != nonStrikerAtStart)
            {
                return false;
            }

            var maximumWickets =
                Math.Max(1, innings.Match.PlayersPerTeam - 1);

            var inningsWillEnd =
                innings.Wickets + 1 >= maximumWickets;

            if (!inningsWillEnd &&
                isLegalDelivery &&
                innings.LegalBalls + 1 >= innings.Match.Overs * 6)
            {
                inningsWillEnd = true;
            }

            if (!inningsWillEnd && innings.InningsNumber == 2)
            {
                var firstInnings =
                    innings.Match.Innings.FirstOrDefault(
                        i => i.InningsNumber == 1);

                if (firstInnings != null &&
                    innings.TotalRuns + totalRuns > firstInnings.TotalRuns)
                {
                    inningsWillEnd = true;
                }
            }

            if (!inningsWillEnd)
            {
                if (!dto.NewBatterMatchPlayerId.HasValue ||
                    !IsEligibleIncomingBatter(
                        innings,
                        dto.NewBatterMatchPlayerId.Value))
                {
                    return false;
                }
            }

            var ball = CreateBallSnapshot(
                innings,
                strikerAtStart,
                nonStrikerAtStart,
                bowler);

            ball.Runs = totalRuns;
            ball.BatterRuns = 0;
            ball.ExtraType = extraType;
            ball.ExtraRuns = totalRuns;
            ball.IsLegalDelivery = isLegalDelivery;
            ball.Notation =
                $"{GetExtraNotationPrefix(extraType)}{totalRuns}W";

            var wicket = new Wicket
            {
                Ball = ball,
                DismissedMatchPlayerId = dismissedId,
                WicketType = "RUN OUT",
                RunsCompleted = completedRuns,
                DismissedPlayerWasStriker =
                    dismissedId == strikerAtStart,
                DidBattersCross = dto.DidBattersCross
            };

            ball.Wicket = wicket;

            innings.TotalRuns += totalRuns;
            innings.Wickets++;

            if (isLegalDelivery)
                innings.LegalBalls++;

            ApplyRunOutState(
                innings,
                strikerAtStart,
                nonStrikerAtStart,
                dismissedId,
                dto.DidBattersCross,
                completedRuns);

            if (isLegalDelivery)
                innings.IsFreeHit = false;

            if (inningsWillEnd)
            {
                innings.Status = "Completed";
                innings.CurrentBowlerMatchPlayerId = null;
            }
            else
            {
                ApplyIncomingBatter(
                    innings,
                    dismissedId,
                    dto.NewBatterMatchPlayerId!.Value);
            }

            await FinalizeDeliveryAsync(
                innings,
                ball,
                wicket);

            return true;
        }

        // ====================================================================
        // CREATE BALL SNAPSHOT
        // ====================================================================

        private Ball CreateBallSnapshot(
            Innings innings,
            int striker,
            int nonStriker,
            int bowler)
        {
            var ballNumber =
                innings.LegalBalls % 6 + 1;

            var overNumber =
                innings.LegalBalls / 6;

            return new Ball
            {
                InningsId = innings.Id,
                OverNumber = overNumber,
                BallNumber = ballNumber,

                StrikerMatchPlayerId = striker,
                NonStrikerMatchPlayerId = nonStriker,
                BowlerMatchPlayerId = bowler,

                PreviousStrikerMatchPlayerId =
                    innings.StrikerMatchPlayerId,

                PreviousNonStrikerMatchPlayerId =
                    innings.NonStrikerMatchPlayerId,

                PreviousBowlerMatchPlayerId =
                    innings.CurrentBowlerMatchPlayerId,

                PreviousTotalRuns =
                    innings.TotalRuns,

                PreviousWickets =
                    innings.Wickets,

                PreviousLegalBalls =
                    innings.LegalBalls,

                PreviousInningsStatus =
                    innings.Status,

                PreviousFreeHit =
                    innings.IsFreeHit,

                PreviousMatchStatus =
                    innings.Match.Status.ToString(),

                PreviousMatchResult =
                    innings.Match.Result.ToString(),

                PreviousCompletionDeadline =
                    innings.Match.CompletionDeadline,

                CreatedAt = DateTime.UtcNow
            };
        }

        // ====================================================================
        // FINALIZE DELIVERY
        // ====================================================================

        private async Task FinalizeDeliveryAsync(
            Innings innings,
            Ball ball,
            Wicket? wicket = null)
        {
            // ------------------------------------------------------------
            // Over completed.
            // ------------------------------------------------------------

            if (ball.IsLegalDelivery &&
                innings.LegalBalls > 0 &&
                innings.LegalBalls % 6 == 0)
            {
                RotateStrike(innings);

                innings.CurrentBowlerMatchPlayerId = null;
            }

            // ------------------------------------------------------------
            // Innings completion.
            // ------------------------------------------------------------

            if (innings.Status != "Completed")
            {
                if (innings.LegalBalls >=
                    innings.Match.Overs * 6)
                {
                    innings.Status = "Completed";
                    innings.CurrentBowlerMatchPlayerId = null;
                }

                if (innings.Wickets >=
                    Math.Max(
                        1,
                        innings.Match.PlayersPerTeam - 1))
                {
                    innings.Status = "Completed";
                    innings.CurrentBowlerMatchPlayerId = null;
                }

                if (innings.InningsNumber == 2)
                {
                    var firstInnings =
                        innings.Match.Innings
                            .FirstOrDefault(
                                i => i.InningsNumber == 1);

                    if (firstInnings != null &&
                        innings.TotalRuns > firstInnings.TotalRuns)
                    {
                        innings.Status = "Completed";
                        innings.CurrentBowlerMatchPlayerId = null;
                    }
                }
            }

            // ------------------------------------------------------------
            // First innings completion does not complete the match.
            // ------------------------------------------------------------

            if (innings.Status == "Completed" &&
                innings.InningsNumber == 2)
            {
                innings.Match.Result =
                    CalculateMatchResult(
                        innings.Match,
                        innings);

                innings.Match.Status =
                    MatchStatus.PendingCompletion;

                innings.Match.CompletionDeadline =
                    DateTime.UtcNow.AddSeconds(10);
            }

            // The new scoring action becomes the only undoable action.
            // Every previous ball is permanently locked.
            foreach (var previousBall in innings.Balls)
            {
                previousBall.CanUndo = false;
            }

            ball.CanUndo = true;

            innings.UpdatedAt = DateTime.UtcNow;
            innings.Match.UpdatedAt = DateTime.UtcNow;

            await _scoringRepository.AddBallAsync(ball);

            await _scoringRepository.SaveChangesAsync();
        }

        // ====================================================================
        // RUN OUT STATE
        // ====================================================================

        private static void ApplyRunOutState(
            Innings innings,
            int strikerAtStart,
            int nonStrikerAtStart,
            int dismissedId,
            bool didBattersCross,
            int completedRuns)
        {
            var striker = strikerAtStart;
            var nonStriker = nonStrikerAtStart;

            // Every completed run swaps the ends.
            if (completedRuns % 2 == 1)
            {
                (striker, nonStriker) =
                    (nonStriker, striker);
            }

            // The run in progress is counted only if the batters crossed.
            // Therefore crossing swaps the ends one additional time.
            if (didBattersCross)
            {
                (striker, nonStriker) =
                    (nonStriker, striker);
            }

            // Keep the dismissed batter at the end where the wicket occurred.
            // The incoming batter will replace this exact position.
            if (striker == dismissedId)
            {
                innings.StrikerMatchPlayerId = dismissedId;
                innings.NonStrikerMatchPlayerId = nonStriker;
            }
            else if (nonStriker == dismissedId)
            {
                innings.StrikerMatchPlayerId = striker;
                innings.NonStrikerMatchPlayerId = dismissedId;
            }
            else
            {
                // Defensive fallback; this should be unreachable because
                // the caller validates the dismissed batter.
                innings.StrikerMatchPlayerId = striker;
                innings.NonStrikerMatchPlayerId = nonStriker;
            }
        }

        // ====================================================================
        // INCOMING BATTER
        // ====================================================================

        private static void ApplyIncomingBatter(
            Innings innings,
            int dismissedId,
            int newBatterId)
        {
            if (innings.StrikerMatchPlayerId == dismissedId)
            {
                innings.StrikerMatchPlayerId = newBatterId;
                return;
            }

            if (innings.NonStrikerMatchPlayerId == dismissedId)
            {
                innings.NonStrikerMatchPlayerId = newBatterId;
            }
        }

        // ====================================================================
        // ELIGIBILITY
        // ====================================================================

        private static bool IsEligibleIncomingBatter(
            Innings innings,
            int matchPlayerId)
        {
            var match =
                innings.Match;

            var player =
                match.MatchPlayers
                    .FirstOrDefault(mp =>
                        mp.Id == matchPlayerId);

            if (player == null)
                return false;

            // MatchPlayer.Team stores the logical team code
            // ("Team1" / "Team2"), while Innings.BattingTeam
            // stores the actual team name. Convert the innings
            // team name to the same logical code before comparing.
            var battingTeamCode =
                innings.BattingTeam.Equals(
                    innings.Match.Team1Name,
                    StringComparison.OrdinalIgnoreCase)
                    ? "Team1"
                    : innings.BattingTeam.Equals(
                        innings.Match.Team2Name,
                        StringComparison.OrdinalIgnoreCase)
                        ? "Team2"
                        : string.Empty;

            if (string.IsNullOrEmpty(battingTeamCode) ||
                !player.Team.Equals(
                    battingTeamCode,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (player.Id ==
                innings.StrikerMatchPlayerId ||
                player.Id ==
                innings.NonStrikerMatchPlayerId)
            {
                return false;
            }

            var hasAlreadyBatted =
                innings.Balls.Any(b =>
                    b.StrikerMatchPlayerId == matchPlayerId ||
                    b.NonStrikerMatchPlayerId == matchPlayerId);

            return !hasAlreadyBatted;
        }

        private static bool IsFieldingTeamPlayer(
            Innings innings,
            int matchPlayerId)
        {
            var player =
                innings.Match.MatchPlayers
                    .FirstOrDefault(
                        mp => mp.Id == matchPlayerId);

            if (player == null)
                return false;

            // MatchPlayer.Team stores Team1 / Team2, while
            // Innings.BowlingTeam stores the actual team name.
            var bowlingTeamCode =
                innings.BowlingTeam.Equals(
                    innings.Match.Team1Name,
                    StringComparison.OrdinalIgnoreCase)
                    ? "Team1"
                    : innings.BowlingTeam.Equals(
                        innings.Match.Team2Name,
                        StringComparison.OrdinalIgnoreCase)
                        ? "Team2"
                        : string.Empty;

            return !string.IsNullOrEmpty(bowlingTeamCode) &&
                   player.Team.Equals(
                       bowlingTeamCode,
                       StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCurrentBatter(
            Innings innings,
            int matchPlayerId)
        {
            return matchPlayerId == innings.StrikerMatchPlayerId ||
                   matchPlayerId == innings.NonStrikerMatchPlayerId;
        }

        // ====================================================================
        // VALIDATION
        // ====================================================================

        private static bool IsValidLiveInnings(
            [NotNullWhen(true)] Innings? innings)
        {
            if (innings == null)
                return false;

            if (innings.Status != "Live")
                return false;

            if (innings.Match.Status != MatchStatus.Live)
                return false;

            return true;
        }

        private static bool HasCurrentBowler(
            Innings innings)
        {
            return innings.CurrentBowlerMatchPlayerId.HasValue;
        }

        private static async Task<bool> IsAuthorizedUmpire(
            Innings innings,
            int umpireId)
        {
            await Task.CompletedTask;

            return innings.Match.UmpireId == umpireId;
        }

        private static bool IsTeamPlayer(
     MatchEntity match,
     int matchPlayerId,
     string team)
        {
            var matchPlayer =
                match.MatchPlayers.FirstOrDefault(
                    mp => mp.Id == matchPlayerId);

            if (matchPlayer == null)
                return false;

            if (string.IsNullOrWhiteSpace(team))
                return false;

            var requestedTeam =
                team.Trim();

            var playerTeam =
                matchPlayer.Team?.Trim();

            if (string.IsNullOrWhiteSpace(playerTeam))
                return false;

            // Direct comparison.
            if (playerTeam.Equals(
                    requestedTeam,
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // MatchPlayer stores Team1 / Team2,
            // while the scoring logic uses the actual team name.
            if (playerTeam.Equals(
                    "Team1",
                    StringComparison.OrdinalIgnoreCase))
            {
                return match.Team1Name.Equals(
                    requestedTeam,
                    StringComparison.OrdinalIgnoreCase);
            }

            if (playerTeam.Equals(
                    "Team2",
                    StringComparison.OrdinalIgnoreCase))
            {
                return match.Team2Name.Equals(
                    requestedTeam,
                    StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool IsTeam(
            MatchEntity match,
            string team)
        {
            return team.Equals(
                       match.Team1Name,
                       StringComparison.OrdinalIgnoreCase)
                   ||
                   team.Equals(
                       match.Team2Name,
                       StringComparison.OrdinalIgnoreCase);
        }

        private static string GetOtherTeam(
            MatchEntity match,
            string team)
        {
            return team.Equals(
                       match.Team1Name,
                       StringComparison.OrdinalIgnoreCase)
                ? match.Team2Name
                : match.Team1Name;
        }

        // ====================================================================
        // STRIKE
        // ====================================================================

        private static void RotateStrikeForRuns(
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
            (
                innings.StrikerMatchPlayerId,
                innings.NonStrikerMatchPlayerId
            ) =
            (
                innings.NonStrikerMatchPlayerId,
                innings.StrikerMatchPlayerId
            );
        }

        // ====================================================================
        // NOTATION
        // ====================================================================

        private static string BuildExtraNotation(
            string extraType,
            int totalRuns)
        {
            return $"{GetExtraNotationPrefix(extraType)}{totalRuns}";
        }

        private static string GetExtraNotationPrefix(
            string extraType)
        {
            return extraType.ToUpperInvariant() switch
            {
                "WIDE" => "WD",
                "NO BALL" => "NB",
                "BYE" => "B",
                "LEG BYE" => "LB",
                _ => string.Empty
            };
        }

        // ====================================================================
        // NORMALIZATION
        // ====================================================================

        private static string NormalizeWicketType(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Trim()
                .Replace("_", " ")
                .ToUpperInvariant();
        }

        private static string NormalizeExtraType(
            string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value
                .Trim()
                .Replace("_", " ")
                .ToUpperInvariant();
        }

        // ====================================================================
        // RESULT
        // ====================================================================

        private static MatchResult CalculateMatchResult(
            MatchEntity match,
            Innings secondInnings)
        {
            var firstInnings =
                match.Innings.FirstOrDefault(
                    i => i.InningsNumber == 1);

            if (firstInnings == null)
                return MatchResult.None;

            if (secondInnings.TotalRuns >
                firstInnings.TotalRuns)
            {
                return secondInnings.BattingTeam.Equals(
                           match.Team1Name,
                           StringComparison.OrdinalIgnoreCase)
                    ? MatchResult.Team1Won
                    : MatchResult.Team2Won;
            }

            if (secondInnings.TotalRuns ==
                firstInnings.TotalRuns)
            {
                return MatchResult.Tie;
            }

            return firstInnings.BattingTeam.Equals(
                       match.Team1Name,
                       StringComparison.OrdinalIgnoreCase)
                ? MatchResult.Team1Won
                : MatchResult.Team2Won;
        }
    }
}