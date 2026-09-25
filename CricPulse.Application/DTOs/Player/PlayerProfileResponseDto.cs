namespace CricPulse.Application.DTOs.Player
{
    public class PlayerProfileResponseDto
    {
        public PlayerResponseDto Profile { get; set; } = null!;

        public PlayerStatisticsResponseDto Statistics { get; set; } = null!;
    }
}