using CricPulse.Domain.Entities;
using CricPulse.Domain.Enums;

namespace CricPulse.Application.Interfaces.Auth
{
    public interface IOtpService
    {
        string GenerateOtp();

        OtpVerification CreateOtpVerification(
            int userId,
            string otp,
            OtpType otpType);

        Task<bool> VerifyOtpAsync(int userId, string otpCode, OtpType otpType);
    }
}