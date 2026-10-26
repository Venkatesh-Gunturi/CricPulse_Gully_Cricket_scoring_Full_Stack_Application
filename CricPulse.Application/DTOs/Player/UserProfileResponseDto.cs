namespace CricPulse.Application.DTOs.Player;

public class UserProfileResponseDto
{
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }

    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? State { get; set; }
    public string? PinCode { get; set; }
    public string? BattingStyle { get; set; }
    public string? BowlingStyle { get; set; }
    public string? PlayerRole { get; set; }

    public PlayerStatisticsResponseDto Statistics { get; set; } = new();
}

public class PlayerStatisticsResponseDto
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

    public int BowlingInnings { get; set; }
    public int BallsBowled { get; set; }
    public int RunsConceded { get; set; }
    public int Wickets { get; set; }
    public int Maidens { get; set; }
    public int ThreeWicketHauls { get; set; }
    public int FiveWicketHauls { get; set; }
    public string? BestBowling { get; set; }

    public int Catches { get; set; }
    public int RunOuts { get; set; }
    public int Stumpings { get; set; }
    public int MVPCount { get; set; }
}