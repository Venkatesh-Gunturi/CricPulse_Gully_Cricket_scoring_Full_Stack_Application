namespace CricPulse.Application.DTOs.Match
{
    public class ScoreExtraDto
    {
        public int InningsId { get; set; }

        public string ExtraType { get; set; } = string.Empty;

        public int Runs { get; set; }

        public int BatterRuns { get; set; }

        public int RunsCompleted { get; set; }

        public int? DismissedMatchPlayerId { get; set; }

        public bool DidBattersCross { get; set; }

        public int? NewBatterMatchPlayerId { get; set; }
    }
}