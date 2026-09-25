namespace CricPulse.Application.DTOs.Player
{
    public class PlayerStatisticsResponseDto
    {
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

        // Derived batting statistics
        public double BattingAverage { get; set; }
        public double StrikeRate { get; set; }

        // Bowling
        public int BowlingInnings { get; set; }
        public int BallsBowled { get; set; }
        public int RunsConceded { get; set; }
        public int Wickets { get; set; }
        public int MaidenOvers { get; set; }

        // Derived bowling statistic
        public double Economy { get; set; }

        // MVP
        public int MVPCount { get; set; }
    }
}