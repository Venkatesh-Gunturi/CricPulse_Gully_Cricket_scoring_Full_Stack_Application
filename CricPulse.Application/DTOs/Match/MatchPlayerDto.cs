namespace CricPulse.Application.DTOs.Match;

public class MatchPlayerDto
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }

    public Guid UserId { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public string? ProfileImageUrl { get; set; }

    public string Team { get; set; } = string.Empty;

    public bool IsCaptain { get; set; }
}