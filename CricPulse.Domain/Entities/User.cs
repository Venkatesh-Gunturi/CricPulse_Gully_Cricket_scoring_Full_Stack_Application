namespace CricPulse.Domain.Entities;

/// <summary>
/// The single canonical CricPulse identity. Cricket and organizer details are
/// optional attributes of the same account; they do not create separate roles.
/// </summary>
public class User
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }

    // Optional cricket profile details. A user does not need to fill these in
    // to create an account, join a match, or organize a match.
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? State { get; set; }
    public string? PinCode { get; set; }
    public string? BattingStyle { get; set; }
    public string? BowlingStyle { get; set; }
    public string? PlayerRole { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
