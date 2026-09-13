namespace CricPulse.Application.DTOs.Match
{
    public class ScoreExtraDto
    {
        public int InningsId { get; set; }

        public string ExtraType { get; set; } = string.Empty;

        // Total runs added to the team's score by this delivery.
        public int Runs { get; set; }

        // Runs credited to the batter when the extra delivery also
        // produces runs from the bat, such as a no-ball hit for 4.
        public int BatterRuns { get; set; }
    }
}