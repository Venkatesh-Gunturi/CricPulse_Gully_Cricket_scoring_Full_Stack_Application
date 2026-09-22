using CricPulse.Application.DTOs.Match;
using CricPulse.Application.Interfaces;
using CricPulse.Application.Interfaces.Match;
using CricPulse.Application.Interfaces.Player;
using CricPulse.Domain.Entities;
using CricPulse.Domain.Enums;
using MatchEntity = CricPulse.Domain.Entities.Match;
namespace CricPulse.Application.Services
{
    public class MatchScoringService : IMatchScoringService
    {
        private readonly IScoringRepository _scoringRepository;
        private readonly IPlayerStatisticsRepository _playerStatisticsRepository;

        // Purpose:
        // Initialize the scoring service with the repositories required for
        // match scoring and player statistics persistence.
        public MatchScoringService(
            IScoringRepository scoringRepository,
            IPlayerStatisticsRepository playerStatisticsRepository)
        {
            _scoringRepository = scoringRepository;
            _playerStatisticsRepository = playerStatisticsRepository;
        }


        // Purpose:
        // Determine the final match result from the two completed innings,
        // regardless of which team batted first.
        private MatchResult CalculateMatchResult(
            MatchEntity match,
            Innings secondInnings)
        {
            var firstInnings = match.Innings
                .FirstOrDefault(i => i.InningsNumber == 1);

            if (firstInnings == null)
            {
                return MatchResult.None;
            }

            if (secondInnings.TotalRuns > firstInnings.TotalRuns)
            {
                return secondInnings.BattingTeam == match.Team1Name
                    ? MatchResult.Team1Won
                    : MatchResult.Team2Won;
            }

            if (secondInnings.TotalRuns < firstInnings.TotalRuns)
            {
                return firstInnings.BattingTeam == match.Team1Name
                    ? MatchResult.Team1Won
                    : MatchResult.Team2Won;
            }

            return MatchResult.Tie;
        }

        // Purpose:
        // Record runs from the bat while validating the current batter and bowler
        // against the innings teams, then automatically complete the innings or match
        // when the configured overs or second-innings target is reached.
        public async Task<bool> ScoreRunsAsync(
            int umpireId,
            int inningsId,
            int runs)
        {
            if (runs != 0 && runs != 1 && runs != 2 &&
                runs != 3 && runs != 4 && runs != 6)
            {
                return false;
            }

            var innings = await _scoringRepository
                .GetInningsForScoringAsync(inningsId);

            if (innings == null)
            {
                return false;
            }

            if (innings.Match.UmpireId != umpireId)
            {
                return false;
            }

            if (innings.Status != "Live")
            {
                return false;
            }

            if (innings.CurrentBowlerMatchPlayerId == null)
            {
                return false;
            }

            var strikerId = innings.StrikerMatchPlayerId;
            var nonStrikerId = innings.NonStrikerMatchPlayerId;
            var bowlerId = innings.CurrentBowlerMatchPlayerId.Value;

            var striker = innings.Match.MatchPlayers
                .FirstOrDefault(mp => mp.Id == strikerId);

            var nonStriker = innings.Match.MatchPlayers
                .FirstOrDefault(mp => mp.Id == nonStrikerId);

            var bowler = innings.Match.MatchPlayers
                .FirstOrDefault(mp => mp.Id == bowlerId);

            // All active players must still belong to the match.
            if (striker == null ||
                nonStriker == null ||
                bowler == null)
            {
                return false;
            }

            // Both batters must belong to the current batting team.
            var battingTeamCode =
                innings.BattingTeam == innings.Match.Team1Name
                    ? "Team1"
                    : innings.BattingTeam == innings.Match.Team2Name
                        ? "Team2"
                        : string.Empty;

            var bowlingTeamCode =
                innings.BowlingTeam == innings.Match.Team1Name
                    ? "Team1"
                    : innings.BowlingTeam == innings.Match.Team2Name
                        ? "Team2"
                        : string.Empty;

            if (string.IsNullOrEmpty(battingTeamCode) ||
                string.IsNullOrEmpty(bowlingTeamCode))
            {
                return false;
            }

            if (striker.Team != battingTeamCode ||
                nonStriker.Team != battingTeamCode)
            {
                return false;
            }

            // The bowler must belong to the opposing team.
            if (bowler.Team != bowlingTeamCode)
            {
                return false;
            }

            // A player cannot occupy two active positions.
            if (strikerId == nonStrikerId ||
                strikerId == bowlerId ||
                nonStrikerId == bowlerId)
            {
                return false;
            }

            var ball = new Ball
            {
                InningsId = innings.Id,
                OverNumber = innings.LegalBalls / 6,
                BallNumber = (innings.LegalBalls % 6) + 1,

                StrikerMatchPlayerId = strikerId,
                NonStrikerMatchPlayerId = nonStrikerId,
                BowlerMatchPlayerId = bowlerId,

                Runs = runs,

                // ScoreRuns represents runs scored directly from the bat.
                BatterRuns = runs,

                IsLegalDelivery = true,
                ExtraRuns = 0,
                CreatedAt = DateTime.UtcNow
            };

            innings.TotalRuns += runs;
            innings.LegalBalls++;

            // Odd runs rotate the strike.
            if (runs == 1 || runs == 3)
            {
                innings.StrikerMatchPlayerId = nonStrikerId;
                innings.NonStrikerMatchPlayerId = strikerId;
            }

            var maximumLegalBalls =
                innings.Match.Overs * 6;

            // The chasing team wins immediately once it scores more
            // than the first innings total.
            var targetReached =
                innings.InningsNumber == 2 &&
                innings.Match.Innings
                    .Any(i =>
                        i.InningsNumber == 1 &&
                        innings.TotalRuns > i.TotalRuns);

            var inningsHasEnded =
                innings.Wickets >=
                    innings.Match.PlayersPerTeam - 1 ||
                innings.LegalBalls >= maximumLegalBalls ||
                targetReached;

            if (inningsHasEnded)
            {
                innings.Status = "Completed";
                innings.CurrentBowlerMatchPlayerId = null;

                if (innings.InningsNumber == 2)
                {
                    var firstInnings = innings.Match.Innings
                        .FirstOrDefault(i => i.InningsNumber == 1);

                    if (firstInnings != null)
                    {
                        innings.Match.Result =
                            CalculateMatchResult(
                                innings.Match,
                                innings);

                        innings.Match.Status =
                            MatchStatus.PendingCompletion;

                        innings.Match.CompletionDeadline =
                            DateTime.UtcNow.AddSeconds(10);

                        innings.Match.UpdatedAt =
                            DateTime.UtcNow;
                    }
                }
            }
            else if (innings.LegalBalls % 6 == 0)
            {
                // End of over: rotate strike and require a new bowler.
                var currentStriker =
                    innings.StrikerMatchPlayerId;

                innings.StrikerMatchPlayerId =
                    innings.NonStrikerMatchPlayerId;

                innings.NonStrikerMatchPlayerId =
                    currentStriker;

                innings.CurrentBowlerMatchPlayerId = null;
            }

            await _scoringRepository.AddBallAsync(ball);
            await _scoringRepository.SaveChangesAsync();

            return true;
        }


        // Purpose:
        // Record a wicket delivery while validating the dismissed player, replacement
        // batter, catcher, and wicket type against the current innings and match lineup.
        public async Task<bool> ScoreWicketAsync(
            int umpireId,
            ScoreWicketDto dto)
        {
            var innings = await _scoringRepository
                .GetInningsForScoringAsync(dto.InningsId);

            if (innings == null)
            {
                return false;
            }

            if (innings.Match.UmpireId != umpireId)
            {
                return false;
            }

            if (innings.Status != "Live")
            {
                return false;
            }

            if (innings.CurrentBowlerMatchPlayerId == null)
            {
                return false;
            }

            var wicketType = dto.WicketType.Trim().ToUpper();

            var validWicketTypes = new[]
            {
        "BOWLED",
        "CAUGHT",
        "RUN OUT",
        "LBW",
        "STUMPED",
        "HIT WICKET"
    };

            if (!validWicketTypes.Contains(wicketType))
            {
                return false;
            }

            if (dto.RunsCompleted < 0)
            {
                return false;
            }

            var strikerId = innings.StrikerMatchPlayerId;
            var nonStrikerId = innings.NonStrikerMatchPlayerId;
            var bowlerId = innings.CurrentBowlerMatchPlayerId.Value;

            var striker = innings.Match.MatchPlayers
                .FirstOrDefault(mp => mp.Id == strikerId);

            var nonStriker = innings.Match.MatchPlayers
                .FirstOrDefault(mp => mp.Id == nonStrikerId);

            var bowler = innings.Match.MatchPlayers
                .FirstOrDefault(mp => mp.Id == bowlerId);

            if (striker == null ||
                nonStriker == null ||
                bowler == null)
            {
                return false;
            }

            var dismissedPlayerId =
                dto.DismissedMatchPlayerId ?? strikerId;

            // RUN OUT can dismiss either active batter.
            // All other wicket types can only dismiss the striker.
            if (wicketType == "RUN OUT")
            {
                if (dismissedPlayerId != strikerId &&
                    dismissedPlayerId != nonStrikerId)
                {
                    return false;
                }

                if (dto.DismissedMatchPlayerId == null)
                {
                    return false;
                }
            }
            else
            {
                if (dismissedPlayerId != strikerId)
                {
                    return false;
                }
            }

            if (wicketType == "CAUGHT")
            {
                if (dto.CaughtByMatchPlayerId == null)
                {
                    return false;
                }

                var catcher = innings.Match.MatchPlayers
                    .FirstOrDefault(mp =>
                        mp.Id == dto.CaughtByMatchPlayerId.Value);

                if (catcher == null)
                {
                    return false;
                }

                if (catcher.Team != bowler.Team)
                {
                    return false;
                }

                if (catcher.Id == dismissedPlayerId)
                {
                    return false;
                }
            }

            var maximumWickets =
                innings.Match.PlayersPerTeam - 1;

            var wicketWillEndInnings =
                innings.Wickets + 1 >= maximumWickets;

            if (!wicketWillEndInnings)
            {
                if (dto.NewBatterMatchPlayerId <= 0)
                {
                    return false;
                }

                var newBatter = innings.Match.MatchPlayers
                    .FirstOrDefault(mp =>
                        mp.Id == dto.NewBatterMatchPlayerId);

                if (newBatter == null)
                {
                    return false;
                }

                if (newBatter.Team != striker.Team)
                {
                    return false;
                }

                if (newBatter.Id == strikerId ||
                    newBatter.Id == nonStrikerId)
                {
                    return false;
                }

                if (newBatter.Id == bowlerId)
                {
                    return false;
                }
            }

            if (strikerId == bowlerId ||
                nonStrikerId == bowlerId)
            {
                return false;
            }

            var ball = new Ball
            {
                InningsId = innings.Id,
                OverNumber = innings.LegalBalls / 6,
                BallNumber = (innings.LegalBalls % 6) + 1,

                StrikerMatchPlayerId = strikerId,
                NonStrikerMatchPlayerId = nonStrikerId,
                BowlerMatchPlayerId = bowlerId,

                Runs = dto.RunsCompleted,
                BatterRuns = dto.RunsCompleted,

                IsLegalDelivery = true,
                ExtraRuns = 0,
                CreatedAt = DateTime.UtcNow
            };

            var wicket = new Wicket
            {
                Ball = ball,
                DismissedMatchPlayerId = dismissedPlayerId,
                WicketType = wicketType,
                CaughtByMatchPlayerId = dto.CaughtByMatchPlayerId,
                RunsCompleted = dto.RunsCompleted
            };

            innings.TotalRuns += dto.RunsCompleted;
            innings.LegalBalls++;
            innings.Wickets++;

            if (!wicketWillEndInnings)
            {
                var newBatter =
                    innings.Match.MatchPlayers
                        .First(mp =>
                            mp.Id == dto.NewBatterMatchPlayerId);

                if (dto.NewBatterIsStriker)
                {
                    innings.StrikerMatchPlayerId =
                        newBatter.Id;

                    innings.NonStrikerMatchPlayerId =
                        dismissedPlayerId == nonStrikerId
                            ? strikerId
                            : nonStrikerId;
                }
                else
                {
                    innings.StrikerMatchPlayerId =
                        dismissedPlayerId == strikerId
                            ? nonStrikerId
                            : strikerId;

                    innings.NonStrikerMatchPlayerId =
                        newBatter.Id;
                }
            }

            var maximumLegalBalls =
                innings.Match.Overs * 6;

            var targetReached =
                innings.InningsNumber == 2 &&
                innings.Match.Innings
                    .Any(i =>
                        i.InningsNumber == 1 &&
                        innings.TotalRuns > i.TotalRuns);

            var inningsHasEnded =
                innings.Wickets >= maximumWickets ||
                innings.LegalBalls >= maximumLegalBalls ||
                targetReached;

            if (inningsHasEnded)
            {
                innings.Status = "Completed";
                innings.CurrentBowlerMatchPlayerId = null;

                if (innings.InningsNumber == 2)
                {
                    var firstInnings = innings.Match.Innings
                        .FirstOrDefault(i => i.InningsNumber == 1);

                    if (firstInnings != null)
                    {
                        innings.Match.Result =
                            CalculateMatchResult(
                                innings.Match,
                                innings);

                        innings.Match.Status =
                            MatchStatus.PendingCompletion;

                        innings.Match.CompletionDeadline =
                            DateTime.UtcNow.AddSeconds(10);

                        innings.Match.UpdatedAt =
                            DateTime.UtcNow;
                    }
                }
            }
            else if (innings.LegalBalls % 6 == 0)
            {
                // End of over: rotate strike and require a new bowler.
                var currentStriker =
                    innings.StrikerMatchPlayerId;

                innings.StrikerMatchPlayerId =
                    innings.NonStrikerMatchPlayerId;

                innings.NonStrikerMatchPlayerId =
                    currentStriker;

                innings.CurrentBowlerMatchPlayerId = null;
            }

            await _scoringRepository.AddBallAsync(ball);
            await _scoringRepository.AddWicketAsync(wicket);
            await _scoringRepository.SaveChangesAsync();

            return true;
        }

        // Purpose:
        // Record an extra delivery while validating the active players against the
        // innings teams and automatically completing the innings or match when required.
       
        public async Task<bool> ScoreExtraAsync(
    int umpireId,
    ScoreExtraDto dto)
        {
            var innings = await _scoringRepository
                .GetInningsForScoringAsync(dto.InningsId);

            if (innings == null)
            {
                return false;
            }

            if (innings.Match.UmpireId != umpireId)
            {
                return false;
            }

            if (innings.Status != "Live")
            {
                return false;
            }

            if (innings.CurrentBowlerMatchPlayerId == null)
            {
                return false;
            }

            var extraType = dto.ExtraType.Trim().ToUpper();

            var validExtraTypes = new[]
            {
        "WIDE",
        "NO BALL",
        "BYE",
        "LEG BYE"
    };

            if (!validExtraTypes.Contains(extraType))
            {
                return false;
            }

            if (dto.Runs <= 0)
            {
                return false;
            }

            if (dto.BatterRuns < 0)
            {
                return false;
            }

            // Byes and leg-byes cannot contain batter runs.
            if ((extraType == "BYE" || extraType == "LEG BYE") &&
                dto.BatterRuns != 0)
            {
                return false;
            }

            int extraRuns;

            if (extraType == "WIDE")
            {
                // All wide runs are extras.
                if (dto.BatterRuns != 0)
                {
                    return false;
                }

                extraRuns = dto.Runs;
            }
            else if (extraType == "NO BALL")
            {
                // A no-ball must contain at least one extra run
                // for the no-ball penalty.
                if (dto.Runs < dto.BatterRuns + 1)
                {
                    return false;
                }

                extraRuns = dto.Runs - dto.BatterRuns;

                if (extraRuns < 1)
                {
                    return false;
                }
            }
            else if (extraType == "BYE" ||
                     extraType == "LEG BYE")
            {
                // All bye / leg-bye runs are extras.
                extraRuns = dto.Runs;
            }
            else
            {
                return false;
            }

            var strikerId = innings.StrikerMatchPlayerId;
            var nonStrikerId = innings.NonStrikerMatchPlayerId;
            var bowlerId = innings.CurrentBowlerMatchPlayerId.Value;

            var striker = innings.Match.MatchPlayers
                .FirstOrDefault(mp => mp.Id == strikerId);

            var nonStriker = innings.Match.MatchPlayers
                .FirstOrDefault(mp => mp.Id == nonStrikerId);

            var bowler = innings.Match.MatchPlayers
                .FirstOrDefault(mp => mp.Id == bowlerId);

            // All active players must belong to the match.
            if (striker == null ||
                nonStriker == null ||
                bowler == null)
            {
                return false;
            }

            // Convert the actual innings team names back to the
            // logical MatchPlayer team values.
            var battingTeamCode =
                innings.BattingTeam == innings.Match.Team1Name
                    ? "Team1"
                    : innings.BattingTeam == innings.Match.Team2Name
                        ? "Team2"
                        : string.Empty;

            var bowlingTeamCode =
                innings.BowlingTeam == innings.Match.Team1Name
                    ? "Team1"
                    : innings.BowlingTeam == innings.Match.Team2Name
                        ? "Team2"
                        : string.Empty;

            if (string.IsNullOrEmpty(battingTeamCode) ||
                string.IsNullOrEmpty(bowlingTeamCode))
            {
                return false;
            }

            // Both active batters must belong to the batting team.
            if (striker.Team != battingTeamCode ||
                nonStriker.Team != battingTeamCode)
            {
                return false;
            }

            // The active bowler must belong to the bowling team.
            if (bowler.Team != bowlingTeamCode)
            {
                return false;
            }

            // A player cannot occupy two active positions.
            if (strikerId == nonStrikerId ||
                strikerId == bowlerId ||
                nonStrikerId == bowlerId)
            {
                return false;
            }

            var isLegalDelivery =
                extraType == "BYE" ||
                extraType == "LEG BYE";

            var ball = new Ball
            {
                InningsId = innings.Id,

                OverNumber = innings.LegalBalls / 6,

                BallNumber = isLegalDelivery
                    ? (innings.LegalBalls % 6) + 1
                    : innings.LegalBalls + 1,

                StrikerMatchPlayerId = strikerId,
                NonStrikerMatchPlayerId = nonStrikerId,
                BowlerMatchPlayerId = bowlerId,

                Runs = dto.Runs,

                // Only the portion scored from the bat belongs to the batter.
                BatterRuns = dto.BatterRuns,

                IsLegalDelivery = isLegalDelivery,
                ExtraRuns = extraRuns,
                ExtraType = extraType,

                CreatedAt = DateTime.UtcNow
            };

            innings.TotalRuns += dto.Runs;

            if (isLegalDelivery)
            {
                innings.LegalBalls++;
            }

            // Determine the number of completed runs that caused
            // the batters to change ends.
            int completedRuns;

            if (extraType == "WIDE")
            {
                // The first run is the automatic wide penalty.
                // Only additional runs represent completed runs.
                completedRuns = dto.Runs - 1;
            }
            else if (extraType == "NO BALL")
            {
                // The first run is the mandatory no-ball extra.
                // Any additional runs represent completed runs,
                // whether they came from the bat or were taken as byes.
                completedRuns = dto.Runs - 1;
            }
            else if (extraType == "BYE" ||
                     extraType == "LEG BYE")
            {
                // Every bye / leg-bye run is a completed run.
                completedRuns = dto.Runs;
            }
            else
            {
                completedRuns = 0;
            }

            // Odd completed runs rotate the strike.
            if (completedRuns % 2 != 0)
            {
                innings.StrikerMatchPlayerId = nonStrikerId;
                innings.NonStrikerMatchPlayerId = strikerId;
            }

            var maximumLegalBalls =
                innings.Match.Overs * 6;

            // The chasing team wins immediately once it exceeds
            // the first innings score.
            var targetReached =
                innings.InningsNumber == 2 &&
                innings.Match.Innings
                    .Any(i =>
                        i.InningsNumber == 1 &&
                        innings.TotalRuns > i.TotalRuns);

            var inningsHasEnded =
                innings.LegalBalls >= maximumLegalBalls ||
                targetReached;

            if (inningsHasEnded)
            {
                innings.Status = "Completed";
                innings.CurrentBowlerMatchPlayerId = null;

                if (innings.InningsNumber == 2)
                {
                    var firstInnings = innings.Match.Innings
                        .FirstOrDefault(i => i.InningsNumber == 1);

                    if (firstInnings != null)
                    {
                        innings.Match.Result =
                            CalculateMatchResult(
                                innings.Match,
                                innings);

                        innings.Match.Status =
                            MatchStatus.PendingCompletion;

                        innings.Match.CompletionDeadline =
                            DateTime.UtcNow.AddSeconds(10);

                        innings.Match.UpdatedAt =
                            DateTime.UtcNow;
                    }
                }
            }
            else if (isLegalDelivery &&
                     innings.LegalBalls % 6 == 0)
            {
                // End of over: rotate strike and require a new bowler.
                var currentStriker =
                    innings.StrikerMatchPlayerId;

                innings.StrikerMatchPlayerId =
                    innings.NonStrikerMatchPlayerId;

                innings.NonStrikerMatchPlayerId =
                    currentStriker;

                innings.CurrentBowlerMatchPlayerId = null;
            }

            await _scoringRepository.AddBallAsync(ball);
            await _scoringRepository.SaveChangesAsync();

            return true;
        }



        // Purpose:
        // Undo the immediately previous scoring action and restore the innings and match
        // to the exact state that existed before that delivery, including reopening a
        // first innings that had just completed.
        public async Task<bool> UndoLastScoreAsync(
            int umpireId,
            UndoScoreDto dto)
        {
            var innings = await _scoringRepository
                .GetInningsForScoringAsync(dto.InningsId);

            if (innings == null)
                return false;

            if (innings.Match.UmpireId != umpireId)
                return false;

            var isLiveInnings = innings.Status == "Live";

            var isCompletedFirstInnings =
                innings.Status == "Completed" &&
                innings.InningsNumber == 1 &&
                innings.Match.Status == MatchStatus.Live;

            var isPendingCompletion =
                innings.Status == "Completed" &&
                innings.Match.Status == MatchStatus.PendingCompletion;

            if (!isLiveInnings &&
                !isCompletedFirstInnings &&
                !isPendingCompletion)
            {
                return false;
            }

            var lastBall = innings.Balls
                .OrderByDescending(b => b.Id)
                .FirstOrDefault();

            if (lastBall == null ||
                lastBall.Id != dto.BallId)
            {
                return false;
            }

            innings.TotalRuns -= lastBall.Runs;

            if (lastBall.IsLegalDelivery)
            {
                innings.LegalBalls--;
            }

            innings.StrikerMatchPlayerId =
                lastBall.StrikerMatchPlayerId;

            innings.NonStrikerMatchPlayerId =
                lastBall.NonStrikerMatchPlayerId;

            innings.CurrentBowlerMatchPlayerId =
                lastBall.BowlerMatchPlayerId;

            if (lastBall.Wicket != null)
            {
                innings.Wickets--;
            }

            if (isCompletedFirstInnings)
            {
                // The final first-innings ball was responsible for completing
                // the innings, so reopening it allows the umpire to continue scoring.
                innings.Status = "Live";
                innings.Match.Status = MatchStatus.Live;
                innings.Match.UpdatedAt = DateTime.UtcNow;
            }

            if (isPendingCompletion)
            {
                // The final second-innings ball created the pending result.
                // Undoing it returns the entire match to normal live scoring.
                innings.Status = "Live";
                innings.Match.Status = MatchStatus.Live;
                innings.Match.Result = MatchResult.None;
                innings.Match.CompletionDeadline = null;
                innings.Match.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                innings.UpdatedAt = DateTime.UtcNow;
            }

            await _scoringRepository.RemoveBallAsync(lastBall);

            await _scoringRepository.SaveChangesAsync();

            return true;
        }

        // Purpose:
        // Record the umpire's toss result and determine which team bats first.
        public async Task<bool> RecordTossAsync(
            int umpireId,
            RecordTossDto dto)
        {
            var match = await _scoringRepository
                .GetMatchForTossAsync(dto.MatchId);

            if (match == null)
            {
                return false;
            }

            if (match.UmpireId != umpireId)
            {
                return false;
            }

            // Toss can only be recorded after the umpire starts the match.
            if (match.Status != MatchStatus.Live)
            {
                return false;
            }

            // The toss can only be recorded once.
            if (!string.IsNullOrWhiteSpace(match.TossWinnerTeam))
            {
                return false;
            }

            if (match.Innings.Any())
            {
                return false;
            }

            var tossWinner = dto.TossWinnerTeam.Trim();
            var tossDecision = dto.TossDecision.Trim().ToUpper();

            if (tossWinner != match.Team1Name &&
                tossWinner != match.Team2Name)
            {
                return false;
            }

            if (tossDecision != "BAT" && tossDecision != "BOWL")
            {
                return false;
            }

            match.TossWinnerTeam = tossWinner;
            match.TossDecision = tossDecision;

            // If the toss winner chooses BAT, they bat first.
            // If they choose BOWL, the opposing team bats first.
            match.BattingFirstTeam = tossDecision == "BAT"
                ? tossWinner
                : tossWinner == match.Team1Name
                    ? match.Team2Name
                    : match.Team1Name;

            match.UpdatedAt = DateTime.UtcNow;

            await _scoringRepository.SaveChangesAsync();

            return true;
        }

        // Purpose:
        // Start either innings of a live match after validating the match state,
        // toss result, selected players, and the correct batting and bowling teams.
        public async Task<bool> StartInningsAsync(
            int umpireId,
            StartInningsDto dto)
        {
            var match = await _scoringRepository
                .GetMatchForInningsAsync(dto.MatchId);

            if (match == null)
            {
                return false;
            }

            if (match.UmpireId != umpireId)
            {
                return false;
            }

            // The match must already be live.
            if (match.Status != MatchStatus.Live)
            {
                return false;
            }

            // Toss must be completed before either innings can start.
            if (string.IsNullOrWhiteSpace(match.BattingFirstTeam))
            {
                return false;
            }

            // MatchPlayer stores the logical values "Team1" and "Team2",
            // while Match stores the actual team names entered by the umpire.
            var team1PlayerCount = match.MatchPlayers
                .Count(mp => mp.Team == "Team1");

            var team2PlayerCount = match.MatchPlayers
                .Count(mp => mp.Team == "Team2");

            // Both teams must have exactly the configured number of players.
            if (team1PlayerCount != match.PlayersPerTeam ||
                team2PlayerCount != match.PlayersPerTeam)
            {
                return false;
            }

            // Striker and non-striker must be different players.
            if (dto.StrikerMatchPlayerId == dto.NonStrikerMatchPlayerId)
            {
                return false;
            }

            if (dto.StrikerMatchPlayerId <= 0 ||
                dto.NonStrikerMatchPlayerId <= 0 ||
                dto.BowlerMatchPlayerId <= 0)
            {
                return false;
            }

            var striker = match.MatchPlayers
                .FirstOrDefault(mp => mp.Id == dto.StrikerMatchPlayerId);

            var nonStriker = match.MatchPlayers
                .FirstOrDefault(mp => mp.Id == dto.NonStrikerMatchPlayerId);

            var bowler = match.MatchPlayers
                .FirstOrDefault(mp => mp.Id == dto.BowlerMatchPlayerId);

            // All selected players must belong to this match.
            if (striker == null ||
                nonStriker == null ||
                bowler == null)
            {
                return false;
            }

            var firstInnings = match.Innings
                .FirstOrDefault(i => i.InningsNumber == 1);

            var secondInnings = match.Innings
                .FirstOrDefault(i => i.InningsNumber == 2);

            int inningsNumber;
            string battingTeam;
            string bowlingTeam;

            if (firstInnings == null)
            {
                // -----------------------------
                // FIRST INNINGS
                // -----------------------------

                inningsNumber = 1;

                battingTeam = match.BattingFirstTeam;

                bowlingTeam = battingTeam == match.Team1Name
                    ? match.Team2Name
                    : match.Team1Name;

                var battingTeamCode = battingTeam == match.Team1Name
                    ? "Team1"
                    : "Team2";

                var bowlingTeamCode = bowlingTeam == match.Team1Name
                    ? "Team1"
                    : "Team2";

                // Both opening batters must belong to the batting-first team.
                if (striker.Team != battingTeamCode ||
                    nonStriker.Team != battingTeamCode)
                {
                    return false;
                }

                // Opening bowler must belong to the opposing team.
                if (bowler.Team != bowlingTeamCode)
                {
                    return false;
                }
            }
            else
            {
                // -----------------------------
                // SECOND INNINGS
                // -----------------------------

                // Second innings can only begin after the first innings has ended.
                if (firstInnings.Status != "Completed")
                {
                    return false;
                }

                // Only one second innings is allowed.
                if (secondInnings != null)
                {
                    return false;
                }

                inningsNumber = 2;

                // The team that batted first now bowls.
                bowlingTeam = firstInnings.BattingTeam;

                // The other team now bats.
                battingTeam = firstInnings.BowlingTeam;

                var battingTeamCode = battingTeam == match.Team1Name
                    ? "Team1"
                    : "Team2";

                var bowlingTeamCode = bowlingTeam == match.Team1Name
                    ? "Team1"
                    : "Team2";

                // Both opening batters must belong to the second innings batting team.
                if (striker.Team != battingTeamCode ||
                    nonStriker.Team != battingTeamCode)
                {
                    return false;
                }

                // Opening bowler must belong to the first innings batting team.
                if (bowler.Team != bowlingTeamCode)
                {
                    return false;
                }
            }

            // A player cannot be both a batter and the opening bowler.
            if (striker.Id == bowler.Id ||
                nonStriker.Id == bowler.Id)
            {
                return false;
            }

            var innings = new Innings
            {
                MatchId = match.Id,
                InningsNumber = inningsNumber,
                BattingTeam = battingTeam,
                BowlingTeam = bowlingTeam,
                StrikerMatchPlayerId = striker.Id,
                NonStrikerMatchPlayerId = nonStriker.Id,
                CurrentBowlerMatchPlayerId = bowler.Id,
                TotalRuns = 0,
                Wickets = 0,
                LegalBalls = 0,
                Status = "Live",
                CreatedAt = DateTime.UtcNow
            };

            match.Innings.Add(innings);
            match.UpdatedAt = DateTime.UtcNow;

            await _scoringRepository.SaveChangesAsync();

            return true;
        }


        // Purpose:
        // Aggregate batting and bowling statistics for every player who participated
        // in the completed match before the match and its scoring records are removed.
        private async Task AggregatePlayerStatisticsAsync(
            MatchEntity match)
        {
            var completedInnings = match.Innings
                .OrderBy(i => i.InningsNumber)
                .ToList();

            foreach (var matchPlayer in match.MatchPlayers)
            {
                var playerId = matchPlayer.PlayerId;

                var statistics =
                    await _playerStatisticsRepository
                        .GetByPlayerIdAsync(playerId);

                if (statistics == null)
                {
                    statistics = new PlayerStatistics
                    {
                        PlayerId = playerId
                    };

                    await _playerStatisticsRepository
                        .CreateAsync(statistics);
                }

                // Every MatchPlayer represents participation in this match.
                statistics.Matches++;

                // ---------------------------------
                // BATTING
                // ---------------------------------

                foreach (var innings in completedInnings)
                {
                    var playerBalls = innings.Balls
                        .Where(b =>
                            b.StrikerMatchPlayerId == matchPlayer.Id)
                        .ToList();

                    if (playerBalls.Count == 0)
                    {
                        continue;
                    }

                    statistics.BattingInnings++;

                    var inningsRuns = playerBalls
                        .Sum(b => b.BatterRuns);

                    statistics.Runs += inningsRuns;

                    statistics.BallsFaced += playerBalls
                        .Count(b => b.IsLegalDelivery);

                    statistics.Fours += playerBalls
                        .Count(b => b.BatterRuns == 4);

                    statistics.Sixes += playerBalls
                        .Count(b => b.BatterRuns == 6);

                    if (inningsRuns >= 100)
                    {
                        statistics.Hundreds++;
                    }
                    else if (inningsRuns >= 50)
                    {
                        statistics.Fifties++;
                    }

                    if (inningsRuns > statistics.HighestScore)
                    {
                        statistics.HighestScore = inningsRuns;
                    }
                }

                // ---------------------------------
                // BOWLING
                // ---------------------------------

                foreach (var innings in completedInnings)
                {
                    var playerBowledBalls = innings.Balls
                        .Where(b =>
                            b.BowlerMatchPlayerId == matchPlayer.Id)
                        .ToList();

                    if (playerBowledBalls.Count == 0)
                    {
                        continue;
                    }

                    statistics.BowlingInnings++;

                    statistics.BallsBowled += playerBowledBalls
                        .Count(b => b.IsLegalDelivery);

                    foreach (var ball in playerBowledBalls)
                    {
                        // Byes and leg-byes are not charged to the bowler.
                        if (ball.ExtraType != "BYE" &&
                            ball.ExtraType != "LEG BYE")
                        {
                            statistics.RunsConceded += ball.Runs;
                        }

                        // Run outs are not credited as bowler wickets.
                        if (ball.Wicket != null &&
                            ball.Wicket.WicketType != "RUN OUT")
                        {
                            statistics.Wickets++;
                        }
                    }

                    // A maiden requires a complete six-legal-ball over
                    // with zero runs charged to the bowler.
                    var completedOvers = playerBowledBalls
                        .Where(b => b.IsLegalDelivery)
                        .GroupBy(b => b.OverNumber);

                    foreach (var over in completedOvers)
                    {
                        var legalBalls = over.Count();

                        if (legalBalls != 6)
                        {
                            continue;
                        }

                        var runsConcededInOver = over
                            .Where(b =>
                                b.ExtraType != "BYE" &&
                                b.ExtraType != "LEG BYE")
                            .Sum(b => b.Runs);

                        if (runsConcededInOver == 0)
                        {
                            statistics.MaidenOvers++;
                        }
                    }
                }

                if (statistics.Id > 0)
                {
                    await _playerStatisticsRepository
                        .UpdateAsync(statistics);
                }
            }
        }

        // Purpose:
        // Finalize a match by aggregating player statistics and permanently removing
        // the completed match and its scoring records.
        public async Task<bool> CompleteMatchAsync(
            int umpireId,
            int matchId)
        {
            var match = await _scoringRepository
                .GetMatchForStatisticsAsync(matchId);

            if (match == null)
            {
                return false;
            }

            if (match.UmpireId != umpireId)
            {
                return false;
            }

            if (match.Status != MatchStatus.PendingCompletion)
            {
                return false;
            }

            if (match.Result == MatchResult.None)
            {
                return false;
            }

            if (match.CompletionDeadline == null)
            {
                return false;
            }

            // Aggregate statistics while all match scoring data is still available.
            await AggregatePlayerStatisticsAsync(match);

            // The completed match is intentionally removed permanently after
            // statistics have been accumulated.
            await _scoringRepository.DeleteCompletedMatchAsync(matchId);

            await _scoringRepository.SaveChangesAsync();

            return true;
        }



    }
}