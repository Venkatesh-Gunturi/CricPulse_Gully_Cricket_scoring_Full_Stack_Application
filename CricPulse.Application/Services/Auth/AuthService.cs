using CricPulse.Application.DTOs.Auth;
using CricPulse.Application.DTOs.User;
using CricPulse.Application.Interfaces.Auth;
using CricPulse.Application.Interfaces.User;
using CricPulse.Domain.Exceptions;

using UserEntity = CricPulse.Domain.Entities.User;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricPulse.Application.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IOtpService _otpService;

        //Constructor for DI(s)
        public AuthService(IUserRepository userRepository, IPasswordHasher passwordHasher, IOtpService otpService)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _otpService = otpService;
        }

        public async Task<UserResponseDto> RegisterPlayerAsync(RegisterPlayerDto dto)
        {
            var normalizedFirstName = FormatName(dto.FirstName);

            var normalizedLastName = string.IsNullOrWhiteSpace(dto.LastName) ? null : FormatName(dto.LastName);

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var normalizedMobileNumber = dto.MobileNumber.Trim();

            // Check whether email already exists

            bool emailExists = await _userRepository.EmailExistsAsync(normalizedEmail);


            if (emailExists)
            {
                throw new ConflictException("An account with this email already exists. Please log in.");

            }

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
                Email = normalizedEmail,
                MobileNumber = normalizedMobileNumber,

                IsEmailVerified = false,
                IsMobileVerified = false,
                IsUmpire = false,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, dto.Password);

            var createdUser = await _userRepository.CreateAsync(user);

            //email OTP generation
            var emailOtp = _otpService.GenerateOtp();

            var emailOtpVerification = _otpService.CreateOtpVerification(createdUser.Id, emailOtp, Domain.Enums.OtpType.Email);

            //mobile number otp generation
            var mobileOtp = _otpService.GenerateOtp();
            var mobileOtpVerification = _otpService.CreateOtpVerification(createdUser.Id, mobileOtp, Domain.Enums.OtpType.Mobile);

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
    }
}
