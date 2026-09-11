using CricPulse.Application.Interfaces.Auth;
using CricPulse.Application.Interfaces.Otp;
using CricPulse.Application.Interfaces.player;
using CricPulse.Application.Interfaces.User;
using CricPulse.Domain.Entities;
using CricPulse.Domain.Enums;


namespace CricPulse.Infrastructure.Authentication
{
    public class OtpService : IOtpService
    {
        private readonly IOtpRepository _otpRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPlayerRepository _playerRepository;

        public OtpService(IOtpRepository otpRepository, IUserRepository userRepository,IPlayerRepository playerRepository  )
        {
            _otpRepository = otpRepository;
            _userRepository = userRepository;
            _playerRepository = playerRepository;
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

        // Purpose:
        // Verify the user's OTP and create their Player profile after successful mobile verification.
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

                // Every verified CricPulse account is automatically a Player.
                var existingPlayer =
                    await _playerRepository.GetByUserIdAsync(user.Id);

                if (existingPlayer == null)
                {
                    var player = new Player
                    {
                        UserId = user.Id,
                        DateOfBirth = default,
                        Gender = string.Empty,
                        BattingStyle = string.Empty,
                        BowlingStyle = string.Empty,
                        PlayerRole = string.Empty,
                        State = string.Empty,
                        PinCode = 0
                    };

                    await _playerRepository.CreateAsync(player);
                }
            }

            await _userRepository.UpdateAsync(user);

            return true;
        }
    }
}