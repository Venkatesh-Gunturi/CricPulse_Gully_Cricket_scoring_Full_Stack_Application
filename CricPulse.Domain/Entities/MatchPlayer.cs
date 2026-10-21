namespace CricPulse.Domain.Entities;

/// <summary>A user's participation record in a specific match.</summary>
public class MatchPlayer
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid UserId { get; set; }
    public string Team { get; set; } = string.Empty;
    public bool IsCaptain { get; set; }
    public DateTime CreatedAt { get; set; }
    public Match Match { get; set; } = null!;
    public User User { get; set; } = null!;
}
