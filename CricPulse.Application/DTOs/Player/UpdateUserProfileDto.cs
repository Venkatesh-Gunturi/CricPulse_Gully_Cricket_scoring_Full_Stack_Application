
namespace CricPulse.Application.DTOs.Player;

public class UpdateUserProfileDto
{
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
}
