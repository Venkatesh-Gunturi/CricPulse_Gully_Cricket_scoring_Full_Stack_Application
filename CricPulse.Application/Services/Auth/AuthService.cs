using CricPulse.Application.DTOs.Auth;
using CricPulse.Application.DTOs.User;
using CricPulse.Application.Interfaces.Auth;
using CricPulse.Application.Interfaces.Otp;
using CricPulse.Application.Interfaces.player;
using CricPulse.Application.Interfaces.User;
using CricPulse.Domain.Entities;
using CricPulse.Domain.Exceptions;

using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserEntity = CricPulse.Domain.Entities.User;
using PlayerEntity = CricPulse.Domain.Entities.Player;


namespace CricPulse.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IOtpService _otpService;
        private readonly IOtpRepository _otpRepository;
        private readonly IJwtService _jwtService;
        private readonly IPendingRegistrationRepository _pendingRegistrationRepository;
        private readonly IPlayerRepository _playerRepository;

        //Constructor for DI(s)
        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IOtpService otpService,IOtpRepository otpRepository, IJwtService jwtService, IPendingRegistrationRepository pendingRegistrationRepository, IPlayerRepository playerRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _otpService = otpService;
            _otpRepository = otpRepository;
            _jwtService = jwtService;
            _pendingRegistrationRepository = pendingRegistrationRepository;
            _playerRepository = playerRepository;
        }

        // Purpose:
        // Start registration by sending an OTP, or complete registration after mobile verification.
        public async Task<RegistrationOtpResponseDto> RegisterPlayerAsync(
            RegisterPlayerDto dto)
        {
            var normalizedMobile = dto.MobileNumber.Trim();

            var existingUser =
                await _userRepository.GetByMobileNumberAsync(normalizedMobile);

            if (existingUser != null)
            {
                throw new ConflictException(
                    "An account with this mobile number already exists.");
            }

            var pendingRegistration =
                await _pendingRegistrationRepository
                    .GetByMobileNumberAsync(normalizedMobile);

            // An expired registration can no longer be used.
            if (pendingRegistration != null &&
                pendingRegistration.OtpExpiresAt < DateTime.UtcNow)
            {
                await _pendingRegistrationRepository.DeleteAsync(
                    pendingRegistration);

                pendingRegistration = null;
            }

            // OTP was already verified, so this call is the final "Create Account" step.
            if (pendingRegistration != null &&
                pendingRegistration.IsOtpVerified)
            {
                if (string.IsNullOrWhiteSpace(dto.Password) ||
                    dto.Password.Length < 8)
                {
                    throw new ConflictException(
                        "Password must contain at least 8 characters.");
                }

                var passwordHash =
                    _passwordHasher.HashPassword(
                        new UserEntity(),
                        dto.Password);

                var user = new UserEntity
                {
                    FirstName = pendingRegistration.FirstName,
                    LastName = pendingRegistration.LastName,
                    Email = null,
                    MobileNumber = pendingRegistration.MobileNumber,
                    PasswordHash = passwordHash,
                    IsEmailVerified = false,
                    IsMobileVerified = true,
                    IsUmpire = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createdUser =
                    await _userRepository.CreateAsync(user);

                // Every successfully registered CricPulse account is automatically a Player.
                var player = new PlayerEntity
                {
                    UserId = createdUser.Id,
                    DateOfBirth = default,
                    Gender = string.Empty,
                    BattingStyle = string.Empty,
                    BowlingStyle = string.Empty,
                    PlayerRole = string.Empty,
                    State = string.Empty,
                    PinCode = 0
                };

                await _playerRepository.CreateAsync(player);

                var token =
                    _jwtService.GenerateToken(createdUser);

                var userResponse = new UserResponseDto
                {
                    Id = createdUser.Id,
                    FirstName = createdUser.FirstName,
                    LastName = createdUser.LastName,
                    Email = createdUser.Email,
                    MobileNumber = createdUser.MobileNumber,
                    IsEmailVerified = createdUser.IsEmailVerified,
                    IsMobileVerified = createdUser.IsMobileVerified,
                    IsUmpire = createdUser.IsUmpire,
                    ProfileImageUrl = createdUser.ProfileImageUrl,
                    IsActive = createdUser.IsActive,
                    CreatedAt = createdUser.CreatedAt
                };

                await _pendingRegistrationRepository.DeleteAsync(
                    pendingRegistration);

                return new RegistrationOtpResponseDto
                {
                    RegistrationId = pendingRegistration.Id,
                    UserId = createdUser.Id,
                    MobileNumber = createdUser.MobileNumber,
                    Message = "Account created successfully.",
                    Token = token,
                    User = userResponse
                };
            }

            // An active pending registration means an OTP has already been issued.
            if (pendingRegistration != null)
            {
                throw new ConflictException(
                    "A registration OTP is already active for this mobile number.");
            }

            var otp = _otpService.GenerateOtp();

            // Development-only OTP visibility until SMS integration is added.
            Console.WriteLine(
                $"[DEV OTP] Mobile: {normalizedMobile}, OTP: {otp}");

            var registration = new PendingRegistration
            {
                FirstName = dto.FirstName.Trim(),

                LastName = string.IsNullOrWhiteSpace(dto.LastName)
                    ? null
                    : dto.LastName.Trim(),

                MobileNumber = normalizedMobile,

                // Password is intentionally not stored at the OTP stage.
                PasswordHash = null,

                OtpCode = otp,

                OtpExpiresAt = DateTime.UtcNow.AddMinutes(5),

                AttemptedCount = 0,

                IsOtpVerified = false,

                CreatedAt = DateTime.UtcNow
            };

            await _pendingRegistrationRepository.CreateAsync(
                registration);

            return new RegistrationOtpResponseDto
            {
                RegistrationId = registration.Id,
                UserId = null,
                MobileNumber = registration.MobileNumber,
                Message = "OTP sent successfully.",
                Token = null,
                User = null
            };
        }

        //Formating the user name ex. like nani --> Nani
        public static string FormatName(string name)
        {
            if(string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
            else
            {
                return string.Join(" ", name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries)
                             .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
            }
        }

        // Purpose:
        // Verify the temporary registration OTP and mark the pending registration as mobile-verified.
        public async Task<bool> VerifyOtpAsync(VerifyOtpDto dto)
        {
            var pendingRegistration =
                await _pendingRegistrationRepository
                    .GetByIdAsync(dto.RegistrationId);

            if (pendingRegistration == null)
                return false;

            // The pending registration and its OTP expire together after 5 minutes.
            if (pendingRegistration.OtpExpiresAt < DateTime.UtcNow)
            {
                await _pendingRegistrationRepository.DeleteAsync(
                    pendingRegistration);

                return false;
            }

            if (pendingRegistration.IsOtpVerified)
                return true;

            if (pendingRegistration.OtpCode != dto.OtpCode)
            {
                pendingRegistration.AttemptedCount++;

                await _pendingRegistrationRepository.UpdateAsync(
                    pendingRegistration);

                return false;
            }

            pendingRegistration.IsOtpVerified = true;

            await _pendingRegistrationRepository.UpdateAsync(
                pendingRegistration);

            return true;
        }


        // Purpose:
        // Authenticate a user and provide a specific reason when login fails.
        public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
        {
            var identifier = dto.Identifier.Trim().ToLowerInvariant();

            UserEntity? user;

            if (identifier.Contains("@"))
            {
                user = await _userRepository.GetByEmailAsync(identifier);
            }
            else
            {
                user = await _userRepository.GetByMobileNumberAsync(
                    dto.Identifier.Trim());
            }

            if (user == null)
            {
                throw new ConflictException(
                    "This mobile number is not registered. Please register first.");
            }

            if (!user.IsActive)
            {
                throw new ConflictException(
                    "This account is not active. Please complete your registration.");
            }

            if (identifier.Contains("@") && !user.IsEmailVerified)
            {
                throw new ConflictException(
                    "This email address is not verified.");
            }

            if (!identifier.Contains("@") && !user.IsMobileVerified)
            {
                throw new ConflictException(
                    "This mobile number is not verified.");
            }

            var passwordResult =
                _passwordHasher.VerifyPassword(
                    user,
                    user.PasswordHash,
                    dto.Password);

            if (!passwordResult)
            {
                throw new ConflictException(
                    "Incorrect password.");
            }

            var token = _jwtService.GenerateToken(user);

            return new LoginResponseDto
            {
                Token = token,

                User = new UserResponseDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    MobileNumber = user.MobileNumber,
                    IsEmailVerified = user.IsEmailVerified,
                    IsMobileVerified = user.IsMobileVerified,
                    IsUmpire = user.IsUmpire,
                    ProfileImageUrl = user.ProfileImageUrl,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }
            };
        }

        public async Task<UserResponseDto> RegisterMatchPlayerAsync(string mobileNumber)
        {
            var normalizedMobile =
                mobileNumber.Trim();

            var existingUser =
                await _userRepository.GetByMobileNumberAsync(
                    normalizedMobile);

            if (existingUser != null)
            {
                throw new ConflictException(
                    "An account with this mobile number already exists.");
            }

            var user = new UserEntity
            {
                FirstName = "Player",
                LastName = null,

                Email = null,

                MobileNumber = normalizedMobile,

                IsEmailVerified = false,
                IsMobileVerified = false,

                IsUmpire = false,

                IsActive = false,

                CreatedAt = DateTime.UtcNow
            };

            var temporaryPassword =
                Convert.ToBase64String(
                    System.Security.Cryptography.RandomNumberGenerator
                        .GetBytes(24));

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    temporaryPassword);

            var createdUser =
                await _userRepository.CreateAsync(user);

            var mobileOtp =
                _otpService.GenerateOtp();

            var otpVerification =
                _otpService.CreateOtpVerification(
                    createdUser.Id,
                    mobileOtp,
                    Domain.Enums.OtpType.Mobile);

            await _otpRepository.CreateAsync(
                otpVerification);

            return new UserResponseDto
            {
                Id = createdUser.Id,

                FirstName = createdUser.FirstName,

                LastName = createdUser.LastName,

                Email = createdUser.Email,

                MobileNumber = createdUser.MobileNumber,

                IsEmailVerified =
                    createdUser.IsEmailVerified,

                IsMobileVerified =
                    createdUser.IsMobileVerified,

                ProfileImageUrl =
                    createdUser.ProfileImageUrl,

                IsActive =
                    createdUser.IsActive,

                CreatedAt =
                    createdUser.CreatedAt
            };
        }

        // Purpose:
        // Promote a registered user to an umpire so they can create and manage matches.
        public async Task<UserResponseDto> BecomeUmpireAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "User not found.");
            }

            if (!user.IsActive)
            {
                throw new InvalidOperationException(
                    "Your account is not active.");
            }

            if (!user.IsMobileVerified)
            {
                throw new InvalidOperationException(
                    "Your mobile number must be verified before becoming an umpire.");
            }

            if (!user.IsUmpire)
            {
                user.IsUmpire = true;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
            }

            return new UserResponseDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                IsEmailVerified = user.IsEmailVerified,
                IsMobileVerified = user.IsMobileVerified,
                IsUmpire = user.IsUmpire,
                ProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
