using CricPulse.Domain.Enums;

namespace CricPulse.Application.DTOs.Auth
{
    public class VerifyOtpDto
    {
        public int UserId { get; set; }
        public string OtpCode { get; set; } = string.Empty;
        public OtpType OtpType { get; set; }
    }
}