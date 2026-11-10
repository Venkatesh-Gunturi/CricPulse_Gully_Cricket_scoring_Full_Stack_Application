using System.Text.RegularExpressions;
using CricPulse.Application.DTOs.Auth;
using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Application.Interfaces.Services;

namespace CricPulse.Application.Services.Auth;
// Handles password reset requests by validating user details and updating password credentials.
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
    // Validates the reset request, verifies the user's identity, and updates their password.
    public async Task RequestPasswordResetAsync(ForgotPasswordDto dto)
    {
        // Validate the supplied mobile number, email, and new password.
        ValidateRequest(dto);
        // Normalize the mobile number and email before performing the user lookup.
        var mobileNumber = NormalizeMobileNumber(dto.MobileNumber);
        var email = NormalizeEmail(dto.Email);
        // Find the account associated with the supplied mobile number.
        var user = await _userRepository.GetByMobileNumberAsync(
            mobileNumber);
        // Ensure the account exists, is active, and the supplied email matches the account.
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
        // Hash the new password before storing it in the user record.
        user.PasswordHash = _passwordHasher.Hash(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        // Persist the updated password and timestamp.
        await _unitOfWork.SaveChangesAsync();
    }
    // Validates the mobile number, email address, new password, and password confirmation.
    private static void ValidateRequest(ForgotPasswordDto dto)
    {
        NormalizeMobileNumber(dto.MobileNumber);
        // Validate the email format and enforce the maximum supported length.
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
        // Enforce the application's password complexity requirements.
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
    // Trims and validates an Indian mobile number against the expected 10-digit format.
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
    // Removes surrounding whitespace and converts the email address to lowercase.
    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
    // Enforces password length and complexity requirements.
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