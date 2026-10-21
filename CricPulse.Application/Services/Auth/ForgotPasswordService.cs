using System.Text.RegularExpressions;
using CricPulse.Application.DTOs.Auth;
using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Application.Interfaces.Services;

namespace CricPulse.Application.Services.Auth;

public class ForgotPasswordService : IForgotPasswordService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasherService _passwordHasher;

    public ForgotPasswordService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasherService passwordHasher)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task RequestPasswordResetAsync(ForgotPasswordDto dto)
    {
        ValidateRequest(dto);

        var mobileNumber = NormalizeMobileNumber(dto.MobileNumber);
        var email = NormalizeEmail(dto.Email);

        var user = await _userRepository.GetByMobileNumberAsync(
            mobileNumber);

        if (user is null ||
            !user.IsActive ||
            !string.Equals(
                user.Email,
                email,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The provided mobile number and email do not match our records.");
        }

        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();
    }

    private static void ValidateRequest(ForgotPasswordDto dto)
    {
        NormalizeMobileNumber(dto.MobileNumber);

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new ArgumentException("Email is required.");
        }

        var email = dto.Email.Trim();

        if (email.Length > 255)
        {
            throw new ArgumentException(
                "Email cannot exceed 255 characters.");
        }

        if (!Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new ArgumentException(
                "Please provide a valid email address.");
        }

        ValidatePassword(dto.NewPassword);

        if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
        {
            throw new ArgumentException(
                "Confirm password is required.");
        }

        if (dto.NewPassword != dto.ConfirmPassword)
        {
            throw new ArgumentException(
                "Password and confirm password do not match.");
        }
    }

    private static string NormalizeMobileNumber(string mobileNumber)
    {
        if (string.IsNullOrWhiteSpace(mobileNumber))
        {
            throw new ArgumentException(
                "Mobile number is required.");
        }

        var normalized = mobileNumber.Trim();

        if (!Regex.IsMatch(normalized, @"^[6-9]\d{9}$"))
        {
            throw new ArgumentException(
                "Mobile number must contain exactly 10 digits and start with 6, 7, 8 or 9.");
        }

        return normalized;
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password is required.");
        }

        if (password.Length < 8)
        {
            throw new ArgumentException(
                "Password must contain at least 8 characters.");
        }

        if (password.Length > 100)
        {
            throw new ArgumentException(
                "Password cannot exceed 100 characters.");
        }

        if (!Regex.IsMatch(password, @"[A-Z]"))
        {
            throw new ArgumentException(
                "Password must contain at least one uppercase letter.");
        }

        if (!Regex.IsMatch(password, @"[a-z]"))
        {
            throw new ArgumentException(
                "Password must contain at least one lowercase letter.");
        }

        if (!Regex.IsMatch(password, @"\d"))
        {
            throw new ArgumentException(
                "Password must contain at least one number.");
        }

        if (!Regex.IsMatch(password, @"[^a-zA-Z0-9]"))
        {
            throw new ArgumentException(
                "Password must contain at least one special character.");
        }
    }
}