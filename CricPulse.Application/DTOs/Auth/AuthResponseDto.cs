namespace CricPulse.Application.DTOs.Auth;

public class AuthResponseDto
{
    public Guid UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string? LastName { get; set; }

    public string MobileNumber { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? ProfileImageUrl { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? State { get; set; }
    public string? PinCode { get; set; }
    public string? BattingStyle { get; set; }
    public string? BowlingStyle { get; set; }
    public string? PlayerRole { get; set; }
}