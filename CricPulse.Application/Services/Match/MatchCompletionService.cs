using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Application.Interfaces.Services;
using CricPulse.Domain.Entities;
using MatchEntity= CricPulse.Domain.Entities.Match;

namespace CricPulse.Application.Services.Match;

public class MatchCompletionService : IMatchCompletionService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IPlayerStatisticsRepository _playerStatisticsRepository;
    private readonly IOrganizerStatisticsRepository _organizerStatisticsRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly IUnitOfWork _unitOfWork;

    public MatchCompletionService(
        IMatchRepository matchRepository,
        IPlayerStatisticsRepository playerStatisticsRepository,
        IOrganizerStatisticsRepository organizerStatisticsRepository,
        ITransactionManager transactionManager,
        IUnitOfWork unitOfWork)
    {
        _matchRepository = matchRepository;
        _playerStatisticsRepository = playerStatisticsRepository;
        _organizerStatisticsRepository = organizerStatisticsRepository;
        _transactionManager = transactionManager;
        _unitOfWork = unitOfWork;
    }

    public async Task FinalizeCompletedMatchAsync(Guid matchId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);

        if (match is null)
            throw new InvalidOperationException("Match was not found.");

        if (match.Status != "Finished")
            throw new InvalidOperationException(
                "Only a finished match can finalize player statistics.");

        if (match.PlayerStatisticsFinalized)
            return;

        if (match.FinishedAt is null)
            throw new InvalidOperationException(
                "Finished match does not have a completion timestamp.");

        await _transactionManager.BeginTransactionAsync();

        try
        {
            /*
             * Re-read the match after transaction begins so all required
             * scoring information is available from the same DbContext.
             */
            match = await _matchRepository.GetByIdAsync(matchId);

            if (match is null)
                throw new InvalidOperationException("Match was not found.");

            if (match.Status != "Finished")
                throw new InvalidOperationException(
                    "Only a finished match can finalize player statistics.");

            if (match.PlayerStatisticsFinalized)
            {
                await _transactionManager.CommitTransactionAsync();
                return;
            }

            var playerMatchStatistics =
                BuildPlayerStatistics(match);

            foreach (var item in playerMatchStatistics)
            {
                var statistics =
                    await _playerStatisticsRepository
                        .GetByUserIdAsync(item.UserId);

                if (statistics is null)
                {
                    statistics = new PlayerStatistics
                    {
                        UserId = item.UserId
                    };

                    await _playerStatisticsRepository.AddAsync(statistics);
                }

                ApplyPlayerStatistics(
                    statistics,
                    item);
            }

            var organizerStatistics =
                await _organizerStatisticsRepository.GetByUserIdAsync(match.CreatedByUserId);
            var isNewOrganizerStatistics = organizerStatistics is null;
            organizerStatistics ??= new OrganizerStatistics { UserId = match.CreatedByUserId };

            organizerStatistics.MatchesFinished++;
            organizerStatistics.MatchesSuccessfullyOrganized++;

            if (isNewOrganizerStatistics)
                await _organizerStatisticsRepository.AddAsync(organizerStatistics);
            else
                await _organizerStatisticsRepository.UpdateAsync(organizerStatistics);

            match.PlayerStatisticsFinalized = true;
            match.UpdatedAt = DateTime.UtcNow;

            await _matchRepository.UpdateAsync(match);

            await _unitOfWork.SaveChangesAsync();

            await _transactionManager.CommitTransactionAsync();
        }
        catch
        {
            await _transactionManager.RollbackTransactionAsync();
            throw;
        }
    }

    private static List<PlayerMatchStatistics> BuildPlayerStatistics(
        MatchEntity match)
    {
        var result = match.MatchPlayers
            .ToDictionary(
                x => x.UserId,
                x => new PlayerMatchStatistics
                {
                    UserId = x.UserId
                });

        foreach (var innings in match.Innings)
        {
            var battingPlayers = innings.Balls
                .SelectMany(
                    ball => new[]
                    {
                        ball.StrikerMatchPlayerId,
                        ball.NonStrikerMatchPlayerId
                    })
                .Distinct()
                .ToList();

            /*
             * Include players who participated but faced zero balls.
             */
            if (innings.CurrentStrikerMatchPlayerId.HasValue)
            {
                battingPlayers.Add(
                    innings.CurrentStrikerMatchPlayerId.Value);
            }

            if (innings.CurrentNonStrikerMatchPlayerId.HasValue)
            {
                battingPlayers.Add(
                    innings.CurrentNonStrikerMatchPlayerId.Value);
            }

            foreach (var matchPlayerId in battingPlayers.Distinct())
            {
                var matchPlayer =
                    match.MatchPlayers.FirstOrDefault(
                        x => x.Id == matchPlayerId);

                if (matchPlayer is null)
                    continue;

                if (!result.TryGetValue(
                        matchPlayer.UserId,
                        out var playerStats))
                {
                    continue;
                }

                playerStats.Innings++;

                var playerBalls = innings.Balls
                    .Where(x =>
                        x.StrikerMatchPlayerId == matchPlayerId)
                    .ToList();

                playerStats.Runs += playerBalls.Sum(
                    x => x.BatterRuns);

                playerStats.BallsFaced += playerBalls.Count(
                    x => x.IsLegalDelivery);

                playerStats.Fours += playerBalls.Count(
                    x => x.BatterRuns == 4);

                playerStats.Sixes += playerBalls.Count(
                    x => x.BatterRuns == 6);

                var inningsScore = playerBalls.Sum(
                    x => x.BatterRuns);

                if (inningsScore > playerStats.HighestScore)
                {
                    playerStats.HighestScore = inningsScore;
                }

                if (inningsScore >= 50 &&
                    inningsScore < 100)
                {
                    playerStats.Fifties++;
                }

                if (inningsScore >= 100)
                {
                    playerStats.Hundreds++;
                }

                var dismissed = innings.Balls
                    .Where(x => x.Wicket is not null)
                    .Any(x =>
                        x.Wicket!.DismissedMatchPlayerId ==
                        matchPlayerId);

                if (!dismissed)
                {
                    playerStats.NotOuts++;
                }
            }

            /*
             * Bowling statistics.
             */
            var bowlingPlayers = innings.Balls
                .GroupBy(x => x.BowlerMatchPlayerId);

            foreach (var bowlingGroup in bowlingPlayers)
            {
                var matchPlayer =
                    match.MatchPlayers.FirstOrDefault(
                        x => x.Id == bowlingGroup.Key);

                if (matchPlayer is null)
                    continue;

                if (!result.TryGetValue(
                        matchPlayer.UserId,
                        out var playerStats))
                {
                    continue;
                }

                playerStats.BowlingInnings++;

                playerStats.BallsBowled += bowlingGroup.Count(
                    x => x.IsLegalDelivery);

                playerStats.RunsConceded += bowlingGroup.Sum(
                    GetBowlerRuns);

                var wicketsInInnings =
                    bowlingGroup
                        .Where(x => x.Wicket is not null)
                        .Count(x =>
                            x.Wicket!.WicketType != "RUN OUT");

                playerStats.Wickets += wicketsInInnings;

                var legalBalls =
                    bowlingGroup.Count(
                        x => x.IsLegalDelivery);

                var oversCompleted =
                    legalBalls / 6;

                if (oversCompleted > 0)
                {
                    var runsConceded =
                        bowlingGroup.Sum(GetBowlerRuns);

                    /*
                     * A maiden is a completed six-ball over
                     * with zero runs conceded.
                     *
                     * Because this is a local/gully cricket
                     * application, only completed six-ball overs
                     * are considered maidens.
                     */
                    for (var over = 1;
                         over <= innings.TotalOvers;
                         over++)
                    {
                        var overBalls = bowlingGroup
                            .Where(x =>
                                x.OverNumber == over)
                            .ToList();

                        if (overBalls.Count == 6 &&
                            overBalls.All(
                                x => x.IsLegalDelivery) &&
                            overBalls.Sum(GetBowlerRuns) == 0)
                        {
                            playerStats.Maidens++;
                        }
                    }
                }

                if (wicketsInInnings >= 3)
                {
                    playerStats.ThreeWicketHauls++;
                }

                if (wicketsInInnings >= 5)
                {
                    playerStats.FiveWicketHauls++;
                }

                var bestBowling =
                    $"{wicketsInInnings}/{bowlingGroup.Sum(GetBowlerRuns)}";

                if (IsBetterBowling(
                        bestBowling,
                        playerStats.BestBowling))
                {
                    playerStats.BestBowling = bestBowling;
                }
            }

            /*
             * Fielding statistics.
             */
            foreach (var ball in innings.Balls)
            {
                var wicket = ball.Wicket;

                if (wicket is null)
                    continue;

                if (wicket.CaughtByMatchPlayerId.HasValue)
                {
                    var catcher =
                        match.MatchPlayers.FirstOrDefault(
                            x =>
                                x.Id ==
                                wicket.CaughtByMatchPlayerId.Value);

                    if (catcher is not null &&
                        result.TryGetValue(
                            catcher.UserId,
                            out var catcherStats))
                    {
                        catcherStats.Catches++;
                    }
                }

                if (wicket.RunOutByMatchPlayerId.HasValue)
                {
                    var runOutPlayer =
                        match.MatchPlayers.FirstOrDefault(
                            x =>
                                x.Id ==
                                wicket.RunOutByMatchPlayerId.Value);

                    if (runOutPlayer is not null &&
                        result.TryGetValue(
                            runOutPlayer.UserId,
                            out var runOutStats))
                    {
                        runOutStats.RunOuts++;
                    }
                }

                if (wicket.StumpedByMatchPlayerId.HasValue)
                {
                    var keeper =
                        match.MatchPlayers.FirstOrDefault(
                            x =>
                                x.Id ==
                                wicket.StumpedByMatchPlayerId.Value);

                    if (keeper is not null &&
                        result.TryGetValue(
                            keeper.UserId,
                            out var keeperStats))
                    {
                        keeperStats.Stumpings++;
                    }
                }
            }
        }

        return result.Values.ToList();
    }

    private static int GetBowlerRuns(Ball ball)
    {
        /*
         * Byes and leg-byes are not charged to the bowler.
         * Wides and no-balls are.
         */
        if (ball.ExtraType is "BYE" or "LEG BYE")
            return 0;

        return ball.Runs;
    }

    private static bool IsBetterBowling(
        string candidate,
        string? existing)
    {
        if (existing is null)
            return true;

        var candidateParts = candidate.Split('/');
        var existingParts = existing.Split('/');

        if (candidateParts.Length != 2 ||
            existingParts.Length != 2)
        {
            return false;
        }

        if (!int.TryParse(candidateParts[0], out var candidateWickets) ||
            !int.TryParse(candidateParts[1], out var candidateRuns) ||
            !int.TryParse(existingParts[0], out var existingWickets) ||
            !int.TryParse(existingParts[1], out var existingRuns))
        {
            return false;
        }

        if (candidateWickets != existingWickets)
        {
            return candidateWickets > existingWickets;
        }

        return candidateRuns < existingRuns;
    }

    private static void ApplyPlayerStatistics(
        PlayerStatistics target,
        PlayerMatchStatistics source)
    {
        target.Matches += 1;
        target.Innings += source.Innings;
        target.Runs += source.Runs;
        target.BallsFaced += source.BallsFaced;
        target.Fours += source.Fours;
        target.Sixes += source.Sixes;
        target.Fifties += source.Fifties;
        target.Hundreds += source.Hundreds;

        if (source.HighestScore > target.HighestScore)
        {
            target.HighestScore = source.HighestScore;
        }

        target.NotOuts += source.NotOuts;

        target.BowlingInnings += source.BowlingInnings;
        target.BallsBowled += source.BallsBowled;
        target.RunsConceded += source.RunsConceded;
        target.Wickets += source.Wickets;
        target.Maidens += source.Maidens;
        target.ThreeWicketHauls += source.ThreeWicketHauls;
        target.FiveWicketHauls += source.FiveWicketHauls;

        target.Catches += source.Catches;
        target.RunOuts += source.RunOuts;
        target.Stumpings += source.Stumpings;

        if (IsBetterBowling(
                source.BestBowling ?? "0/0",
                target.BestBowling))
        {
            target.BestBowling =
                source.BestBowling;
        }
    }

    private sealed class PlayerMatchStatistics
    {
        public Guid UserId { get; init; }

        public int Innings { get; set; }
        public int Runs { get; set; }
        public int BallsFaced { get; set; }
        public int HighestScore { get; set; }
        public int NotOuts { get; set; }
        public int Fours { get; set; }
        public int Sixes { get; set; }
        public int Fifties { get; set; }
        public int Hundreds { get; set; }

        public int BowlingInnings { get; set; }
        public int BallsBowled { get; set; }
        public int RunsConceded { get; set; }
        public int Wickets { get; set; }
        public int Maidens { get; set; }
        public int ThreeWicketHauls { get; set; }
        public int FiveWicketHauls { get; set; }
        public string? BestBowling { get; set; }

        public int Catches { get; set; }
        public int RunOuts { get; set; }
        public int Stumpings { get; set; }
    }
}