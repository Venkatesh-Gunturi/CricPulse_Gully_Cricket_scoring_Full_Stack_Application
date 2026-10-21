namespace CricPulse.Application.DTOs.Scoring;

public class TossDto
{
    public Guid CreatedByUserId { get; set; }

    public string TossWinnerTeam { get; set; } = string.Empty;

    public string TossDecision { get; set; } = string.Empty;

    public bool Agree { get; set; }
}