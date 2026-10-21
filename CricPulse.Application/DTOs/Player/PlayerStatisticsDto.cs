namespace CricPulse.Application.DTOs.Player;

public class PlayerStatisticsDto
{
    public int Matches { get; set; }

    public int Innings { get; set; }

    public int Runs { get; set; }

    public int BallsFaced { get; set; }

    public int HighestScore { get; set; }

    public int NotOuts { get; set; }

    public int Fours { get; set; }

    public int Sixes { get; set; }

    public int Fifties { get; set; }

    public int Hundreds { get; set; }

    public decimal BattingAverage { get; set; }

    public decimal StrikeRate { get; set; }

    public int BowlingInnings { get; set; }

    public int BallsBowled { get; set; }

    public int RunsConceded { get; set; }

    public int Wickets { get; set; }

    public int Maidens { get; set; }

    public decimal Economy { get; set; }

    public string? BestBowling { get; set; }

    public int ThreeWicketHauls { get; set; }

    public int FiveWicketHauls { get; set; }

    public int Catches { get; set; }

    public int RunOuts { get; set; }

    public int Stumpings { get; set; }

    public int MVPCount { get; set; }
}