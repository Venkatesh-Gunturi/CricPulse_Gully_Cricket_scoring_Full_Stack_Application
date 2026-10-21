namespace CricPulse.Application.DTOs.Match;

public class CancelMatchDto
{
    public string CancellationReasonType { get; set; } = string.Empty;

    public string? CancellationReason { get; set; }
}