using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CricPulse.Application.DTOs.Match;
using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Application.Interfaces.Services;
using CricPulse.Domain.Entities;
using MatchEntity = CricPulse.Domain.Entities.Match;
namespace CricPulse.Application.Services.Match;

public class MatchService : IMatchService
{
    private readonly IUserRepository _userRepository;
    private readonly IPlayerStatisticsRepository _playerStatisticsRepository;
    private readonly IOrganizerStatisticsRepository _organizerStatisticsRepository;
    private readonly IMatchRepository _matchRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionManager _transactionManager;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IReverseGeocodingService _reverseGeocodingService;

    public MatchService(
        IUserRepository userRepository,
        IPlayerStatisticsRepository playerStatisticsRepository,
        IOrganizerStatisticsRepository organizerStatisticsRepository,
        IMatchRepository matchRepository,
        IUnitOfWork unitOfWork,
        ITransactionManager transactionManager,
        IPasswordHasherService passwordHasher,
        IReverseGeocodingService reverseGeocodingService)
    {
        _userRepository = userRepository;
        _playerStatisticsRepository = playerStatisticsRepository;
        _organizerStatisticsRepository = organizerStatisticsRepository;
        _matchRepository = matchRepository;
        _unitOfWork = unitOfWork;
        _transactionManager = transactionManager;
        _passwordHasher = passwordHasher;
        _reverseGeocodingService = reverseGeocodingService;
    }


    public async Task<MatchCreatedDto> CreateMatchAsync(CreateMatchDto dto)
    {
        ValidateMatch(dto);

        var organizer = await _userRepository.GetByIdAsync(dto.CreatedByUserId);

        if (organizer is null || !organizer.IsActive)
            throw new InvalidOperationException(
                "Organizer account was not found.");

        var organizerStatistics =
            await _organizerStatisticsRepository.GetByUserIdAsync(dto.CreatedByUserId);
        var createOrganizerStatistics = organizerStatistics is null;
        organizerStatistics ??= new OrganizerStatistics { UserId = dto.CreatedByUserId };

        ValidatePlayers(dto);

        // Check the organizer's stored contact details directly before resolving
        // roster entries. This does not depend on repository lookups matching
        // the database's exact mobile/email formatting.
        ValidateCreatorIsNotInRoster(dto.Players, organizer);

        var now = DateTime.UtcNow;

        var resolvedLocation =
            await _reverseGeocodingService.ResolveAsync(
                dto.Latitude,
                dto.Longitude);

        if (resolvedLocation is null ||
            string.IsNullOrWhiteSpace(resolvedLocation.State))
        {
            throw new InvalidOperationException(
                "Unable to identify the state from the supplied match coordinates. Please try again.");
        }

        var normalizedState = IndianStatesAndUnionTerritories
            .FirstOrDefault(x => string.Equals(
                x,
                resolvedLocation.State.Trim(),
                StringComparison.OrdinalIgnoreCase));

        if (normalizedState is null)
        {
            throw new InvalidOperationException(
                $"The location provider returned an unrecognized Indian state or Union Territory: {resolvedLocation.State}");
        }

        var match = new MatchEntity
        {
            Id = Guid.NewGuid(),
            CreatedByUserId = dto.CreatedByUserId,
            Name = dto.Name.Trim(),
            TeamAName = dto.TeamAName.Trim(),
            TeamALogo = NormalizeTeamLogo(dto.TeamALogo, "Team A"),
            TeamBName = dto.TeamBName.Trim(),
            TeamBLogo = NormalizeTeamLogo(dto.TeamBLogo, "Team B"),
            ScheduledAt = dto.ScheduledAt,
            Venue = dto.Venue.Trim(),
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            LocationAddress = resolvedLocation.Address,
            State = normalizedState,
            PinCode = resolvedLocation.PinCode,
            Overs = dto.Overs,
            PlayersPerTeam = dto.PlayersPerTeam,
            Status = "Upcoming",
            CreatedAt = now,
            UpdatedAt = now
        };

        await _transactionManager.BeginTransactionAsync();

        try
        {
            var resolvedPlayers = new List<ResolvedPlayer>();

            foreach (var playerDto in dto.Players)
            {
                var resolvedPlayer = await ResolvePlayerAsync(
                    dto.CreatedByUserId,
                    playerDto);

                resolvedPlayers.Add(resolvedPlayer);
            }

            // Explicitly prevent the match organizer from occupying a player slot.
            if (resolvedPlayers.Any(
                player => player.UserId == dto.CreatedByUserId))
            {
                throw new InvalidOperationException(
                    "The match organizer cannot be listed as a player in their own match.");
            }

            // A registered user can occupy only one roster slot in the match.
            var duplicateUserIds = resolvedPlayers
                .GroupBy(player => player.UserId)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateUserIds.Count > 0)
            {
                throw new InvalidOperationException(
                    "A player account can appear only once in a match, on one team only.");
            }

            // Retain the existing team-size and team-validation checks.
            ValidateResolvedPlayers(dto, resolvedPlayers);

            for (var i = 0; i < resolvedPlayers.Count; i++)
            {
                var resolved = resolvedPlayers[i];

                var teamPlayers = resolvedPlayers
                    .Where(player => player.Team == resolved.Team)
                    .ToList();

                var isCaptain = teamPlayers[0] == resolved;

                match.MatchPlayers.Add(
                    new MatchPlayer
                    {
                        Id = Guid.NewGuid(),
                        MatchId = match.Id,
                        UserId = resolved.UserId,
                        Team = resolved.Team,
                        IsCaptain = isCaptain,
                        CreatedAt = now
                    });
            }

            await _matchRepository.AddAsync(match);

            organizerStatistics.MatchesCreated++;
            if (createOrganizerStatistics)
                await _organizerStatisticsRepository.AddAsync(organizerStatistics);
            else
                await _organizerStatisticsRepository.UpdateAsync(organizerStatistics);

            await _unitOfWork.SaveChangesAsync();

            await _transactionManager.CommitTransactionAsync();

            return MapToDto(match, resolvedPlayers);
        }
        catch
        {
            await _transactionManager.RollbackTransactionAsync();
            throw;
        }
    }


    private async Task<ResolvedPlayer> ResolvePlayerAsync(
        Guid createdByUserId,
        MatchPlayerInputDto dto)
    {
        var mobile =
            NormalizeMobile(dto.MobileNumber);

        var email =
            NormalizeEmail(dto.Email);

        if (mobile is null && email is null)
            throw new ArgumentException(
                "Mobile number or email is required for every player.");

        User? mobileUser = null;
        User? emailUser = null;

        if (mobile is not null)
        {
            mobileUser =
                await _userRepository
                    .GetByMobileNumberAsync(mobile);
        }

        if (email is not null)
        {
            emailUser =
                await _userRepository
                    .GetByEmailAsync(email);
        }

        // When both contacts are supplied, they must either both be unused
        // (new player) or both resolve to the same existing account. Never
        // attach an unknown contact to an account found through the other one.
        if (mobile is not null &&
            email is not null &&
            ((mobileUser is null) != (emailUser is null)))
        {
            throw new InvalidOperationException(
                "One contact detail already belongs to an account while the other does not. Use the matching account details; a new account will not be linked automatically.");
        }

        if (mobileUser is not null &&
            emailUser is not null &&
            mobileUser.Id != emailUser.Id)
        {
            throw new InvalidOperationException(
                "The mobile number and email belong to different registered accounts.");
        }

        var existingUser =
            mobileUser ?? emailUser;

        if (existingUser is not null)
        {
            // A match organizer cannot also occupy a player slot in that match.
            if (existingUser.Id == createdByUserId)
            {
                throw new InvalidOperationException(
                    "The match organizer cannot be listed as a player in their own match.");
            }

            if (!existingUser.IsActive)
            {
                throw new InvalidOperationException(
                    "This player account is inactive and cannot be added to a match.");
            }

            return new ResolvedPlayer
            {
                UserId = existingUser.Id,
                PlayerName = BuildPlayerName(existingUser),
                Team = NormalizeTeam(dto.Team)
            };
        }

        if (mobile is null || email is null)
        {
            throw new InvalidOperationException(
                "This player is not registered. Both mobile number and email are required to create a new player.");
        }

        ValidateNewPlayerName(dto.FirstName, dto.LastName);

        var password = GenerateTemporaryPassword();

        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = NormalizeName(dto.FirstName!),
            LastName = NormalizeOptionalName(dto.LastName),
            MobileNumber = mobile,
            Email = email,
            PasswordHash = _passwordHasher.Hash(password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var playerStatistics = new PlayerStatistics
        {
            UserId = newUser.Id
        };

        // A roster participant is a normal User account. No Player or organizer
        // profile row is created or required.
        await _userRepository.AddAsync(newUser);
        await _playerStatisticsRepository.AddAsync(playerStatistics);

        await _unitOfWork.SaveChangesAsync();

        return new ResolvedPlayer
        {
            UserId = newUser.Id,
            PlayerName = BuildPlayerName(newUser),
            Team = NormalizeTeam(dto.Team)
        };
    }

    private static void ValidateMatch(CreateMatchDto dto)
    {
        if (dto.CreatedByUserId == Guid.Empty)
            throw new ArgumentException(
                "Creator user ID is required.");

        if (string.IsNullOrWhiteSpace(dto.Name))
            throw new ArgumentException(
                "Match name is required.");

        if (dto.Name.Trim().Length > 200)
            throw new ArgumentException(
                "Match name cannot exceed 200 characters.");

        if (string.IsNullOrWhiteSpace(dto.TeamAName))
            throw new ArgumentException(
                "Team A name is required.");

        if (dto.TeamAName.Trim().Length > 100)
            throw new ArgumentException(
                "Team A name cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(dto.TeamBName))
            throw new ArgumentException(
                "Team B name is required.");

        if (dto.TeamBName.Trim().Length > 100)
            throw new ArgumentException(
                "Team B name cannot exceed 100 characters.");

        if (string.Equals(
                dto.TeamAName.Trim(),
                dto.TeamBName.Trim(),
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "Team A and Team B must have different names.");
        }

        // Persist stable keys from the predefined CricPulse logo collection;
        // arbitrary image URLs are deliberately rejected.
        _ = NormalizeTeamLogo(dto.TeamALogo, "Team A");
        _ = NormalizeTeamLogo(dto.TeamBLogo, "Team B");

        if (string.IsNullOrWhiteSpace(dto.Venue))
            throw new ArgumentException(
                "Venue is required.");

        if (dto.Venue.Trim().Length > 200)
            throw new ArgumentException(
                "Venue cannot exceed 200 characters.");

        if (dto.ScheduledAt == default)
            throw new ArgumentException(
                "Match date and time are required.");

        if (dto.ScheduledAt <= DateTime.UtcNow)
        {
            throw new ArgumentException(
                "Match date and time must be in the future.");
        }

        if (dto.Overs < 1 || dto.Overs > 90)
            throw new ArgumentException(
                "Overs must be between 1 and 90.");

        if (dto.PlayersPerTeam < 4 ||
            dto.PlayersPerTeam > 11)
        {
            throw new ArgumentException(
                "Players per team must be between 4 and 11.");
        }

        if (dto.Latitude is < -90 or > 90)
            throw new ArgumentException(
                "Invalid latitude.");

        if (dto.Longitude is < -180 or > 180)
            throw new ArgumentException(
                "Invalid longitude.");

        if (!string.IsNullOrWhiteSpace(dto.LocationAddress) &&
            dto.LocationAddress.Trim().Length > 500)
        {
            throw new ArgumentException(
                "Address cannot exceed 500 characters.");
        }
    }

    private static void ValidateCreatorIsNotInRoster(
        IEnumerable<MatchPlayerInputDto> players,
        User organizer)
    {
        var organizerMobile = NormalizeStoredMobile(organizer.MobileNumber);
        var organizerEmail = string.IsNullOrWhiteSpace(organizer.Email)
            ? null
            : organizer.Email.Trim().ToLowerInvariant();

        foreach (var player in players)
        {
            var mobile = NormalizeMobile(player.MobileNumber);
            var email = NormalizeEmail(player.Email);

            var matchesOrganizerMobile =
                mobile is not null &&
                organizerMobile is not null &&
                string.Equals(mobile, organizerMobile, StringComparison.Ordinal);

            var matchesOrganizerEmail =
                email is not null &&
                organizerEmail is not null &&
                string.Equals(email, organizerEmail, StringComparison.OrdinalIgnoreCase);

            if (matchesOrganizerMobile || matchesOrganizerEmail)
            {
                throw new InvalidOperationException(
                    "The match organizer cannot be listed as a player in their own match.");
            }
        }
    }

    private static string? NormalizeStoredMobile(string? mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile))
            return null;

        var digits = Regex.Replace(mobile, @"\D", string.Empty);

        // Accept stored values such as +91XXXXXXXXXX while comparing them
        // with the application's 10-digit Indian mobile input.
        if (digits.Length == 12 && digits.StartsWith("91", StringComparison.Ordinal))
            digits = digits[2..];

        return digits.Length == 10 ? digits : null;
    }

    private static void ValidatePlayers(CreateMatchDto dto)
    {
        var expectedCount =
            dto.PlayersPerTeam * 2;

        if (dto.Players is null ||
            dto.Players.Count != expectedCount)
        {
            throw new ArgumentException(
                $"Exactly {expectedCount} players are required.");
        }

        var teamA = dto.Players
            .Where(x => NormalizeTeam(x.Team) == "TeamA")
            .ToList();

        var teamB = dto.Players
            .Where(x => NormalizeTeam(x.Team) == "TeamB")
            .ToList();

        if (teamA.Count != dto.PlayersPerTeam)
        {
            throw new ArgumentException(
                $"Team A must contain exactly {dto.PlayersPerTeam} players.");
        }

        if (teamB.Count != dto.PlayersPerTeam)
        {
            throw new ArgumentException(
                $"Team B must contain exactly {dto.PlayersPerTeam} players.");
        }

        // Catch repeated contact details before performing database lookups.
        // This is an early check only; the resolved UserId check below is the
        // authoritative identity check (email and mobile can point to one account).
        var mobileNumbers = dto.Players
            .Select(x => NormalizeMobile(x.MobileNumber))
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        if (mobileNumbers.Count != mobileNumbers.Distinct(StringComparer.Ordinal).Count())
        {
            throw new InvalidOperationException(
                "The same mobile number cannot appear in more than one roster slot.");
        }

        var emails = dto.Players
            .Select(x => NormalizeEmail(x.Email))
            .Where(x => x is not null)
            .Select(x => x!)
            .ToList();

        if (emails.Count != emails.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            throw new InvalidOperationException(
                "The same email address cannot appear in more than one roster slot.");
        }
    }

    private static void ValidateResolvedPlayers(
        CreateMatchDto dto,
        List<ResolvedPlayer> players)
    {
        var duplicatePlayers = players
            .GroupBy(x => x.UserId)
            .Any(x => x.Count() > 1);

        if (duplicatePlayers)
            throw new InvalidOperationException(
                "A player account can appear only once in a match, on one team only.");

        var teamA = players
            .Where(x => x.Team == "TeamA")
            .ToList();

        var teamB = players
            .Where(x => x.Team == "TeamB")
            .ToList();

        if (teamA.Count != dto.PlayersPerTeam ||
            teamB.Count != dto.PlayersPerTeam)
        {
            throw new InvalidOperationException(
                "Both teams must contain the selected number of players.");
        }
    }

    private static string NormalizeTeam(string team)
    {
        if (string.IsNullOrWhiteSpace(team))
            throw new ArgumentException(
                "Player team is required.");

        return team.Trim().ToLowerInvariant() switch
        {
            "teama" => "TeamA",
            "team a" => "TeamA",
            "teamb" => "TeamB",
            "team b" => "TeamB",
            _ => throw new ArgumentException(
                "Player team must be TeamA or TeamB.")
        };
    }

    private static string? NormalizeMobile(string? mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile))
            return null;

        var value = mobile.Trim();

        if (!Regex.IsMatch(value, @"^[6-9]\d{9}$"))
            throw new ArgumentException(
                "Mobile number must contain exactly 10 digits and start with 6, 7, 8 or 9.");

        return value;
    }

    private static string? NormalizeEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var value = email.Trim().ToLowerInvariant();

        if (value.Length > 255 ||
            !Regex.IsMatch(
                value,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
        {
            throw new ArgumentException(
                "Please provide a valid email address.");
        }

        return value;
    }

    private static void ValidateNewPlayerName(
        string? firstName,
        string? lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException(
                "First name is required for a new player.");

        if (firstName.Trim().Length > 15)
            throw new ArgumentException(
                "First name cannot exceed 15 characters.");

        if (!string.IsNullOrWhiteSpace(lastName) &&
            lastName.Trim().Length > 15)
        {
            throw new ArgumentException(
                "Last name cannot exceed 15 characters.");
        }
    }

    private static string NormalizeName(string value)
    {
        var trimmed = value.Trim();

        if (trimmed.Length == 1)
            return trimmed.ToUpperInvariant();

        return char.ToUpperInvariant(trimmed[0]) +
               trimmed[1..];
    }

    private static string? NormalizeOptionalName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return NormalizeName(value);
    }

    private static string BuildPlayerName(User user)
    {
        return string.Join(
            " ",
            new[]
            {
                user.FirstName,
                user.LastName
            }
            .Where(x => !string.IsNullOrWhiteSpace(x)));
    }

    private static readonly string[] PredefinedTeamLogoKeys =
    {
        "crest-blue",
        "crest-red",
        "crest-green",
        "crest-purple",
        "crest-orange",
        "crest-black",
        "crest-gold",
        "crest-teal"
    };

    private static string NormalizeTeamLogo(
        string? value,
        string teamLabel)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(
                $"Select a predefined logo for {teamLabel}.");
        }

        var normalized = PredefinedTeamLogoKeys.FirstOrDefault(
            key => string.Equals(
                key,
                value.Trim(),
                StringComparison.OrdinalIgnoreCase));

        if (normalized is null)
        {
            throw new ArgumentException(
                $"{teamLabel} logo must be selected from the predefined CricPulse logo collection.");
        }

        return normalized;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static string GenerateTemporaryPassword()
    {
        return Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));
    }

    private sealed class ResolvedPlayer
    {
        public Guid UserId { get; set; }

        public string PlayerName { get; set; } = string.Empty;

        public string Team { get; set; } = string.Empty;
    }

    private static MatchCreatedDto MapToDto(
    MatchEntity match,
    List<ResolvedPlayer> players)
    {
        var playerLookup = players
            .ToDictionary(x => x.UserId);

        return new MatchCreatedDto
        {
            MatchId = match.Id,
            CreatedByUserId = match.CreatedByUserId,
            Name = match.Name,
            TeamAName = match.TeamAName,
            TeamALogo = match.TeamALogo,
            TeamBName = match.TeamBName,
            TeamBLogo = match.TeamBLogo,
            ScheduledAt = match.ScheduledAt,
            Venue = match.Venue,
            LocationAddress = match.LocationAddress,
            State = match.State,
            PinCode = match.PinCode,
            Latitude = match.Latitude,
            Longitude = match.Longitude,
            Overs = match.Overs,
            PlayersPerTeam = match.PlayersPerTeam,
            Status = match.Status,
            Players = match.MatchPlayers
                .OrderBy(x => x.Team)
                .ThenBy(x => x.CreatedAt)
                .Select(x =>
                {
                    var player =
                        playerLookup[x.UserId];

                    return new CreatedMatchPlayerDto
                    {
                        MatchPlayerId = x.Id,
                        UserId = x.UserId,
                        PlayerName = player.PlayerName,
                        Team = x.Team,
                        IsCaptain = x.IsCaptain
                    };
                })
                .ToList()
        };
    }

    private static readonly string[] IndianStatesAndUnionTerritories =
{
    "Andhra Pradesh",
    "Arunachal Pradesh",
    "Assam",
    "Bihar",
    "Chhattisgarh",
    "Goa",
    "Gujarat",
    "Haryana",
    "Himachal Pradesh",
    "Jharkhand",
    "Karnataka",
    "Kerala",
    "Madhya Pradesh",
    "Maharashtra",
    "Manipur",
    "Meghalaya",
    "Mizoram",
    "Nagaland",
    "Odisha",
    "Punjab",
    "Rajasthan",
    "Sikkim",
    "Tamil Nadu",
    "Telangana",
    "Tripura",
    "Uttar Pradesh",
    "Uttarakhand",
    "West Bengal",
    "Andaman and Nicobar Islands",
    "Chandigarh",
    "Dadra and Nagar Haveli and Daman and Diu",
    "Delhi",
    "Jammu and Kashmir",
    "Ladakh",
    "Lakshadweep",
    "Puducherry"
};

    public Task<IReadOnlyList<string>> GetIndianStatesAsync()
    {
        return Task.FromResult<IReadOnlyList<string>>(
            IndianStatesAndUnionTerritories);
    }

    public async Task<IReadOnlyList<MatchDiscoveryDto>> GetMatchesAsync(
        MatchDiscoveryQueryDto query)
    {
        ArgumentNullException.ThrowIfNull(query);

        string? status = null;

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            status = query.Status.Trim();

            var allowedStatuses = new[]
            {
            "Upcoming", "Live", "Finished", "Cancelled"
        };

            status = allowedStatuses.FirstOrDefault(
                x => string.Equals(
                    x, status, StringComparison.OrdinalIgnoreCase));

            if (status is null)
                throw new ArgumentException("Invalid match status.");
        }

        string? state = null;

        if (!string.IsNullOrWhiteSpace(query.State))
        {
            state = IndianStatesAndUnionTerritories.FirstOrDefault(
                x => string.Equals(
                    x, query.State.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (state is null)
                throw new ArgumentException(
                    "Select a valid Indian state or Union Territory.");
        }

        var hasLatitude = query.Latitude.HasValue;
        var hasLongitude = query.Longitude.HasValue;

        if (hasLatitude != hasLongitude)
            throw new ArgumentException(
                "Latitude and longitude must be provided together.");

        var useNearby = hasLatitude && hasLongitude;

        if (useNearby &&
            (query.Latitude is < -90 or > 90 ||
             query.Longitude is < -180 or > 180))
        {
            throw new ArgumentException("Invalid coordinates.");
        }

        if (query.RadiusKm <= 0 || query.RadiusKm > 30)
            throw new ArgumentException(
                "Radius must be greater than 0 and at most 30 km.");

        double? minLat = null;
        double? maxLat = null;
        double? minLon = null;
        double? maxLon = null;

        if (useNearby)
        {
            var lat = query.Latitude!.Value;
            var lon = query.Longitude!.Value;
            var latDelta = query.RadiusKm / 110.574;
            var cosLat = Math.Abs(Math.Cos(lat * Math.PI / 180));

            minLat = Math.Max(-90, lat - latDelta);
            maxLat = Math.Min(90, lat + latDelta);

            var lonDelta = cosLat < 0.000001
                ? 180
                : Math.Min(180, query.RadiusKm / (111.320 * cosLat));

            minLon = Math.Max(-180, lon - lonDelta);
            maxLon = Math.Min(180, lon + lonDelta);
        }

        var matches = await _matchRepository.GetForDiscoveryAsync(
            status, state, minLat, maxLat, minLon, maxLon);

        var results = matches
            .Select(match =>
            {
                double? distance = useNearby
                    ? CalculateDistanceKm(
                        query.Latitude!.Value,
                        query.Longitude!.Value,
                        match.Latitude,
                        match.Longitude)
                    : null;

                return new { Match = match, Distance = distance };
            })
            .Where(x => !useNearby ||
                        x.Distance!.Value <= query.RadiusKm)
            .OrderBy(x => useNearby ? x.Distance : null)
            .ThenBy(x => x.Match.ScheduledAt)
            .Select(x =>
            {
                var match = x.Match;
                var showLocation =
                    match.Status is "Upcoming" or "Live";

                var lat = match.Latitude.ToString(
                    System.Globalization.CultureInfo.InvariantCulture);

                var lon = match.Longitude.ToString(
                    System.Globalization.CultureInfo.InvariantCulture);

                return new MatchDiscoveryDto
                {
                    MatchId = match.Id,
                    Name = match.Name,
                    TeamAName = match.TeamAName,
                    TeamALogo = match.TeamALogo,
                    TeamBName = match.TeamBName,
                    TeamBLogo = match.TeamBLogo,
                    ScheduledAt = match.ScheduledAt,
                    Status = match.Status,
                    Overs = match.Overs,
                    PlayersPerTeam = match.PlayersPerTeam,
                    Venue = match.Venue,
                    LocationAddress = match.LocationAddress,
                    State = match.State,
                    Latitude = showLocation ? match.Latitude : null,
                    Longitude = showLocation ? match.Longitude : null,
                    MapsUrl = showLocation
                        ? $"https://www.google.com/maps/dir/?api=1&destination={lat},{lon}"
                        : null,
                    DistanceKm = x.Distance is null
                        ? null
                        : Math.Round(x.Distance.Value, 2),
                    WinnerTeam = match.WinnerTeam,
                    ResultText = match.ResultText,
                    Innings = match.Innings
                        .OrderBy(i => i.InningsNumber)
                        .Select(i => new MatchInningsSummaryDto
                        {
                            InningsNumber = i.InningsNumber,
                            BattingTeam = i.BattingTeam,
                            TotalRuns = i.TotalRuns,
                            Wickets = i.Wickets,
                            LegalBalls = i.LegalBalls,
                            TotalOvers = i.TotalOvers,
                            Status = i.Status
                        })
                        .ToList()
                };
            })
            .ToList();

        return results;
    }

    private static double CalculateDistanceKm(
        double latitude1,
        double longitude1,
        double latitude2,
        double longitude2)
    {
        const double earthRadiusKm = 6371.0088;

        var lat1 = latitude1 * Math.PI / 180;
        var lat2 = latitude2 * Math.PI / 180;
        var deltaLat = (latitude2 - latitude1) * Math.PI / 180;
        var deltaLon = (longitude2 - longitude1) * Math.PI / 180;

        var a =
            Math.Pow(Math.Sin(deltaLat / 2), 2) +
            Math.Cos(lat1) * Math.Cos(lat2) *
            Math.Pow(Math.Sin(deltaLon / 2), 2);

        return earthRadiusKm *
            2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }


    public async Task<IReadOnlyList<MatchDiscoveryDto>> GetMyMatchesAsync(
        Guid createdByUserId,
        string? status)
    {
        if (createdByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "organizer user ID is required.");
        }

        string? normalizedStatus = null;

        if (!string.IsNullOrWhiteSpace(status))
        {
            var allowedStatuses = new[]
            {
            "Upcoming",
            "Live",
            "Finished",
            "Cancelled"
        };

            normalizedStatus = allowedStatuses.FirstOrDefault(
                x => string.Equals(
                    x,
                    status.Trim(),
                    StringComparison.OrdinalIgnoreCase));

            if (normalizedStatus is null)
            {
                throw new ArgumentException(
                    "Invalid match status.");
            }
        }

        var matches = await _matchRepository.GetByCreatorAsync(
            createdByUserId,
            normalizedStatus);

        return matches.Select(match => new MatchDiscoveryDto
        {
            MatchId = match.Id,
            Name = match.Name,
            TeamAName = match.TeamAName,
            TeamALogo = match.TeamALogo,
            TeamBName = match.TeamBName,
            TeamBLogo = match.TeamBLogo,
            ScheduledAt = match.ScheduledAt,
            Status = match.Status,
            PlayersPerTeam = match.PlayersPerTeam,
            CancellationReasonType = match.CancellationReasonType,
            CancellationReason = match.CancellationReason,
            Overs = match.Overs,
            Venue = match.Venue,
            LocationAddress = match.LocationAddress,
            State = match.State,
            Latitude = match.Status is "Upcoming" or "Live"
                ? match.Latitude
                : null,
            Longitude = match.Status is "Upcoming" or "Live"
                ? match.Longitude
                : null,
            MapsUrl = match.Status is "Upcoming" or "Live"
                ? $"https://www.google.com/maps/dir/?api=1&destination={match.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{match.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
                : null,
            WinnerTeam = match.WinnerTeam,
            ResultText = match.ResultText,
            Innings = match.Innings
                .OrderBy(i => i.InningsNumber)
                .Select(i => new MatchInningsSummaryDto
                {
                    InningsNumber = i.InningsNumber,
                    BattingTeam = i.BattingTeam,
                    TotalRuns = i.TotalRuns,
                    Wickets = i.Wickets,
                    LegalBalls = i.LegalBalls,
                    TotalOvers = i.TotalOvers,
                    Status = i.Status
                })
                .ToList()
        }).ToList();
    }



    public async Task<MatchCreatedDto?> GetMatchByIdAsync(
        Guid matchId,
        Guid createdByUserId)
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match ID is required.");

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Organizer user ID is required.");

        var match = await _matchRepository.GetByIdAsync(matchId);

        // Only the organizer who created this match can access it
        // through this management endpoint.
        if (match is null || match.CreatedByUserId != createdByUserId)
            return null;

        return new MatchCreatedDto
        {
            MatchId = match.Id,
            CreatedByUserId = match.CreatedByUserId,
            Name = match.Name,
            TeamAName = match.TeamAName,
            TeamALogo = match.TeamALogo,
            TeamBName = match.TeamBName,
            TeamBLogo = match.TeamBLogo,
            ScheduledAt = match.ScheduledAt,
            Venue = match.Venue,
            LocationAddress = match.LocationAddress,
            State = match.State,
            PinCode = match.PinCode,
            Latitude = match.Latitude,
            Longitude = match.Longitude,
            Overs = match.Overs,
            PlayersPerTeam = match.PlayersPerTeam,
            Status = match.Status,
            TossAgreed = match.TossAgreed,
            TossWinnerTeam = match.TossWinnerTeam,
            TossDecision = match.TossDecision,

            Players = match.MatchPlayers
                .OrderBy(p => p.Team)
                .ThenBy(p => p.CreatedAt)
                .Select(p => new CreatedMatchPlayerDto
                {
                    MatchPlayerId = p.Id,
                    UserId = p.UserId,
                    PlayerName = BuildPlayerName(p.User),
                    MobileNumber = p.User.MobileNumber,
                    Email = p.User.Email,
                    Team = p.Team,
                    IsCaptain = p.IsCaptain
                })
                .ToList()
        };
    }

    public async Task<PublicMatchDetailsDto?> GetPublicMatchByIdAsync(
    Guid matchId)
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Match ID is required.");

        var match = await _matchRepository.GetByIdAsync(matchId);

        if (match is null)
            return null;

        return new PublicMatchDetailsDto
        {
            MatchId = match.Id,

            Name = match.Name,

            TeamAName = match.TeamAName,
            TeamALogo = match.TeamALogo,

            TeamBName = match.TeamBName,
            TeamBLogo = match.TeamBLogo,

            ScheduledAt = match.ScheduledAt,

            Venue = match.Venue,
            LocationAddress = match.LocationAddress,
            State = match.State,

            Overs = match.Overs,
            PlayersPerTeam = match.PlayersPerTeam,

            Status = match.Status,

            TossAgreed = match.TossAgreed,
            TossWinnerTeam = match.TossWinnerTeam,
            TossDecision = match.TossDecision,

            CancellationReasonType = match.CancellationReasonType,
            CancellationReason = match.CancellationReason,

            WinnerTeam = match.WinnerTeam,
            ResultText = match.ResultText,

            Players = match.MatchPlayers
                .OrderBy(p => p.Team)
                .ThenBy(p => p.CreatedAt)
                .Select(p => new PublicMatchPlayerDto
                {
                    PlayerName = BuildPlayerName(p.User),
                    Team = p.Team,
                    IsCaptain = p.IsCaptain
                })
                .ToList()
        };
    }



    public async Task<MatchCreatedDto> UpdateMatchAsync(
        Guid matchId,
        Guid createdByUserId,
        UpdateMatchDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (matchId == Guid.Empty)
            throw new ArgumentException("Match ID is required.");

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Organizer user ID is required.");

        var match = await _matchRepository.GetByIdAsync(matchId);

        if (match is null || match.CreatedByUserId != createdByUserId)
            throw new InvalidOperationException(
                "Match was not found or you are not its organizer.");

        if (!string.Equals(match.Status, "Upcoming",
                StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                "Only upcoming matches can be updated.");

        ValidateUpdateMatch(dto);

        var organizer = await _userRepository.GetByIdAsync(createdByUserId);

        if (organizer is null || !organizer.IsActive)
            throw new InvalidOperationException(
                "Organizer account was not found.");

        var createDto = new CreateMatchDto
        {
            CreatedByUserId = createdByUserId,
            Name = dto.Name,
            TeamAName = dto.TeamAName,
            TeamALogo = dto.TeamALogo,
            TeamBName = dto.TeamBName,
            TeamBLogo = dto.TeamBLogo,
            ScheduledAt = dto.ScheduledAt,
            Venue = dto.Venue,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            Overs = dto.Overs,
            PlayersPerTeam = dto.PlayersPerTeam,
            Players = dto.Players
        };

        ValidatePlayers(createDto);
        ValidateCreatorIsNotInRoster(createDto.Players, organizer);

        var resolvedPlayers = new List<ResolvedPlayer>();

        foreach (var playerDto in createDto.Players)
        {
            var resolved = await ResolvePlayerAsync(
                createdByUserId,
                playerDto);

            resolvedPlayers.Add(resolved);
        }

        if (resolvedPlayers.Any(p => p.UserId == createdByUserId))
            throw new InvalidOperationException(
                "The match organizer cannot be listed as a player.");

        if (resolvedPlayers.GroupBy(p => p.UserId).Any(g => g.Count() > 1))
            throw new InvalidOperationException(
                "A player account can appear only once in a match.");

        ValidateResolvedPlayers(createDto, resolvedPlayers);

        var now = DateTime.UtcNow;

        // Update the existing match; do not create a new match.
        match.Name = dto.Name.Trim();
        match.TeamAName = dto.TeamAName.Trim();
        match.TeamALogo = NormalizeTeamLogo(dto.TeamALogo, "Team A");
        match.TeamBName = dto.TeamBName.Trim();
        match.TeamBLogo = NormalizeTeamLogo(dto.TeamBLogo, "Team B");
        match.ScheduledAt = dto.ScheduledAt;
        match.Venue = dto.Venue.Trim();
        match.Latitude = dto.Latitude;
        match.Longitude = dto.Longitude;
        match.Overs = dto.Overs;
        match.PlayersPerTeam = dto.PlayersPerTeam;
        match.UpdatedAt = now;

        // Preserve existing roster records where possible.
        var existingByUserId = match.MatchPlayers
            .ToDictionary(p => p.UserId);

        var submittedIds = resolvedPlayers
            .Select(p => p.UserId)
            .ToHashSet();

        foreach (var existing in match.MatchPlayers.ToList())
        {
            if (!submittedIds.Contains(existing.UserId))
                match.MatchPlayers.Remove(existing);
        }

        foreach (var player in resolvedPlayers)
        {
            if (existingByUserId.TryGetValue(player.UserId, out var existing))
            {
                existing.Team = player.Team;
                existing.IsCaptain = false;
            }
            else
            {
                match.MatchPlayers.Add(new MatchPlayer
                {
                    Id = Guid.NewGuid(),
                    MatchId = match.Id,
                    UserId = player.UserId,
                    Team = player.Team,
                    IsCaptain = false,
                    CreatedAt = now
                });
            }
        }

        // First player in each team becomes captain.
        foreach (var team in new[] { "TeamA", "TeamB" })
        {
            var captain = resolvedPlayers.First(p => p.Team == team);

            var captainRecord = match.MatchPlayers
                .Single(p => p.UserId == captain.UserId);

            captainRecord.IsCaptain = true;
        }

        await _matchRepository.UpdateAsync(match);
        await _unitOfWork.SaveChangesAsync();

        // Reload the updated match with its related data.
        var updatedMatch = await _matchRepository.GetByIdAsync(match.Id)
            ?? throw new InvalidOperationException(
                "The updated match could not be reloaded.");

        return new MatchCreatedDto
        {
            MatchId = updatedMatch.Id,
            CreatedByUserId = updatedMatch.CreatedByUserId,
            Name = updatedMatch.Name,
            TeamAName = updatedMatch.TeamAName,
            TeamALogo = updatedMatch.TeamALogo,
            TeamBName = updatedMatch.TeamBName,
            TeamBLogo = updatedMatch.TeamBLogo,
            ScheduledAt = updatedMatch.ScheduledAt,
            Venue = updatedMatch.Venue,
            LocationAddress = updatedMatch.LocationAddress,
            State = updatedMatch.State,
            PinCode = updatedMatch.PinCode,
            Latitude = updatedMatch.Latitude,
            Longitude = updatedMatch.Longitude,
            Overs = updatedMatch.Overs,
            PlayersPerTeam = updatedMatch.PlayersPerTeam,
            Status = updatedMatch.Status,
            Players = updatedMatch.MatchPlayers
                .OrderBy(p => p.Team)
                .ThenBy(p => p.CreatedAt)
                .Select(p => new CreatedMatchPlayerDto
                {
                    MatchPlayerId = p.Id,
                    UserId = p.UserId,
                    PlayerName = BuildPlayerName(p.User),
                    MobileNumber = p.User.MobileNumber,
                    Email = p.User.Email,
                    Team = p.Team,
                    IsCaptain = p.IsCaptain
                })
                .ToList()
        };
    }



    private static void ValidateUpdateMatch(UpdateMatchDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) ||
            dto.Name.Trim().Length > 200)
            throw new ArgumentException(
                "Match name is required and cannot exceed 200 characters.");

        if (string.IsNullOrWhiteSpace(dto.TeamAName) ||
            dto.TeamAName.Trim().Length > 100)
            throw new ArgumentException(
                "Team A name is required and cannot exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(dto.TeamBName) ||
            dto.TeamBName.Trim().Length > 100)
            throw new ArgumentException(
                "Team B name is required and cannot exceed 100 characters.");

        if (string.Equals(dto.TeamAName.Trim(), dto.TeamBName.Trim(),
            StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException(
                "Team A and Team B must have different names.");

        _ = NormalizeTeamLogo(dto.TeamALogo, "Team A");
        _ = NormalizeTeamLogo(dto.TeamBLogo, "Team B");

        if (string.IsNullOrWhiteSpace(dto.Venue) ||
            dto.Venue.Trim().Length > 200)
            throw new ArgumentException(
                "Venue is required and cannot exceed 200 characters.");

        if (dto.ScheduledAt == default)
            throw new ArgumentException(
                "Match date and time are required.");

        if (dto.Overs is < 1 or > 90)
            throw new ArgumentException(
                "Overs must be between 1 and 90.");

        if (dto.PlayersPerTeam is < 4 or > 11)
            throw new ArgumentException(
                "Players per team must be between 4 and 11.");

        if (dto.Latitude is < -90 or > 90 ||
            dto.Longitude is < -180 or > 180)
            throw new ArgumentException("Invalid coordinates.");
    }

    public async Task CancelMatchAsync(
    Guid matchId,
    Guid createdByUserId,
    CancelMatchDto dto)
    {
        if (matchId == Guid.Empty)
            throw new ArgumentException("Invalid match ID.");

        if (createdByUserId == Guid.Empty)
            throw new ArgumentException("Invalid organizer ID.");

        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.CancellationReasonType))
            throw new ArgumentException("Cancellation reason type is required.");

        var match = await _matchRepository.GetByIdAsync(matchId);

        if (match == null)
            throw new KeyNotFoundException("Match not found.");

        if (match.CreatedByUserId != createdByUserId)
            throw new UnauthorizedAccessException(
                "You are not authorized to cancel this match.");

        var canCancel =
    string.Equals(match.Status, "Upcoming",
        StringComparison.OrdinalIgnoreCase) ||
    string.Equals(match.Status, "Live",
        StringComparison.OrdinalIgnoreCase);

        if (!canCancel)
        {
            throw new InvalidOperationException(
                "Only upcoming or live matches can be cancelled.");
        }


        match.Status = "Cancelled";
        match.CancellationReasonType = dto.CancellationReasonType.Trim();
        match.CancellationReason = string.IsNullOrWhiteSpace(dto.CancellationReason)
            ? null
            : dto.CancellationReason.Trim();
        match.UpdatedAt = DateTime.UtcNow;

        // Update the organizer's lifetime cancellation statistics.
        var organizerStatistics =
            await _organizerStatisticsRepository.GetByUserIdAsync(createdByUserId);

        var createOrganizerStatistics = organizerStatistics is null;

        organizerStatistics ??= new OrganizerStatistics
        {
            UserId = createdByUserId
        };

        organizerStatistics.MatchesCancelled++;

        await _transactionManager.BeginTransactionAsync();

        try
        {
            await _matchRepository.UpdateAsync(match);

            if (createOrganizerStatistics)
                await _organizerStatisticsRepository.AddAsync(organizerStatistics);
            else
                await _organizerStatisticsRepository.UpdateAsync(organizerStatistics);

            await _unitOfWork.SaveChangesAsync();
            await _transactionManager.CommitTransactionAsync();
        }
        catch
        {
            await _transactionManager.RollbackTransactionAsync();
            throw;
        }

    }

}