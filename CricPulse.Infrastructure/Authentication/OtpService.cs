using CricPulse.Application.Interfaces.Auth;
using CricPulse.Application.Interfaces.Otp;
using CricPulse.Domain.Entities;
using CricPulse.Domain.Enums;
using CricPulse.Application.Interfaces.User;

namespace CricPulse.Infrastructure.Authentication
{
    public class OtpService : IOtpService
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IUserRepository _userRepository;

        public OtpService(IOtpRepository otpRepository, IUserRepository userRepository  )
        {
            _otpRepository = otpRepository;
            _userRepository = userRepository;
        }

        public string GenerateOtp()
        {
            return Random.Shared.Next(100000, 1000000).ToString();
        }

        public OtpVerification CreateOtpVerification(
            int userId,
            string otp,
            OtpType otpType)
        {
            return new OtpVerification
            {
                UserId = userId,
                OtpCode = otp,
                OtpType = otpType,
                ExpiresAt = DateTime.UtcNow.AddMinutes(5),
                IsUsed = false,
                AttemptedCount = 0,
                CreatedAt = DateTime.UtcNow
            };
        }

        public async Task<bool> VerifyOtpAsync(
            int userId,
            string otpCode,
            OtpType otpType)
        {
            var otpVerification =
                await _otpRepository.GetLatestAsync(userId, otpType);

            if (otpVerification == null)
                return false;

            if (otpVerification.IsUsed)
                return false;

            if (otpVerification.ExpiresAt < DateTime.UtcNow)
                return false;

            if (otpVerification.OtpCode != otpCode)
            {
                otpVerification.AttemptedCount++;

                await _otpRepository.UpdateAsync(otpVerification);

                return false;
            }

            otpVerification.IsUsed = true;

            await _otpRepository.UpdateAsync(otpVerification);

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                return false;

            if (otpType == OtpType.Email)
            {
                user.IsEmailVerified = true;
            }
            else if (otpType == OtpType.Mobile)
            {
                user.IsMobileVerified = true;
            }

            await _userRepository.UpdateAsync(user);

            return true;
        }
    }
}