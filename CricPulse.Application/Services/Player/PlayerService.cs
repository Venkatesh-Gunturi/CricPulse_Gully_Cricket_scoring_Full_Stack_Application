using CricPulse.Application.DTOs.Player;
using CricPulse.Application.Interfaces.player;
using CricPulse.Application.Interfaces.Player;
using PlayerEntity = CricPulse.Domain.Entities.Player;

namespace CricPulse.Application.Services.Player
{
    public class PlayerService : IPlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly IPlayerStatisticsRepository _playerStatisticsRepository;

        public PlayerService(
            IPlayerRepository playerRepository,
            IPlayerStatisticsRepository playerStatisticsRepository)
        {
            _playerRepository = playerRepository;
            _playerStatisticsRepository = playerStatisticsRepository;
        }

        public async Task<PlayerResponseDto> CreateProfileAsync(
            int userId,
            CreatePlayerDto dto)
        {
            var existingPlayer =
                await _playerRepository.GetByUserIdAsync(userId);

            if (existingPlayer != null)
            {
                throw new InvalidOperationException(
                    "Player profile already exists.");
            }

            var player = new PlayerEntity
            {
                UserId = userId,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                BattingStyle = dto.BattingStyle,
                BowlingStyle = dto.BowlingStyle,
                PlayerRole = dto.PlayerRole,
                State = dto.State,
                PinCode = dto.PinCode
            };

            var createdPlayer =
                await _playerRepository.CreateAsync(player);

            return MapPlayerResponse(createdPlayer);
        }

        public async Task<PlayerProfileResponseDto?> GetPlayerProfileAsync(
            int userId)
        {
            var player =
                await _playerRepository.GetByUserIdAsync(userId);

            if (player == null)
            {
                return null;
            }

            var statistics =
                await _playerStatisticsRepository
                    .GetByPlayerIdAsync(player.Id);

            var statisticsResponse =
                MapStatisticsResponse(statistics);

            return new PlayerProfileResponseDto
            {
                Profile = MapPlayerResponse(player),
                Statistics = statisticsResponse
            };
        }

        public async Task<PlayerResponseDto> UpdateProfileAsync(
            int userId,
            CreatePlayerDto dto)
        {
            var player =
                await _playerRepository.GetByUserIdAsync(userId);

            if (player == null)
            {
                throw new InvalidOperationException(
                    "Player profile not found.");
            }

            player.DateOfBirth = dto.DateOfBirth;
            player.Gender = dto.Gender;
            player.BattingStyle = dto.BattingStyle;
            player.BowlingStyle = dto.BowlingStyle;
            player.PlayerRole = dto.PlayerRole;
            player.State = dto.State;
            player.PinCode = dto.PinCode;

            var updatedPlayer =
                await _playerRepository.UpdateAsync(player);

            return MapPlayerResponse(updatedPlayer);
        }

        private static PlayerResponseDto MapPlayerResponse(
            PlayerEntity player)
        {
            return new PlayerResponseDto
            {
                Id = player.Id,
                UserId = player.UserId,

                FirstName = player.User.FirstName,
                LastName = player.User.LastName,
                ProfileImageUrl = player.User.ProfileImageUrl,

                DateOfBirth = player.DateOfBirth,
                Gender = player.Gender,
                BattingStyle = player.BattingStyle,
                BowlingStyle = player.BowlingStyle,
                PlayerRole = player.PlayerRole,
                State = player.State,
                PinCode = player.PinCode
            };
        }

        private static PlayerStatisticsResponseDto MapStatisticsResponse(
            CricPulse.Domain.Entities.PlayerStatistics? statistics)
        {
            if (statistics == null)
            {
                return new PlayerStatisticsResponseDto();
            }

            var battingAverage = 0.0;

            /*
             * We don't currently store dismissals in PlayerStatistics.
             * Therefore batting average cannot be calculated accurately
             * from the existing career data.
             *
             * We leave it as 0 until dismissal tracking is persisted.
             */

            var strikeRate = 0.0;

            if (statistics.BallsFaced > 0)
            {
                strikeRate =
                    (double)statistics.Runs /
                    statistics.BallsFaced *
                    100;
            }

            var economy = 0.0;

            if (statistics.BallsBowled > 0)
            {
                economy =
                    (double)statistics.RunsConceded /
                    statistics.BallsBowled *
                    6;
            }

            return new PlayerStatisticsResponseDto
            {
                Matches = statistics.Matches,
                BattingInnings = statistics.BattingInnings,
                Runs = statistics.Runs,
                BallsFaced = statistics.BallsFaced,
                Fours = statistics.Fours,
                Sixes = statistics.Sixes,
                Fifties = statistics.Fifties,
                Hundreds = statistics.Hundreds,
                HighestScore = statistics.HighestScore,

                BattingAverage = battingAverage,
                StrikeRate = strikeRate,

                BowlingInnings = statistics.BowlingInnings,
                BallsBowled = statistics.BallsBowled,
                RunsConceded = statistics.RunsConceded,
                Wickets = statistics.Wickets,
                MaidenOvers = statistics.MaidenOvers,

                Economy = economy,

                MVPCount = statistics.MVPCount
            };
        }
    }
}