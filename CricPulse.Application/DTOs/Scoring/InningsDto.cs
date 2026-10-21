namespace CricPulse.Application.DTOs.Scoring;

public class InningsDto
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }

    public int InningsNumber { get; set; }

    public string BattingTeam { get; set; } = string.Empty;

    public string BowlingTeam { get; set; } = string.Empty;

    public int TotalRuns { get; set; }

    public int Wickets { get; set; }

    public int LegalBalls { get; set; }

    public int TotalOvers { get; set; }

    public int? TargetRuns { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid? CurrentStrikerMatchPlayerId { get; set; }

    public Guid? CurrentNonStrikerMatchPlayerId { get; set; }

    public Guid? CurrentBowlerMatchPlayerId { get; set; }

    public string? CurrentStrikerName { get; set; }

    public string? CurrentNonStrikerName { get; set; }

    public string? CurrentBowlerName { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}