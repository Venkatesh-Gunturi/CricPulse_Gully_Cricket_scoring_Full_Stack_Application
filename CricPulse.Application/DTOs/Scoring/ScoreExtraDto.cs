namespace CricPulse.Application.DTOs.Scoring;

public class ScoreExtraDto
{
    public Guid CreatedByUserId { get; set; }

    public string ExtraType { get; set; } = string.Empty;

    public int TotalRuns { get; set; }

    public int BatterRuns { get; set; }

    public int RunsCompleted { get; set; }
    public bool BattersCrossed { get; set; }
    public Guid? DismissedMatchPlayerId { get; set; }

    public Guid? RunOutByMatchPlayerId { get; set; }

    public Guid? NewBatterMatchPlayerId { get; set; }
}