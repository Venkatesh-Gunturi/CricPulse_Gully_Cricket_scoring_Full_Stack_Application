
using CricPulse.Application.DTOs.Scoring;
using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Application.Interfaces.Services;
using CricPulse.Domain.Entities;
using MatchEntity = CricPulse.Domain.Entities.Match;

namespace CricPulse.Application.Services.Scoring;
/// <summary>
/// Builds the current match state, including innings scores,
/// player statistics, and live scoring information.
/// </summary>
public class MatchStateUpdateService : IMatchStateUpdateService
{
    private readonly IMatchRepository _matchRepository;
    private readonly IMatchUpdateNotifier _matchUpdateNotifier;
   
    public MatchStateUpdateService(
        IMatchRepository matchRepository,
        IMatchUpdateNotifier matchUpdateNotifier)
    {
        _matchRepository = matchRepository;
        _matchUpdateNotifier = matchUpdateNotifier;      
    }

    // Retrieves the latest match state and maps it into a response DTO.
    public async Task<MatchStateDto> GetCurrentMatchStateAsync(Guid matchId)
    {
        var match = await _matchRepository.GetByIdAsync(matchId);

        if (match is null)
        {
            throw new InvalidOperationException(
                "Match was not found.");
        }

        return MapState(match);
    }

    // Retrieves the latest match state and broadcasts it to connected clients.
    public async Task<MatchStateDto> GetAndBroadcastMatchStateAsync(Guid matchId)
    {
        var state = await GetCurrentMatchStateAsync(matchId);

        await _matchUpdateNotifier.NotifyMatchStateUpdatedAsync(
            matchId,
            state);

        return state;
    }

    // Maps match and innings entities into a complete match-state response,
    // including current player details and scorecard calculations.
    private static MatchStateDto MapState(MatchEntity match)
    {
        var players = match.MatchPlayers.ToList();

        string GetPlayerName(Guid? matchPlayerId)
        {
            if (!matchPlayerId.HasValue)
                return "—";
            // Resolve the match-player ID against the match roster.
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
            MVPUserId = match.MVPUserId,
            MVPPlayerName = match.MVPPlayerName,
            MVPScore = match.MVPScore,

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
                    // Calculate the remaining runs and legal balls needed to reach the target.
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
                    // Calculate current and required run rates using legal deliveries only.
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
                        StrikerBalls = strikerBalls.Count(b =>
                        b.ExtraType != "WIDE" &&
                        (b.IsLegalDelivery || b.ExtraType == "NO BALL")),
                        StrikerFours = strikerBalls.Count(b => b.BatterRuns == 4),
                        StrikerSixes = strikerBalls.Count(b => b.BatterRuns == 6),

                        NonStrikerRuns = nonStrikerBalls.Sum(b => b.BatterRuns),
                        NonStrikerBalls = nonStrikerBalls.Count(b =>
                            b.ExtraType != "WIDE" &&
                            (b.IsLegalDelivery || b.ExtraType == "NO BALL")),
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

}
