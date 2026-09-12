using CricPulse.Application.DTOs.User;

namespace CricPulse.Application.DTOs.Auth
{
    public class RegistrationOtpResponseDto
    {
        public int RegistrationId { get; set; }

        public int? UserId { get; set; }

        public string MobileNumber { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string? Token { get; set; }

        public UserResponseDto? User { get; set; }
    }
}