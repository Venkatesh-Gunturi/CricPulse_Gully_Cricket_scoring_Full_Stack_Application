
namespace CricPulse.Application.DTOs.Scoring;

public class BatterRecordDto
{
    public Guid MatchPlayerId { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public int Runs { get; set; }

    public int Balls { get; set; }

    public int Fours { get; set; }

    public int Sixes { get; set; }

    public decimal StrikeRate { get; set; }

    public string Dismissal { get; set; } = "Not out";
}
