namespace CricPulse.Application.DTOs.Player
{
    public class PlayerResponseDto
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }

        public string? ProfileImageUrl { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Gender { get; set; } = string.Empty;

        public string BattingStyle { get; set; } = string.Empty;

        public string BowlingStyle { get; set; } = string.Empty;

        public string PlayerRole { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public int PinCode { get; set; }
    }
}