namespace CricPulse.Application.DTOs.Match;

public class MatchDto
{
    public Guid Id { get; set; }

    public Guid CreatedByUserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string TeamAName { get; set; } = string.Empty;

    public string TeamBName { get; set; } = string.Empty;

    public DateTime ScheduledAt { get; set; }

    public string? LocationAddress { get; set; }

    public string? State { get; set; }

    public string? PinCode { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }

    public double? ActualStartLatitude { get; set; }

    public double? ActualStartLongitude { get; set; }

    public DateTime? ActualStartedAt { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? CancellationReasonType { get; set; }

    public string? CancellationReason { get; set; }

    public DateTime? CancelledAt { get; set; }

    public string? LogoUrl { get; set; }

    public DateTime? FinishedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}