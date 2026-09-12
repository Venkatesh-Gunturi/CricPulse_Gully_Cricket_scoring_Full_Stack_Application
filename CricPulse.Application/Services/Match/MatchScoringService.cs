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

        public async Task<bool> ScoreRunsAsync(int umpireId, int inningsId, int runs)
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
                BowlerMatchPlayerId = innings.CurrentBowlerMatchPlayerId.Value,
                Runs = runs,
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
                innings.StrikerMatchPlayerId = innings.NonStrikerMatchPlayerId;
                innings.NonStrikerMatchPlayerId = currentStriker;

                // The current bowler must be selected again for the next over.
                innings.CurrentBowlerMatchPlayerId = null;
            }

            await _scoringRepository.AddBallAsync(ball);
            await _scoringRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ScoreWicketAsync(
     int umpireId,
     ScoreWicketDto dto)
        {
            var innings = await _scoringRepository
                .GetInningsForScoringAsync(dto.InningsId);

            if (innings == null || innings.Status != "Live")
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

            // Save the state before changing striker/non-striker.
            var strikerId = innings.StrikerMatchPlayerId;
            var nonStrikerId = innings.NonStrikerMatchPlayerId;

            // For normal wickets, the striker is dismissed.
            // Run Out may dismiss either batter, so the DTO must specify it.
            var dismissedPlayerId =
                dto.DismissedMatchPlayerId ?? strikerId;

            if (wicketType == "CAUGHT" &&
                dto.CaughtByMatchPlayerId == null)
            {
                return false;
            }

            if (dto.NewBatterMatchPlayerId <= 0)
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
                BowlerMatchPlayerId = innings.CurrentBowlerMatchPlayerId.Value,
                Runs = dto.RunsCompleted,
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

            // The umpire decides who is striker after the new batter comes in.
            if (dto.NewBatterIsStriker)
            {
                innings.StrikerMatchPlayerId = dto.NewBatterMatchPlayerId;
                innings.NonStrikerMatchPlayerId = nonStrikerId;
            }
            else
            {
                innings.StrikerMatchPlayerId = strikerId;
                innings.NonStrikerMatchPlayerId = dto.NewBatterMatchPlayerId;
            }

            // End of over: rotate strike and require a new bowler.
            if (innings.LegalBalls % 6 == 0)
            {
                var currentStriker = innings.StrikerMatchPlayerId;

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

        public async Task<bool> ScoreExtraAsync(
    int umpireId,
    ScoreExtraDto dto)
        {
            var innings = await _scoringRepository
                .GetInningsForScoringAsync(dto.InningsId);

            if (innings == null || innings.Status != "Live")
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
                BowlerMatchPlayerId = innings.CurrentBowlerMatchPlayerId.Value,

                Runs = dto.Runs,
                IsLegalDelivery = isLegalDelivery,
                ExtraType = extraType,
                ExtraRuns = dto.Runs,
                CreatedAt = DateTime.UtcNow
            };

            innings.TotalRuns += dto.Runs;

            if (isLegalDelivery)
            {
                innings.LegalBalls++;
            }

            // Odd extra runs rotate the strike.
            if (dto.Runs % 2 != 0)
            {
                innings.StrikerMatchPlayerId = nonStrikerId;
                innings.NonStrikerMatchPlayerId = strikerId;
            }

            // End of over only applies to legal deliveries.
            if (isLegalDelivery && innings.LegalBalls % 6 == 0)
            {
                var currentStriker = innings.StrikerMatchPlayerId;

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
        // Undo only the immediately previous scoring action and restore the innings
        // state that existed before that delivery was recorded.
        public async Task<bool> UndoLastScoreAsync(
            int umpireId,
            UndoScoreDto dto)
        {
            var innings = await _scoringRepository
                .GetInningsForScoringAsync(dto.InningsId);

            if (innings == null || innings.Status != "Live")
            {
                return false;
            }

            var lastBall = innings.Balls
                .OrderByDescending(b => b.Id)
                .FirstOrDefault();

            // Only the latest scoring action can be undone.
            // This prevents rolling the match back multiple deliveries.
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
            innings.StrikerMatchPlayerId = lastBall.StrikerMatchPlayerId;
            innings.NonStrikerMatchPlayerId = lastBall.NonStrikerMatchPlayerId;
            innings.CurrentBowlerMatchPlayerId = lastBall.BowlerMatchPlayerId;

            // If the delivery contained a wicket, restore the wicket count.
            if (lastBall.Wicket != null)
            {
                innings.Wickets--;
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
        // Start the first innings after validating the match, toss, teams, and opening players.
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

            // The match must already have been started by the umpire.
            if (match.Status != MatchStatus.Live)
            {
                return false;
            }

            // Toss must be completed before innings setup.
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

            // Both opening batters must belong to the batting-first team.
            if (striker.Team != match.BattingFirstTeam ||
                nonStriker.Team != match.BattingFirstTeam)
            {
                return false;
            }

            var bowlingTeam = match.BattingFirstTeam == match.Team1Name
                ? match.Team2Name
                : match.Team1Name;

            // The opening bowler must belong to the bowling team.
            if (bowler.Team != bowlingTeam)
            {
                return false;
            }

            // A player cannot be both a batter and the opening bowler.
            if (striker.Team == bowler.Team ||
                nonStriker.Team == bowler.Team)
            {
                return false;
            }

            // Only one first innings can be started.
            if (match.Innings.Any(i => i.InningsNumber == 1))
            {
                return false;
            }

            var innings = new Innings
            {
                MatchId = match.Id,
                InningsNumber = 1,
                BattingTeam = match.BattingFirstTeam,
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
        // Start the match when the umpire chooses to begin, provided the
        // 24-hour grace period after the scheduled match time has not expired.
        // Purpose:
        // Start a scheduled match after validating the assigned umpire and 24-hour start window.
        public async Task<bool> StartMatchAsync(int umpireId, int matchId)
        {
            var match = await _scoringRepository
                .GetMatchForTossAsync(matchId);

            if (match == null)
            {
                return false;
            }

            // Only the assigned umpire can start the match.
            if (match.UmpireId != umpireId)
            {
                return false;
            }

            // A match can only be started once.
            if (match.Status != MatchStatus.Scheduled ||
                match.StartedAt != null)
            {
                return false;
            }

            var scheduledDateTime = match.MatchDate.Date
                .Add(match.MatchTime);

            var startDeadline = scheduledDateTime.AddHours(24);

            // The umpire can start before, at, or after the scheduled time,
            // but not after the 24-hour grace period.
            if (DateTime.UtcNow > startDeadline)
            {
                match.Status = MatchStatus.Cancelled;
                match.UpdatedAt = DateTime.UtcNow;
                match.CancellationReason = "Umpire unavailable";

                await _scoringRepository.SaveChangesAsync();

                return false;
            }

            // Starting the match makes it visible as Live immediately.
            match.Status = MatchStatus.Live;
            match.StartedAt = DateTime.UtcNow;
            match.UpdatedAt = DateTime.UtcNow;

            await _scoringRepository.SaveChangesAsync();

            return true;
        }
    }
}