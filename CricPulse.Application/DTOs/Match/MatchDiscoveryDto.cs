namespace CricPulse.Application.DTOs.Match;

public class MatchDiscoveryQueryDto
{
    public string? Status { get; set; }
    public string? State { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double RadiusKm { get; set; } = 30;
}

public class MatchDiscoveryDto
{
    public Guid MatchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string TeamAName { get; set; } = string.Empty;
    public string? TeamALogo { get; set; }
    public string TeamBName { get; set; } = string.Empty;
    public string? TeamBLogo { get; set; }
    public DateTime ScheduledAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int PlayersPerTeam { get; set; }
    public bool TossAgreed { get; set; }
    public string? TossWinnerTeam { get; set; }
    public string? TossDecision { get; set; }
    public string? CancellationReasonType { get; set; }

    public string? CancellationReason { get; set; }
    public int Overs { get; set; }

    public string? Venue { get; set; }
    public string? LocationAddress { get; set; }
    public string? State { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? MapsUrl { get; set; }
    public double? DistanceKm { get; set; }

    public string? WinnerTeam { get; set; }
    public string? ResultText { get; set; }

    public List<MatchInningsSummaryDto> Innings { get; set; } = new();
}

public class MatchInningsSummaryDto
{
    public int InningsNumber { get; set; }
    public string BattingTeam { get; set; } = string.Empty;
    public int TotalRuns { get; set; }
    public int Wickets { get; set; }
    public int LegalBalls { get; set; }
    public int TotalOvers { get; set; }
    public string Status { get; set; } = string.Empty;
}