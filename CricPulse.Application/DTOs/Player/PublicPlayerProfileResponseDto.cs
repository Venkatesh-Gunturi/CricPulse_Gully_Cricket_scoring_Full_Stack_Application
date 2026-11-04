namespace CricPulse.Application.DTOs.Player;

public class PublicPlayerProfileResponseDto
{
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? ProfileImageUrl { get; set; }

    public int? Age { get; set; }
    public string? Gender { get; set; }
    public string? State { get; set; }

    public string? BattingStyle { get; set; }
    public string? BowlingStyle { get; set; }
    public string? PlayerRole { get; set; }

    public PlayerStatisticsResponseDto Statistics { get; set; } = new();
}