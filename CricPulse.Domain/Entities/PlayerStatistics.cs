namespace CricPulse.Domain.Entities;

public class PlayerStatistics
{
    public Guid UserId { get; set; }

    // Batting
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

    // Bowling
    public int BowlingInnings { get; set; }

    public int BallsBowled { get; set; }

    public int RunsConceded { get; set; }

    public int Wickets { get; set; }

    public int Maidens { get; set; }

    public int ThreeWicketHauls { get; set; }

    public int FiveWicketHauls { get; set; }

    public string? BestBowling { get; set; }

    // Fielding
    public int Catches { get; set; }

    public int RunOuts { get; set; }

    public int Stumpings { get; set; }

    // Achievements
    public int MVPCount { get; set; }

    public User User { get; set; } = null!;
}