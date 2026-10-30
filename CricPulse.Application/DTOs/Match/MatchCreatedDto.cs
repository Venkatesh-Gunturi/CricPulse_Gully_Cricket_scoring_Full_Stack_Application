namespace CricPulse.Application.DTOs.Match;

public class MatchCreatedDto
{
    public Guid MatchId { get; set; }

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

    public int Overs { get; set; }

    public int PlayersPerTeam { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool TossAgreed { get; set; }
    public string? TossWinnerTeam { get; set; }
    public string? TossDecision { get; set; }

    public List<CreatedMatchPlayerDto> Players { get; set; } = new();
}

public class CreatedMatchPlayerDto
{
    public Guid MatchPlayerId { get; set; }

    public Guid UserId { get; set; }
public string PlayerName { get; set; } = string.Empty;
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }

    public string Team { get; set; } = string.Empty;

    public bool IsCaptain { get; set; }
}

public class PublicMatchDetailsDto
{
    public Guid MatchId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string TeamAName { get; set; } = string.Empty;
    public string? TeamALogo { get; set; }

    public string TeamBName { get; set; } = string.Empty;
    public string? TeamBLogo { get; set; }

    public DateTime ScheduledAt { get; set; }

    public string Venue { get; set; } = string.Empty;
    public string? LocationAddress { get; set; }
    public string? State { get; set; }

    public int Overs { get; set; }
    public int PlayersPerTeam { get; set; }

    public string Status { get; set; } = string.Empty;

    public bool TossAgreed { get; set; }
    public string? TossWinnerTeam { get; set; }
    public string? TossDecision { get; set; }

    public string? CancellationReasonType { get; set; }
    public string? CancellationReason { get; set; }

    public string? WinnerTeam { get; set; }
    public string? ResultText { get; set; }

    public List<PublicMatchPlayerDto> Players { get; set; } = new();
}

public class PublicMatchPlayerDto
{
    public string PlayerName { get; set; } = string.Empty;

    public string Team { get; set; } = string.Empty;

    public bool IsCaptain { get; set; }
}