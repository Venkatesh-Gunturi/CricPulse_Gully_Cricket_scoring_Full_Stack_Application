using System.Text.RegularExpressions;
using System.Security.Cryptography;
using CricPulse.Application.DTOs.Auth;
using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Application.Interfaces.Services;
using CricPulse.Domain.Entities;

namespace CricPulse.Application.Services.Auth;
// Handles user registration, login, input validation, and authentication response mapping.
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPlayerStatisticsRepository _playerStatisticsRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionManager _transactionManager;

    public AuthService(
        IUserRepository userRepository,
        IPlayerStatisticsRepository playerStatisticsRepository,
        IPasswordHasherService passwordHasher,
        IUnitOfWork unitOfWork,
        ITransactionManager transactionManager)
    {
        _userRepository = userRepository;
        _playerStatisticsRepository = playerStatisticsRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
        _transactionManager = transactionManager;
    }

    // Validates registration details, creates the user and their initial player statistics,
    // and saves both records within a single database transaction.
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // Validate the supplied registration details before creating the account.
        ValidateRegistration(dto);
        // Normalize user input to maintain consistent data formats in the database.
        var firstName = NormalizeName(dto.FirstName);
        var lastName = NormalizeOptionalName(dto.LastName);
        var mobileNumber = NormalizeMobileNumber(dto.MobileNumber);
        var email = NormalizeEmail(dto.Email);
        // Prevent registration with a mobile number that is already in use.
        if (await _userRepository.ExistsByMobileNumberAsync(mobileNumber))
        {
            throw new InvalidOperationException(
                "An account with this mobile number already exists.");
        }
        // Prevent registration with an email address that is already in use.
        if (await _userRepository.ExistsByEmailAsync(email))
        {
            throw new InvalidOperationException(
                "An account with this email already exists.");
        }
        // Create the user entity with normalized details and a securely hashed password.
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            MobileNumber = mobileNumber,
            Email = email,
            PasswordHash = _passwordHasher.Hash(
                string.IsNullOrWhiteSpace(dto.Password)
                    ? Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                    : dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var playerStatistics = new PlayerStatistics
        {
            UserId = user.Id
        };

        // Keep user creation and initial statistics creation in the same transaction.
        await _transactionManager.BeginTransactionAsync();
        // Add both entities before committing them together.
        try
        {
            await _userRepository.AddAsync(user);
            await _playerStatisticsRepository.AddAsync(playerStatistics);

            // Persist the pending changes before committing the transaction.
            await _unitOfWork.SaveChangesAsync();
            // Commit only after both records have been saved successfully.
            await _transactionManager.CommitTransactionAsync();
        }
        catch
        {
            // Roll back the transaction if any registration operation fails.
            await _transactionManager.RollbackTransactionAsync();
            throw;
        }

        return MapToResponse(user);
    }

    // Authenticates a user using their registered mobile number or email and password.
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // Ensure the login identifier is provided.
        if (string.IsNullOrWhiteSpace(dto.LoginIdentifier))
        {
            throw new ArgumentException(
                "Mobile number or email is required.");
        }
        // Ensure a password is provided before attempting authentication.
        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new ArgumentException("Password is required.");
        }
        // Trim whitespace and normalize email casing before looking up the user.
        var loginIdentifier = dto.LoginIdentifier.Trim();
        // Reject inactive users, unknown accounts, and incorrect passwords.
        if (loginIdentifier.Contains('@'))
        {
            loginIdentifier = loginIdentifier.ToLowerInvariant();
        }

        var user = await _userRepository
            .GetByLoginIdentifierAsync(loginIdentifier);

        if (user is null ||
            !user.IsActive ||
            !_passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            throw new InvalidOperationException(
                "Invalid mobile/email or password.");
        }

        return MapToResponse(user);
    }
    // Validates registration fields, including names, mobile number, email, and optional password.
    private static void ValidateRegistration(RegisterDto dto)
    {
        // Validates a name field and enforces whether it is required and its maximum length.
        ValidateName(
            dto.FirstName,
            "First name",
            required: true);

        ValidateName(
            dto.LastName,
            "Last name",
            required: false);

        NormalizeMobileNumber(dto.MobileNumber);

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new ArgumentException("Email is required.");
        }
        // Validate the email format and enforce the database length constraint.
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

        // Password is optional for the user-ID-based prototype flow. If a
        // password is supplied, validate it fully; otherwise a random secret
        // is hashed so the database never stores a blank password hash.
        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            ValidatePassword(dto.Password);
            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                throw new ArgumentException("Confirm password is required when setting a password.");
            if (dto.Password != dto.ConfirmPassword)
                throw new ArgumentException("Password and confirm password do not match.");
        }
        else if (!string.IsNullOrWhiteSpace(dto.ConfirmPassword))
        {
            throw new ArgumentException("Enter a password before confirming it.");
        }
    }
    // Validates a name field and enforces whether it is required and its maximum length.
    private static void ValidateName(string? value, string fieldName, bool required)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
            {
                throw new ArgumentException(
                    $"{fieldName} is required.");
            }

            return;
        }

        var name = value.Trim();

        if (name.Length > 15)
        {
            throw new ArgumentException(
                $"{fieldName} cannot exceed 15 characters.");
        }
    }
    // Trims the name and capitalizes its first alphabetic character.
    private static string NormalizeName(string name)
    {
        var normalized = name.Trim();

        if (normalized.Length == 0)
        {
            return normalized;
        }

        return CapitalizeFirstLetter(normalized);
    }
    // Normalizes an optional name, returning null when no value is provided.
    private static string? NormalizeOptionalName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return CapitalizeFirstLetter(name.Trim());
    }
    // Capitalizes the first alphabetic character without changing the remaining characters.
    private static string CapitalizeFirstLetter(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            if (!char.IsLetter(value[i]))
            {
                continue;
            }

            return value[..i]
                + char.ToUpper(value[i])
                + value[(i + 1)..];
        }

        return value;
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
    // Maps the user entity to the authentication response DTO without exposing sensitive fields.
    private static AuthResponseDto MapToResponse(User user)
    {
        return new AuthResponseDto
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
    }
}