namespace CricPulse.Domain.Entities;

public class Match
{
    public Guid Id { get; set; }

    public Guid CreatedByUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string TeamAName { get; set; } = string.Empty;

    public string? TeamALogo { get; set; }

    public string TeamBName { get; set; } = string.Empty;

    public string? TeamBLogo { get; set; }

    public DateTime ScheduledAt { get; set; }

    public string Venue { get; set; } = string.Empty;

    public string? LocationAddress { get; set; }

    public string? State { get; set; }

    public string? PinCode { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double? ActualStartLatitude { get; set; }

    public double? ActualStartLongitude { get; set; }

    public DateTime? ActualStartedAt { get; set; }

    public int Overs { get; set; }

    public int PlayersPerTeam { get; set; }

    public string Status { get; set; } = "Upcoming";

    public string? TossWinnerTeam { get; set; }

    public string? TossDecision { get; set; }

    public bool TossAgreed { get; set; }

    public string? CancellationReasonType { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? FinishedAt { get; set; }

    public string? WinnerTeam { get; set; }
    public string? ResultText { get; set; }
    public bool PlayerStatisticsFinalized { get; set; }
    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public ICollection<MatchPlayer> MatchPlayers { get; set; } =
        new List<MatchPlayer>();

    public ICollection<Innings> Innings { get; set; } =
        new List<Innings>();
}