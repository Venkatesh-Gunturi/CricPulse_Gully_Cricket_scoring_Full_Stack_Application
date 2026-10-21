namespace CricPulse.Application.DTOs.Scoring;

public class StartMatchDto
{
    public Guid MatchId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public double Latitude { get; set; }

    public double Longitude { get; set; }
}