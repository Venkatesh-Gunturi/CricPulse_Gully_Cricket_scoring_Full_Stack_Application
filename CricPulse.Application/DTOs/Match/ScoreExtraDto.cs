namespace CricPulse.Application.DTOs.Match
{
    public class ScoreExtraDto
    {
        public int InningsId { get; set; }
        public string ExtraType { get; set; } = string.Empty;
        public int Runs { get; set; }
    }
}