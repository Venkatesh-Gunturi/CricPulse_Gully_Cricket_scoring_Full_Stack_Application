using System.ComponentModel.DataAnnotations;

namespace CricPulse.Application.DTOs.Auth
{
    public class VerifyOtpDto
    {
        [Required]
        public int RegistrationId { get; set; }

        [Required]
        [RegularExpression(
            @"^[0-9]{6}$",
            ErrorMessage = "OTP must contain exactly 6 digits.")]
        public string OtpCode { get; set; } = string.Empty;
    }
}