
namespace CricPulse.Application.DTOs.Scoring;

public class BowlerRecordDto
{
    public Guid MatchPlayerId { get; set; }

    public string PlayerName { get; set; } = string.Empty;

    public decimal Overs { get; set; }

    public int Maidens { get; set; }

    public int RunsConceded { get; set; }

    public int Wickets { get; set; }

    public decimal Economy { get; set; }
}
