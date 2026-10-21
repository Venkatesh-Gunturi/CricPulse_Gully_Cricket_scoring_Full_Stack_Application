namespace CricPulse.Application.DTOs.Scoring;

public class ScoreResponseDto
{
    public Guid InningsId { get; set; }

    public int TotalRuns { get; set; }

    public int Wickets { get; set; }

    public int LegalBalls { get; set; }

    public int OverNumber { get; set; }

    public int BallNumber { get; set; }

    public string? CurrentStrikerName { get; set; }

    public string? CurrentNonStrikerName { get; set; }

    public string? CurrentBowlerName { get; set; }

    public bool InningsCompleted { get; set; }

    public string? InningsStatus { get; set; }

    public string? Message { get; set; }
}