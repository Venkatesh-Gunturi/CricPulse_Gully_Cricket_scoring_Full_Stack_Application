using CricPulse.Application.DTOs.Match;
using CricPulse.Application.Interfaces;
using CricPulse.Application.Interfaces.Match;
using CricPulse.Domain.Entities;
using CricPulse.Domain.Enums;

namespace CricPulse.Application.Services
{
    public class MatchScoringService : IMatchScoringService
    {
        private readonly IScoringRepository _scoringRepository;

        public MatchScoringService(IScoringRepository scoringRepository)
        {
            _scoringRepository = scoringRepository;
        }

        // Purpose:
        // Record runs scored from the bat and attribute those runs to the striker
        // so batting and bowling statistics can later be calculated accurately.
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

            var ball = new Ball
            {
                InningsId = innings.Id,
                OverNumber = innings.LegalBalls / 6,
                BallNumber = (innings.LegalBalls % 6) + 1,

                StrikerMatchPlayerId = strikerId,
                NonStrikerMatchPlayerId = nonStrikerId,
                BowlerMatchPlayerId =
                    innings.CurrentBowlerMatchPlayerId.Value,

                Runs = runs,

                // All runs recorded through ScoreRuns are runs scored from the bat.
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

            // End of over: strike changes automatically.
            if (innings.LegalBalls % 6 == 0)
            {
                var currentStriker = innings.StrikerMatchPlayerId;

                innings.StrikerMatchPlayerId =
                    innings.NonStrikerMatchPlayerId;

                innings.NonStrikerMatchPlayerId =
                    currentStriker;

                // The current bowler must be selected again for the next over.
                innings.CurrentBowlerMatchPlayerId = null;
            }

            await _scoringRepository.AddBallAsync(ball);
            await _scoringRepository.SaveChangesAsync();

            return true;
        }


        // Purpose:
        // Record a wicket delivery, automatically finish an innings when the maximum
        // wickets or overs are reached, attribute completed runs to the batter,
        // and determine the second-innings match result.
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

            // For normal wickets, the striker is dismissed.
            // Run Out may dismiss either batter, so the DTO must specify it.
            var dismissedPlayerId =
                dto.DismissedMatchPlayerId ?? strikerId;

            if (wicketType == "RUN OUT" &&
                dto.DismissedMatchPlayerId == null)
            {
                return false;
            }

            if (wicketType == "CAUGHT" &&
                dto.CaughtByMatchPlayerId == null)
            {
                return false;
            }

            // For an N-player team, N-1 wickets means the innings is all out.
            var maximumWickets =
                innings.Match.PlayersPerTeam - 1;

            var wicketWillEndInnings =
                innings.Wickets + 1 >= maximumWickets;

            // A new batter is required only when the innings will continue.
            if (!wicketWillEndInnings &&
                dto.NewBatterMatchPlayerId <= 0)
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

                BowlerMatchPlayerId =
                    innings.CurrentBowlerMatchPlayerId.Value,

                // Runs completed during a wicket delivery are runs scored
                // from the bat for the purposes of this scoring model.
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
                // The umpire decides who is striker after the new batter comes in.
                if (dto.NewBatterIsStriker)
                {
                    innings.StrikerMatchPlayerId =
                        dto.NewBatterMatchPlayerId;

                    innings.NonStrikerMatchPlayerId =
                        nonStrikerId;
                }
                else
                {
                    innings.StrikerMatchPlayerId =
                        strikerId;

                    innings.NonStrikerMatchPlayerId =
                        dto.NewBatterMatchPlayerId;
                }
            }

            // End of over: rotate strike and require a new bowler.
            if (innings.LegalBalls % 6 == 0)
            {
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

            var maximumLegalBalls =
                innings.Match.Overs * 6;

            var inningsHasEnded =
                innings.Wickets >= maximumWickets ||
                innings.LegalBalls >= maximumLegalBalls;

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
                            innings.TotalRuns > firstInnings.TotalRuns
                                ? MatchResult.Team2Won
                                : innings.TotalRuns < firstInnings.TotalRuns
                                    ? MatchResult.Team1Won
                                    : MatchResult.Tie;

                        innings.Match.Status =
                            MatchStatus.PendingCompletion;

                        innings.Match.CompletionDeadline =
                            DateTime.UtcNow.AddSeconds(10);
                    }
                }
            }

            await _scoringRepository.SaveChangesAsync();

            return true;
        }

        // Purpose:
        // Record an extra delivery while correctly separating batter runs from
        // extra runs so batting and bowling statistics remain accurate.
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

            // Byes and leg-byes can never contain batter runs.
            if ((extraType == "BYE" || extraType == "LEG BYE") &&
                dto.BatterRuns != 0)
            {
                return false;
            }

            int extraRuns;

            if (extraType == "WIDE")
            {
                // Every wide run is an extra and none belongs to the batter.
                if (dto.BatterRuns != 0)
                {
                    return false;
                }

                extraRuns = dto.Runs;
            }
            else if (extraType == "NO BALL")
            {
                // A no-ball always contributes at least one extra run.
                if (dto.Runs < dto.BatterRuns + 1)
                {
                    return false;
                }

                extraRuns = dto.Runs - dto.BatterRuns;

                // At least one run must be the no-ball penalty.
                if (extraRuns < 1)
                {
                    return false;
                }
            }
            else
            {
                // BYE / LEG BYE
                extraRuns = dto.Runs;
            }

            var strikerId = innings.StrikerMatchPlayerId;
            var nonStrikerId = innings.NonStrikerMatchPlayerId;

            bool isLegalDelivery =
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

                BowlerMatchPlayerId =
                    innings.CurrentBowlerMatchPlayerId.Value,

                // Total runs added to the team's score.
                Runs = dto.Runs,

                // Only runs actually scored from the bat belong to the batter.
                BatterRuns = dto.BatterRuns,

                IsLegalDelivery = isLegalDelivery,

                // Runs classified as extras.
                ExtraRuns = extraRuns,

                ExtraType = extraType,

                CreatedAt = DateTime.UtcNow
            };

            innings.TotalRuns += dto.Runs;

            if (isLegalDelivery)
            {
                innings.LegalBalls++;
            }

            // Odd total runs rotate the strike.
            if (dto.Runs % 2 != 0)
            {
                innings.StrikerMatchPlayerId = nonStrikerId;
                innings.NonStrikerMatchPlayerId = strikerId;
            }

            // End of over only applies to legal deliveries.
            if (isLegalDelivery &&
                innings.LegalBalls % 6 == 0)
            {
                var currentStriker =
                    innings.StrikerMatchPlayerId;

                innings.StrikerMatchPlayerId =
                    innings.NonStrikerMatchPlayerId;

                innings.NonStrikerMatchPlayerId =
                    currentStriker;

                // The bowler must be selected again for the next over.
                innings.CurrentBowlerMatchPlayerId = null;
            }

            await _scoringRepository.AddBallAsync(ball);
            await _scoringRepository.SaveChangesAsync();

            return true;
        }

        // Purpose:
        // Undo the immediately previous scoring action and restore the innings and match
        // to the exact state that existed before that delivery, including cancelling
        // a pending match completion when the final ball is undone.
        public async Task<bool> UndoLastScoreAsync(
            int umpireId,
            UndoScoreDto dto)
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

            var isLiveInnings = innings.Status == "Live";
            var isCompletedInnings = innings.Status == "Completed";

            // A completed innings can only be undone when its match is waiting
            // for final completion. This is the safety window after the last ball.
            if (!isLiveInnings &&
                !(isCompletedInnings &&
                  innings.Match.Status == MatchStatus.PendingCompletion))
            {
                return false;
            }

            var lastBall = innings.Balls
                .OrderByDescending(b => b.Id)
                .FirstOrDefault();

            // Only the latest scoring action can be undone.
            if (lastBall == null || lastBall.Id != dto.BallId)
            {
                return false;
            }

            // Restore the team score.
            innings.TotalRuns -= lastBall.Runs;

            // Wides and no-balls are not legal deliveries,
            // so they did not advance the over.
            if (lastBall.IsLegalDelivery)
            {
                innings.LegalBalls--;
            }

            // Restore the exact striker, non-striker and bowler
            // that were active before this delivery.
            innings.StrikerMatchPlayerId =
                lastBall.StrikerMatchPlayerId;

            innings.NonStrikerMatchPlayerId =
                lastBall.NonStrikerMatchPlayerId;

            innings.CurrentBowlerMatchPlayerId =
                lastBall.BowlerMatchPlayerId;

            // If the delivery contained a wicket, restore the wicket count.
            if (lastBall.Wicket != null)
            {
                innings.Wickets--;
            }

            // If this was the final delivery that caused the second innings
            // to finish the match, return the match to its live scoring state.
            if (innings.Match.Status == MatchStatus.PendingCompletion)
            {
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

            // Both teams must have exactly the configured number of players.
            var team1PlayerCount = match.MatchPlayers
                .Count(mp => mp.Team == match.Team1Name);

            var team2PlayerCount = match.MatchPlayers
                .Count(mp => mp.Team == match.Team2Name);

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

                // Both opening batters must belong to the batting-first team.
                if (striker.Team != battingTeam ||
                    nonStriker.Team != battingTeam)
                {
                    return false;
                }

                // Opening bowler must belong to the opposing team.
                if (bowler.Team != bowlingTeam)
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

                // Both opening batters must belong to the second innings batting team.
                if (striker.Team != battingTeam ||
                    nonStriker.Team != battingTeam)
                {
                    return false;
                }

                // Opening bowler must belong to the first innings batting team.
                if (bowler.Team != bowlingTeam)
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
        // Permanently complete a match after its second innings has reached a final result
        // and the umpire has confirmed completion or the completion grace period has expired.
        public async Task<bool> CompleteMatchAsync(
            int umpireId,
            int matchId)
        {
            var match = await _scoringRepository
                .GetMatchForTossAsync(matchId);

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

            // The umpire can confirm immediately, or the backend can complete
            // the match after the grace period has expired.
            if (DateTime.UtcNow < match.CompletionDeadline.Value)
            {
                // Manual confirmation is intentionally allowed during the
                // grace period, so the umpire does not have to wait 10 seconds.
            }

            match.Status = MatchStatus.Completed;
            match.CompletionDeadline = null;
            match.UpdatedAt = DateTime.UtcNow;

            await _scoringRepository.SaveChangesAsync();

            return true;
        }

    }
}