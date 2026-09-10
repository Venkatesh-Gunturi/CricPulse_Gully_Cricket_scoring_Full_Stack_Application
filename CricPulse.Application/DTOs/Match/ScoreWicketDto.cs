namespace CricPulse.Application.DTOs.Match
{
    public class ScoreWicketDto
    {
        public int InningsId { get; set; }
        public string WicketType { get; set; } = string.Empty;
        public int? DismissedMatchPlayerId { get; set; }
        public int? CaughtByMatchPlayerId { get; set; }
        public int RunsCompleted { get; set; }
        public int NewBatterMatchPlayerId { get; set; }
        public bool NewBatterIsStriker { get; set; }
    }
}