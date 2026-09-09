namespace CricPulse.Domain.Entities
{
    public class MatchPlayer
    {
        public int Id { get; set; }

        public int MatchId { get; set; }

        public int PlayerId { get; set; }

        public string Team { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public Match Match { get; set; } = null!;

        public Player Player { get; set; } = null!;
    }
}