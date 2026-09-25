using CricPulse.Application.Interfaces.Match;
using CricPulse.Application.Interfaces.Player;
using CricPulse.Domain.Entities;
using MatchEntity = CricPulse.Domain.Entities.Match;
namespace CricPulse.Application.Services.Player
{
    public class PlayerStatisticsService : IPlayerStatisticsService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IPlayerStatisticsRepository _playerStatisticsRepository;

        public PlayerStatisticsService(
            IMatchRepository matchRepository,
            IPlayerStatisticsRepository playerStatisticsRepository)
        {
            _matchRepository = matchRepository;
            _playerStatisticsRepository = playerStatisticsRepository;
        }

        public async Task CalculateAndSaveMatchStatisticsAsync(int matchId)
        {
            var match = await _matchRepository.GetByIdAsync(matchId);

            if (match == null)
            {
                throw new InvalidOperationException(
                    "Match not found.");
            }

            if (match.Innings == null || !match.Innings.Any())
            {
                throw new InvalidOperationException(
                    "Match does not contain any innings.");
            }

            var matchPlayers = match.MatchPlayers
                .ToList();

            if (!matchPlayers.Any())
            {
                throw new InvalidOperationException(
                    "Match does not contain any players.");
            }

            var matchResults = new List<PlayerMatchStatistics>();

            foreach (var matchPlayer in matchPlayers)
            {
                var batting = CalculateBattingStatistics(
                    match,
                    matchPlayer);

                var bowling = CalculateBowlingStatistics(
                    match,
                    matchPlayer);

                var fielding = CalculateFieldingStatistics(
                    match,
                    matchPlayer);

                var mvpScore =
                    batting.MvpPoints +
                    bowling.MvpPoints +
                    fielding.MvpPoints;

                matchResults.Add(
                    new PlayerMatchStatistics
                    {
                        PlayerId = matchPlayer.PlayerId,

                        Runs = batting.Runs,
                        BallsFaced = batting.BallsFaced,
                        Fours = batting.Fours,
                        Sixes = batting.Sixes,
                        Fifties = batting.Fifties,
                        Hundreds = batting.Hundreds,
                        HighestScore = batting.Runs,
                        BattingInnings = batting.BattingInnings,

                        BallsBowled = bowling.BallsBowled,
                        RunsConceded = bowling.RunsConceded,
                        Wickets = bowling.Wickets,
                        MaidenOvers = bowling.MaidenOvers,
                        BowlingInnings = bowling.BowlingInnings,

                        MvpScore = mvpScore
                    });
            }

            var mvp = matchResults
                .OrderByDescending(x => x.MvpScore)
                .ThenByDescending(x => x.Runs)
                .ThenByDescending(x => x.Wickets)
                .FirstOrDefault();

            foreach (var result in matchResults)
            {
                await UpdateCareerStatisticsAsync(
                    result,
                    result.PlayerId == mvp?.PlayerId);
            }
        }

        private PlayerBattingMatchStatistics CalculateBattingStatistics(
            MatchEntity match,
            MatchPlayer matchPlayer)
        {
            var battingBalls = match.Innings
                .SelectMany(i => i.Balls)
                .Where(b =>
                    b.StrikerMatchPlayerId == matchPlayer.Id)
                .ToList();

            if (!battingBalls.Any())
            {
                return new PlayerBattingMatchStatistics();
            }

            var runs = battingBalls.Sum(b => b.BatterRuns);

            var ballsFaced = battingBalls.Count(
                b => b.IsLegalDelivery);

            var fours = battingBalls.Count(
                b => b.BatterRuns == 4);

            var sixes = battingBalls.Count(
                b => b.BatterRuns == 6);

            var fifties = runs >= 50 && runs < 100
                ? 1
                : 0;

            var hundreds = runs >= 100
                ? 1
                : 0;

            var mvpPoints = runs;

            mvpPoints += fours;
            mvpPoints += sixes * 2;

            if (runs >= 50)
            {
                mvpPoints += 10;
            }

            if (runs >= 100)
            {
                mvpPoints += 25;
            }

            return new PlayerBattingMatchStatistics
            {
                Runs = runs,
                BallsFaced = ballsFaced,
                Fours = fours,
                Sixes = sixes,
                Fifties = fifties,
                Hundreds = hundreds,
                BattingInnings = 1,
                MvpPoints = mvpPoints
            };
        }

        private PlayerBowlingMatchStatistics CalculateBowlingStatistics(
            MatchEntity match,
            MatchPlayer matchPlayer)
        {
            var bowlingBalls = match.Innings
                .SelectMany(i => i.Balls)
                .Where(b =>
                    b.BowlerMatchPlayerId == matchPlayer.Id)
                .ToList();

            if (!bowlingBalls.Any())
            {
                return new PlayerBowlingMatchStatistics();
            }

            var wickets = bowlingBalls
                .Where(b => b.Wicket != null)
                .Count(w =>
                    w.Wicket!.WicketType == "BOWLED" ||
                    w.Wicket!.WicketType == "CAUGHT" ||
                    w.Wicket!.WicketType == "LBW" ||
                    w.Wicket!.WicketType == "STUMPED" ||
                    w.Wicket!.WicketType == "HIT WICKET");

            var legalBalls = bowlingBalls
                .Count(b => b.IsLegalDelivery);

            var runsConceded = bowlingBalls
                .Where(b =>
                    b.ExtraType != "BYE" &&
                    b.ExtraType != "LEG BYE")
                .Sum(b => b.Runs);

            var maidenOvers = CalculateMaidenOvers(
                bowlingBalls);

            var mvpPoints = wickets * 20;

            mvpPoints += maidenOvers * 10;

            if (wickets >= 3)
            {
                mvpPoints += 10;
            }

            if (wickets >= 5)
            {
                mvpPoints += 25;
            }

            return new PlayerBowlingMatchStatistics
            {
                BallsBowled = legalBalls,
                RunsConceded = runsConceded,
                Wickets = wickets,
                MaidenOvers = maidenOvers,
                BowlingInnings = 1,
                MvpPoints = mvpPoints
            };
        }

        private int CalculateMaidenOvers(
            List<Ball> bowlingBalls)
        {
            var maidenOvers = 0;

            var overs = bowlingBalls
                .GroupBy(b => b.OverNumber);

            foreach (var over in overs)
            {
                var legalBalls = over.Count(
                    b => b.IsLegalDelivery);

                if (legalBalls != 6)
                {
                    continue;
                }

                var runsConceded = over
                    .Where(b =>
                        b.ExtraType != "BYE" &&
                        b.ExtraType != "LEG BYE")
                    .Sum(b => b.Runs);

                if (runsConceded == 0)
                {
                    maidenOvers++;
                }
            }

            return maidenOvers;
        }

        private PlayerFieldingMatchStatistics CalculateFieldingStatistics(
            MatchEntity match,
            MatchPlayer matchPlayer)
        {
            var wickets = match.Innings
                .SelectMany(i => i.Balls)
                .Where(b => b.Wicket != null)
                .Select(b => b.Wicket!)
                .ToList();

            var catches = wickets.Count(
                w => w.CaughtByMatchPlayerId == matchPlayer.Id);

            var stumpings = wickets.Count(
                w => w.StumpedByMatchPlayerId == matchPlayer.Id);

            var mvpPoints =
                (catches * 10) +
                (stumpings * 10);

            return new PlayerFieldingMatchStatistics
            {
                Catches = catches,
                Stumpings = stumpings,
                MvpPoints = mvpPoints
            };
        }

        private async Task UpdateCareerStatisticsAsync(
            PlayerMatchStatistics result,
            bool isMvp)
        {
            var statistics =
                await _playerStatisticsRepository
                    .GetByPlayerIdAsync(result.PlayerId);

            if (statistics == null)
            {
                statistics = new PlayerStatistics
                {
                    PlayerId = result.PlayerId
                };

                await _playerStatisticsRepository
                    .CreateAsync(statistics);
            }

            statistics.Matches++;

            statistics.BattingInnings +=
                result.BattingInnings;

            statistics.Runs +=
                result.Runs;

            statistics.BallsFaced +=
                result.BallsFaced;

            statistics.Fours +=
                result.Fours;

            statistics.Sixes +=
                result.Sixes;

            statistics.Fifties +=
                result.Fifties;

            statistics.Hundreds +=
                result.Hundreds;

            if (result.HighestScore >
                statistics.HighestScore)
            {
                statistics.HighestScore =
                    result.HighestScore;
            }

            statistics.BowlingInnings +=
                result.BowlingInnings;

            statistics.BallsBowled +=
                result.BallsBowled;

            statistics.RunsConceded +=
                result.RunsConceded;

            statistics.Wickets +=
                result.Wickets;

            statistics.MaidenOvers +=
                result.MaidenOvers;

            if (isMvp)
            {
                statistics.MVPCount++;
            }

            await _playerStatisticsRepository
                .UpdateAsync(statistics);
        }

        private class PlayerMatchStatistics
        {
            public int PlayerId { get; set; }

            public int Runs { get; set; }
            public int BallsFaced { get; set; }
            public int Fours { get; set; }
            public int Sixes { get; set; }
            public int Fifties { get; set; }
            public int Hundreds { get; set; }
            public int HighestScore { get; set; }
            public int BattingInnings { get; set; }

            public int BallsBowled { get; set; }
            public int RunsConceded { get; set; }
            public int Wickets { get; set; }
            public int MaidenOvers { get; set; }
            public int BowlingInnings { get; set; }

            public int MvpScore { get; set; }
        }

        private class PlayerBattingMatchStatistics
        {
            public int Runs { get; set; }
            public int BallsFaced { get; set; }
            public int Fours { get; set; }
            public int Sixes { get; set; }
            public int Fifties { get; set; }
            public int Hundreds { get; set; }
            public int BattingInnings { get; set; }
            public int MvpPoints { get; set; }
        }

        private class PlayerBowlingMatchStatistics
        {
            public int BallsBowled { get; set; }
            public int RunsConceded { get; set; }
            public int Wickets { get; set; }
            public int MaidenOvers { get; set; }
            public int BowlingInnings { get; set; }
            public int MvpPoints { get; set; }
        }

        private class PlayerFieldingMatchStatistics
        {
            public int Catches { get; set; }
            public int Stumpings { get; set; }
            public int MvpPoints { get; set; }
        }
    }
}