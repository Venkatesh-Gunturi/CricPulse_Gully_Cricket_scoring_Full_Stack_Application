namespace CricPulse.Domain.Entities
{
    public class PlayerStatistics
    {
        public int Id { get; set; }

        public int PlayerId { get; set; }

        // Batting
        public int Matches { get; set; }
        public int BattingInnings { get; set; }
        public int Runs { get; set; }
        public int BallsFaced { get; set; }
        public int Fours { get; set; }
        public int Sixes { get; set; }
        public int Fifties { get; set; }
        public int Hundreds { get; set; }
        public int HighestScore { get; set; }

        // Bowling
        public int BowlingInnings { get; set; }
        public int BallsBowled { get; set; }
        public int RunsConceded { get; set; }
        public int Wickets { get; set; }
        public int MaidenOvers { get; set; }

        public Player Player { get; set; } = null!;
    }
}