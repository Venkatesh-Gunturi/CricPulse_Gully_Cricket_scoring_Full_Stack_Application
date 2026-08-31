using CricPulse.Application.DTOs.Auth;
using CricPulse.Application.DTOs.User;
using CricPulse.Application.Interfaces.Auth;
using CricPulse.Application.Interfaces.Otp;
using CricPulse.Application.Interfaces.User;
using CricPulse.Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserEntity = CricPulse.Domain.Entities.User;


namespace CricPulse.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IOtpService _otpService;
        private readonly IOtpRepository _otpRepository;

        //Constructor for DI(s)
        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IOtpService otpService,IOtpRepository otpRepository)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _otpService = otpService;
            _otpRepository = otpRepository;
        }

        public async Task<UserResponseDto> RegisterPlayerAsync(RegisterPlayerDto dto)
        {
            var normalizedFirstName = FormatName(dto.FirstName);

            var normalizedLastName = string.IsNullOrWhiteSpace(dto.LastName) ? null : FormatName(dto.LastName);


            var normalizedMobileNumber = dto.MobileNumber.Trim();

            
            // Check whether mobile number already exists
            bool mobileExists = await _userRepository.MobileExistsAsync(normalizedMobileNumber);


            if (mobileExists)
            {
                throw new ConflictException("An account with this mobile number already exists. Please log in.");

            }

            var user = new UserEntity
            {
                FirstName = normalizedFirstName,
                LastName = normalizedLastName,
                Email = null,
                MobileNumber = normalizedMobileNumber,

                IsEmailVerified = false,
                IsMobileVerified = false,
                IsUmpire = false,
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            var createdUser = await _userRepository.CreateAsync(user);

            // Mobile OTP generation
            var mobileOtp = _otpService.GenerateOtp();

            var mobileOtpVerification = _otpService.CreateOtpVerification(
                createdUser.Id,
                mobileOtp,
                Domain.Enums.OtpType.Mobile);

            await _otpRepository.CreateAsync(mobileOtpVerification);


            return new UserResponseDto
            {
                Id = createdUser.Id,
                FirstName = createdUser.FirstName,
                LastName = createdUser.LastName,
                Email = createdUser.Email,
                MobileNumber = createdUser.MobileNumber,
                IsEmailVerified=createdUser.IsEmailVerified,
                IsMobileVerified=createdUser.IsMobileVerified,
                ProfileImageUrl = createdUser.ProfileImageUrl,
                IsActive= createdUser.IsActive,
                CreatedAt= createdUser.CreatedAt
            };
        }

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

        public async Task<bool> VerifyOtpAsync(VerifyOtpDto dto)
        {
            var isVerified = await _otpService.VerifyOtpAsync(
            dto.UserId,
            dto.OtpCode,
            dto.OtpType);


            if (!isVerified)
            {
                return false;
            }

            var user = await _userRepository.GetByIdAsync(dto.UserId);

            if (user == null)
            {
                return false;
            }

            if (dto.OtpType == Domain.Enums.OtpType.Mobile)
            {
                user.IsMobileVerified = true;
                user.IsActive = true;
            }
            else if (dto.OtpType == Domain.Enums.OtpType.Email)
            {
                user.IsEmailVerified = true;
            }

            await _userRepository.UpdateAsync(user);

            return true;


}



        public async Task<UserResponseDto?> LoginAsync(LoginDto dto)
        {
            var identifier = dto.Identifier.Trim().ToLowerInvariant();

            UserEntity? user;

            if (identifier.Contains("@"))
            {
                user = await _userRepository.GetByEmailAsync(identifier);
            }
            else
            {
                user = await _userRepository.GetByMobileNumberAsync(dto.Identifier.Trim());
            }

            if (user == null)
            {
                return null;
            }

            if (!user.IsMobileVerified || !user.IsActive)
            {
                return null;
            }

            var passwordResult = _passwordHasher.VerifyPassword(
                user,
                user.PasswordHash,
                dto.Password);

            if (!passwordResult)
            {
                return null;
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
                ProfileImageUrl = user.ProfileImageUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
