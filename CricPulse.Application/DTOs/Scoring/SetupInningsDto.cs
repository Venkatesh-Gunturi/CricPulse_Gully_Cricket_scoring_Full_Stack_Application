namespace CricPulse.Application.DTOs.Scoring;

public class SetupInningsDto
{
    public Guid CreatedByUserId { get; set; }

    public Guid StrikerMatchPlayerId { get; set; }

    public Guid NonStrikerMatchPlayerId { get; set; }

    public Guid BowlerMatchPlayerId { get; set; }
}