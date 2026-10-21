
using System.Net.Mail;
using System.Text.RegularExpressions;
using CricPulse.Application.DTOs.Player;
using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Application.Interfaces.Services;
using CricPulse.Domain.Entities;

namespace CricPulse.Application.Services.Player;

public sealed class UserAccountProfileService : IUserAccountProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserAccountProfileService(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UserProfileResponseDto> GetProfileAsync(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.");
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null || !user.IsActive)
            throw new KeyNotFoundException("Active user account was not found.");
        return MapProfile(user);
    }

    public async Task<UserProfileResponseDto> UpdateProfileAsync(
        Guid userId,
        UpdateUserProfileDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var firstName = NormalizeName(dto.FirstName, "First name", required: true)!;
        var lastName = NormalizeName(dto.LastName, "Last name", required: false);
        var mobileNumber = (dto.MobileNumber ?? string.Empty).Trim();
        var email = (dto.Email ?? string.Empty).Trim().ToLowerInvariant();
        var profileImageUrl = NormalizeImageUrl(dto.ProfileImageUrl);

        ValidateMobileNumber(mobileNumber);
        ValidateEmail(email);

        var user = await _userRepository.GetByIdAsync(userId);

        if (user is null || !user.IsActive)
        {
            throw new KeyNotFoundException("Active user account was not found.");
        }

        var mobileOwner =
            await _userRepository.GetByMobileNumberAsync(mobileNumber);

        if (mobileOwner is not null && mobileOwner.Id != userId)
        {
            throw new ArgumentException(
                "This mobile number is already registered.");
        }

        var emailOwner = await _userRepository.GetByEmailAsync(email);

        if (emailOwner is not null && emailOwner.Id != userId)
        {
            throw new ArgumentException(
                "This email address is already registered.");
        }

        user.FirstName = firstName;
        user.LastName = lastName;
        user.MobileNumber = mobileNumber;
        user.Email = email;
        user.ProfileImageUrl = profileImageUrl;
        ValidateCricketDetails(dto);
        user.DateOfBirth = dto.DateOfBirth;
        user.Gender = NormalizeOptional(dto.Gender);
        user.State = NormalizeOptional(dto.State);
        user.PinCode = NormalizeOptional(dto.PinCode);
        user.BattingStyle = NormalizeOptional(dto.BattingStyle);
        user.BowlingStyle = NormalizeOptional(dto.BowlingStyle);
        user.PlayerRole = NormalizeOptional(dto.PlayerRole);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync();

        return MapProfile(user);
    }

    private static string? NormalizeName(
        string? value,
        string fieldName,
        bool required)
    {
        var name = value?.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            if (required)
            {
                throw new ArgumentException($"{fieldName} is required.");
            }

            return null;
        }

        if (name.Length > 15)
        {
            throw new ArgumentException(
                $"{fieldName} cannot exceed 15 characters.");
        }

        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    private static void ValidateMobileNumber(string mobileNumber)
    {
        if (!Regex.IsMatch(mobileNumber, @"^[6-9]\d{9}$"))
        {
            throw new ArgumentException(
                "Mobile number must contain exactly 10 digits and start with 6, 7, 8 or 9.");
        }
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || email.Length > 255)
        {
            throw new ArgumentException(
                "A valid email address is required.");
        }

        try
        {
            var parsed = new MailAddress(email);

            if (!string.Equals(
                    parsed.Address,
                    email,
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Invalid email address.");
            }
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid email address.");
        }
    }

    private static UserProfileResponseDto MapProfile(User user) => new()
    {
        UserId = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        MobileNumber = user.MobileNumber,
        Email = user.Email,
        ProfileImageUrl = user.ProfileImageUrl,
        DateOfBirth = user.DateOfBirth,
        Gender = user.Gender,
        State = user.State,
        PinCode = user.PinCode,
        BattingStyle = user.BattingStyle,
        BowlingStyle = user.BowlingStyle,
        PlayerRole = user.PlayerRole
    };

    private static void ValidateCricketDetails(UpdateUserProfileDto dto)
    {
        if (dto.DateOfBirth.HasValue && dto.DateOfBirth.Value.Date > DateTime.UtcNow.Date)
            throw new ArgumentException("Date of birth cannot be in the future.");
        if (!string.IsNullOrWhiteSpace(dto.Gender) && dto.Gender is not ("Male" or "Female" or "Prefer not to say"))
            throw new ArgumentException("Invalid gender selection.");
        if (!string.IsNullOrWhiteSpace(dto.BattingStyle) && dto.BattingStyle is not ("Right Hand" or "Left Hand"))
            throw new ArgumentException("Invalid batting style selection.");
        if (!string.IsNullOrWhiteSpace(dto.PlayerRole) && dto.PlayerRole is not ("Batter" or "Bowler" or "All-Rounder" or "Wicket-Keeper"))
            throw new ArgumentException("Invalid player role selection.");
        if (!string.IsNullOrWhiteSpace(dto.PinCode) && !Regex.IsMatch(dto.PinCode.Trim(), @"^\d{6}$"))
            throw new ArgumentException("Pin code must contain exactly 6 digits.");
        if (!string.IsNullOrWhiteSpace(dto.State) && !IsValidState(dto.State.Trim()))
            throw new ArgumentException("Invalid state or union territory selection.");
        if (!string.IsNullOrWhiteSpace(dto.BowlingStyle) && !IsValidBowlingStyle(dto.BowlingStyle.Trim()))
            throw new ArgumentException("Invalid bowling style selection.");
    }

    private static bool IsValidBowlingStyle(string value) => value is
        "Right Arm Fast" or "Right Arm Fast Medium" or "Right Arm Medium Fast" or
        "Right Arm Medium" or "Left Arm Fast" or "Left Arm Fast Medium" or
        "Left Arm Medium Fast" or "Left Arm Medium" or "Right Arm Off Break" or
        "Right Arm Leg Break" or "Left Arm Orthodox" or "Left Arm Wrist Spin" or "None";

    private static bool IsValidState(string value) => value is
        "Andhra Pradesh" or "Arunachal Pradesh" or "Assam" or "Bihar" or
        "Chhattisgarh" or "Goa" or "Gujarat" or "Haryana" or "Himachal Pradesh" or
        "Jharkhand" or "Karnataka" or "Kerala" or "Madhya Pradesh" or "Maharashtra" or
        "Manipur" or "Meghalaya" or "Mizoram" or "Nagaland" or "Odisha" or "Punjab" or
        "Rajasthan" or "Sikkim" or "Tamil Nadu" or "Telangana" or "Tripura" or
        "Uttar Pradesh" or "Uttarakhand" or "West Bengal" or "Andaman and Nicobar Islands" or
        "Chandigarh" or "Dadra and Nagar Haveli and Daman and Diu" or "Delhi" or
        "Jammu and Kashmir" or "Ladakh" or "Lakshadweep" or "Puducherry";

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? NormalizeImageUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var url = value.Trim();

        if (url.Length > 500 ||
            !Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps &&
             uri.Scheme != Uri.UriSchemeHttp))
        {
            throw new ArgumentException(
                "Profile image must be a valid HTTP or HTTPS URL no longer than 500 characters.");
        }

        return url;
    }
}
