namespace CricPulse.Application.DTOs.Match
{
    public class StartInningsDto
    {
        public int MatchId { get; set; }
        public int StrikerMatchPlayerId { get; set; }
        public int NonStrikerMatchPlayerId { get; set; }
        public int BowlerMatchPlayerId { get; set; }
    }
}