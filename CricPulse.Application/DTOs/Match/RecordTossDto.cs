namespace CricPulse.Application.DTOs.Match
{
    public class RecordTossDto
    {
        public int MatchId { get; set; }
        public string TossWinnerTeam { get; set; } = string.Empty;
        public string TossDecision { get; set; } = string.Empty;
    }
}