using CricPulse.Application.DTOs.Match;
using CricPulse.Application.Interfaces;
using CricPulse.Application.Interfaces.Match;
using CricPulse.Domain.Entities;

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
    }
}