namespace CricPulse.Application.DTOs.Scoring;

public class ScoreWicketDto
{
    public Guid CreatedByUserId { get; set; }

    public string WicketType { get; set; } = string.Empty;

    public Guid DismissedMatchPlayerId { get; set; }

    public Guid? NewBatterMatchPlayerId { get; set; }

    public Guid? CaughtByMatchPlayerId { get; set; }

    public Guid? RunOutByMatchPlayerId { get; set; }

    public Guid? StumpedByMatchPlayerId { get; set; }

    public int RunsCompleted { get; set; }
}