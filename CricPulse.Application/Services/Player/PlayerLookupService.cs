
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CricPulse.Application.DTOs.Player;
using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Application.Interfaces.Services;
using CricPulse.Domain.Entities;

namespace CricPulse.Application.Services.Player;
/// <summary>
/// Handles player lookup, onboarding, and roster-player validation
/// using mobile numbers and email addresses.
/// </summary>
public class PlayerLookupService
{
    private readonly IUserRepository _userRepository;
    private readonly IPlayerStatisticsRepository _statisticsRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionManager _transactionManager;

    public PlayerLookupService(
        IUserRepository userRepository,
        IPlayerStatisticsRepository statisticsRepository,
        IPasswordHasherService passwordHasher,
        IUnitOfWork unitOfWork,
        ITransactionManager transactionManager)
    {
        _userRepository = userRepository;
        _statisticsRepository = statisticsRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _transactionManager = transactionManager;
    }

    /// <summary>
    /// Finds an existing player using a mobile number, email address, or both.
    /// If both identifiers are supplied, they must belong to the same account.
    /// </summary>
    public async Task<PlayerLookupDto?> FindByIdentifierAsync(string? mobileNumber, string? email)
    {
        var mobile = NormalizeMobile(mobileNumber);
        var normalizedEmail = NormalizeEmail(email);

        if (mobile is null && normalizedEmail is null)
            throw new ArgumentException(
                "Enter a mobile number or email address.");

        var mobileUser = mobile is null
            ? null
            : await _userRepository.GetByMobileNumberAsync(mobile);

        var emailUser = normalizedEmail is null
            ? null
            : await _userRepository.GetByEmailAsync(normalizedEmail);

        if (mobileUser is not null &&
            emailUser is not null &&
            mobileUser.Id != emailUser.Id)
        {
            throw new InvalidOperationException(
                "The mobile number and email belong to different accounts.");
        }

        var user = mobileUser ?? emailUser;

        if (user is null)
            return null;

        if (!user.IsActive)
            throw new InvalidOperationException("This account is inactive.");

        return MapPlayer(user);
    }

    /// <summary>
    /// Creates a new player account and its statistics record after validating
    /// the player's contact details and ensuring they are not already registered.
    /// </summary>
    public async Task<PlayerLookupDto> OnboardAsync(OnboardPlayerDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var mobile = NormalizeMobile(dto.MobileNumber)
            ?? throw new ArgumentException("Mobile number is required.");

        var email = NormalizeEmail(dto.Email)
            ?? throw new ArgumentException("Email is required.");

        var firstName = dto.FirstName.Trim();
        var lastName = string.IsNullOrWhiteSpace(dto.LastName) ? null : dto.LastName.Trim();

        if (firstName.Length == 0)
            throw new ArgumentException("First name is required.");

        if (firstName.Length > 15)
            throw new ArgumentException(
                "First name cannot exceed 15 characters.");

        if (lastName?.Length > 15)
            throw new ArgumentException(
                "Last name cannot exceed 15 characters.");

        var existingMobile =
            await _userRepository.GetByMobileNumberAsync(mobile);

        var existingEmail =
            await _userRepository.GetByEmailAsync(email);

        if (existingMobile is not null || existingEmail is not null)
            throw new InvalidOperationException(
                "This mobile number or email already belongs to an account. Look up the existing player instead.");

        var now = DateTime.UtcNow;

        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = Capitalize(firstName),
            LastName = lastName is null ? null : Capitalize(lastName),
            MobileNumber = mobile,
            Email = email,
            // Generate a random password because umpire-onboarded players
            // will set their own password later through Forgot Password.
            PasswordHash = _passwordHasher.Hash(
                Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        // Create the player's statistics record along with their account.
        var statistics = new PlayerStatistics
        {
            UserId = user.Id
        };
        // Keep account creation and statistics creation in a single transaction.
        await _transactionManager.BeginTransactionAsync();

        try
        {
            await _userRepository.AddAsync(user);
            await _statisticsRepository.AddAsync(statistics);

            await _unitOfWork.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();

            return MapPlayer(user);
        }
        catch
        {
            await _transactionManager.RollbackTransactionAsync();
            throw;
        }
    }

    /// <summary>
    /// Converts a User entity into the player lookup response DTO.
    /// </summary>
    private static PlayerLookupDto MapPlayer(User user)
    {
        return new PlayerLookupDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName ?? string.Empty,
            MobileNumber = user.MobileNumber,
            Email = user.Email,
        };
    }

    /// <summary>
    /// Trims and validates an Indian 10-digit mobile number.
    /// Returns null when no mobile number is supplied.
    /// </summary>
    private static string? NormalizeMobile(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var mobile = value.Trim();

        if (!Regex.IsMatch(mobile, @"^[6-9]\d{9}$"))
            throw new ArgumentException(
                "Mobile number must contain exactly 10 digits and start with 6, 7, 8 or 9.");

        return mobile;
    }

    /// <summary>
    /// Trims and converts an email address to lowercase before validating it.
    /// Returns null when no email address is supplied.
    /// </summary>
    private static string? NormalizeEmail(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var email = value.Trim().ToLowerInvariant();

        if (email.Length > 255 ||
            !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new ArgumentException("Please provide a valid email address.");
        }

        return email;
    }

    /// <summary>
    /// Capitalizes the first character of a name while preserving the remaining characters.
    /// </summary>
    private static string Capitalize(string value)
    {
        return value.Length == 1
            ? value.ToUpperInvariant()
            : char.ToUpperInvariant(value[0]) + value[1..];
    }

    /// <summary>
    /// Validates a player identifier before adding the player to a match roster.
    /// Checks account status, organizer identity, and duplicates across both teams.
    /// Returns null when the identifier does not belong to a registered user,
    /// allowing the frontend to offer player onboarding.
    /// </summary>
    public async Task<PlayerLookupDto?> ValidateRosterPlayerAsync(ValidateRosterPlayerDto dto, Guid createdByUserId)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (string.IsNullOrWhiteSpace(dto.Identifier))
            throw new ArgumentException(
                "Enter a mobile number or email address.");

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Invalid creator user ID.");

        var organizer = await _userRepository.GetByIdAsync(createdByUserId);

        if (organizer is null || !organizer.IsActive)
            throw new InvalidOperationException(
                "A valid active user account is required.");

        var identifier = dto.Identifier.Trim();

        User? user;

        if (identifier.Contains('@'))
        {
            var email = NormalizeEmail(identifier)
                ?? throw new ArgumentException(
                    "Enter a valid email address.");

            user = await _userRepository.GetByEmailAsync(email);
        }
        else
        {
            var mobile = NormalizeMobile(identifier)
                ?? throw new ArgumentException(
                    "Enter a valid mobile number.");

            user = await _userRepository.GetByMobileNumberAsync(mobile);
        }

        // An unregistered identifier allows the frontend to offer onboarding.
        if (user is null)
            return null;

        if (!user.IsActive)
            throw new InvalidOperationException(
                "This account is inactive and cannot be added to a match.");

        // Organizing and playing are actions, not mutually exclusive account roles.
        if (user.Id == createdByUserId)
            throw new InvalidOperationException(
                "The match organizer cannot join their own match as a player.");

        // Check IDs of players already resolved in either team.
        if (dto.ExistingUserIds?.Contains(user.Id) == true)
            throw new InvalidOperationException(
                "This player has already been added to Team A or Team B.");

        // Also check identifiers in other slots, including unresolved slots.
        foreach (var existingIdentifier in dto.ExistingIdentifiers ?? [])
        {
            if (string.IsNullOrWhiteSpace(existingIdentifier))
                continue;

            var value = existingIdentifier.Trim();
            User? existingUser;

            try
            {
                if (value.Contains('@'))
                {
                    var email = NormalizeEmail(value);

                    if (email is null)
                        continue;

                    existingUser = await _userRepository
                        .GetByEmailAsync(email);
                }
                else
                {
                    var mobile = NormalizeMobile(value);

                    if (mobile is null)
                        continue;

                    existingUser = await _userRepository
                        .GetByMobileNumberAsync(mobile);
                }
            }
            catch (ArgumentException)
            {
                // Ignore incomplete or malformed values in other slots.
                continue;
            }

            if (existingUser?.Id == user.Id)
                throw new InvalidOperationException(
                    "This player has already been added to Team A or Team B.");
        }

        return MapPlayer(user);
    }

}
