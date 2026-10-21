namespace CricPulse.Domain.Entities;

/// <summary>Match organization metrics keyed directly to the canonical User.Id.</summary>
public class OrganizerStatistics
{
    public Guid UserId { get; set; }
    public int MatchesCreated { get; set; }
    public int MatchesSuccessfullyOrganized { get; set; }
    public int MatchesFinished { get; set; }
    public int MatchesCancelled { get; set; }
    public int MatchesManaged { get; set; }
    public User User { get; set; } = null!;
}
