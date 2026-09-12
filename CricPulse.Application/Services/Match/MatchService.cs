using CricPulse.Application.DTOs.Match;
using CricPulse.Application.Interfaces.Auth;
using CricPulse.Application.Interfaces.Location;
using CricPulse.Application.Interfaces.Match;
using CricPulse.Application.Interfaces.Otp;
using CricPulse.Application.Interfaces.player;
using CricPulse.Application.Interfaces.User;
using CricPulse.Application.Utilities;
using CricPulse.Domain.Enums;
using CricPulse.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchEntity=CricPulse.Domain.Entities.Match;
using MatchPlayerEntity = CricPulse.Domain.Entities.MatchPlayer;
using PlayerEntity = CricPulse.Domain.Entities.Player;
using UserEntity = CricPulse.Domain.Entities.User;


namespace CricPulse.Application.Services.Match
{
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILocationService _locationService;
        private readonly IPlayerRepository _playerRepository;
        private readonly IOtpService _otpService;
        private readonly IOtpRepository _otpRepository;
        private readonly IPasswordHasher _passwordHasher;

        public MatchService(IMatchRepository matchRepository, IUserRepository userRepository, ILocationService locationService,IOtpRepository otpRepository,IOtpService otpService,IPasswordHasher passwordHasher,IPlayerRepository playerRepository)
        {
            _matchRepository = matchRepository;
            _userRepository = userRepository;
            _locationService = locationService;
            _otpRepository = otpRepository;
            _passwordHasher = passwordHasher;
            _playerRepository = playerRepository;
            _otpService = otpService;


        }

        // Purpose:
        // Validate match creation details, prepare players, capture the match location,
        // and create a new scheduled match.
        public async Task<MatchResponseDto> CreateMatchAsync(
            int umpireId,
            CreateMatchDto dto)
        {
            var user = await _userRepository.GetByIdAsync(umpireId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            // Any authenticated registered user becomes an umpire when creating a match.
            if (!user.IsUmpire)
            {
                user.IsUmpire = true;
                await _userRepository.UpdateAsync(user);
            }

            if (string.IsNullOrWhiteSpace(dto.Team1Name) ||
                string.IsNullOrWhiteSpace(dto.Team2Name))
            {
                throw new InvalidOperationException(
                    "Both team names are required.");
            }

            if (!int.TryParse(dto.Team1Logo, out var team1LogoId) ||
                !Enum.IsDefined(typeof(TeamLogo), team1LogoId))
            {
                throw new InvalidOperationException(
                    "Team 1 logo must be one of the predefined logos.");
            }

            if (!int.TryParse(dto.Team2Logo, out var team2LogoId) ||
                !Enum.IsDefined(typeof(TeamLogo), team2LogoId))
            {
                throw new InvalidOperationException(
                    "Team 2 logo must be one of the predefined logos.");
            }

            if (dto.PlayersPerTeam < 4 || dto.PlayersPerTeam > 11)
            {
                throw new InvalidOperationException(
                    "Players per team must be between 4 and 11.");
            }

            if (dto.Overs < 1 || dto.Overs > 90)
            {
                throw new InvalidOperationException(
                    "Overs must be between 1 and 90.");
            }

            if (dto.MatchDate == default)
            {
                throw new InvalidOperationException(
                    "Match date is required.");
            }

            if (string.IsNullOrWhiteSpace(dto.VenueName))
            {
                throw new InvalidOperationException(
                    "Venue name is required.");
            }

            if (dto.Latitude < -90 || dto.Latitude > 90 ||
                dto.Longitude < -180 || dto.Longitude > 180)
            {
                throw new InvalidOperationException(
                    "Invalid match location.");
            }

            if (dto.Players == null)
            {
                throw new InvalidOperationException(
                    "Players are required.");
            }

            var expectedPlayers = dto.PlayersPerTeam * 2;

            if (dto.Players.Count != expectedPlayers)
            {
                throw new InvalidOperationException(
                    $"Exactly {expectedPlayers} players are required.");
            }

            var validTeams = new[] { "Team1", "Team2" };

            if (dto.Players.Any(p => !validTeams.Contains(p.Team)))
            {
                throw new InvalidOperationException(
                    "Players must belong to Team1 or Team2.");
            }

            var team1Players = dto.Players
                .Where(p => p.Team == "Team1")
                .ToList();

            var team2Players = dto.Players
                .Where(p => p.Team == "Team2")
                .ToList();

            if (team1Players.Count != dto.PlayersPerTeam)
            {
                throw new InvalidOperationException(
                    $"Team 1 must have exactly {dto.PlayersPerTeam} players.");
            }

            if (team2Players.Count != dto.PlayersPerTeam)
            {
                throw new InvalidOperationException(
                    $"Team 2 must have exactly {dto.PlayersPerTeam} players.");
            }

            var normalizedMobiles = dto.Players
                .Select(p => p.MobileNumber.Trim())
                .ToList();

            if (normalizedMobiles.Any(m =>
                    m.Length != 10 || !m.All(char.IsDigit)))
            {
                throw new InvalidOperationException(
                    "Every player mobile number must contain exactly 10 digits.");
            }

            if (normalizedMobiles.Count != normalizedMobiles.Distinct().Count())
            {
                throw new InvalidOperationException(
                    "A player cannot be assigned more than once.");
            }

            var playersToAdd = new List<MatchPlayerEntity>();

            foreach (var playerDto in dto.Players)
            {
                var mobileNumber = playerDto.MobileNumber.Trim();

                var userAccount =
                    await _userRepository.GetByMobileNumberAsync(mobileNumber);

                if (userAccount == null ||
                    !userAccount.IsMobileVerified ||
                    userAccount.Player == null)
                {
                    throw new InvalidOperationException(
                        $"Player with mobile number {mobileNumber} is not onboarded.");
                }

                if (userAccount.Id == umpireId)
                {
                    throw new InvalidOperationException(
                        "The match umpire cannot participate as a player.");
                }

                playersToAdd.Add(new MatchPlayerEntity
                {
                    PlayerId = userAccount.Player.Id,
                    Team = playerDto.Team,
                    CreatedAt = DateTime.UtcNow
                });
            }

            if (!string.IsNullOrWhiteSpace(dto.LiveStreamUrl))
            {
                if (!Uri.TryCreate(
                        dto.LiveStreamUrl,
                        UriKind.Absolute,
                        out var youtubeUri) ||
                    (youtubeUri.Host != "youtube.com" &&
                     youtubeUri.Host != "www.youtube.com" &&
                     youtubeUri.Host != "youtu.be" &&
                     youtubeUri.Host != "m.youtube.com"))
                {
                    throw new InvalidOperationException(
                        "Live stream URL must be a valid YouTube URL.");
                }
            }

            var state = await _locationService.GetStateAsync(
                dto.Latitude,
                dto.Longitude);

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new InvalidOperationException(
                    "Unable to determine match state from the current location.");
            }

            var match = new MatchEntity
            {
                UmpireId = umpireId,

                Team1Name = dto.Team1Name.Trim(),
                Team1Logo = dto.Team1Logo,
                Team2Name = dto.Team2Name.Trim(),
                Team2Logo = dto.Team2Logo,

                PlayersPerTeam = dto.PlayersPerTeam,
                Overs = dto.Overs,

                MatchDate = dto.MatchDate,
                MatchTime = dto.MatchTime,

                VenueName = dto.VenueName.Trim(),
                Address = dto.Address?.Trim() ?? string.Empty,

                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                State = state,

                LiveStreamUrl = dto.LiveStreamUrl?.Trim(),

                Status = MatchStatus.Scheduled,

                CreatedAt = DateTime.UtcNow
            };

            foreach (var matchPlayer in playersToAdd)
            {
                match.MatchPlayers.Add(matchPlayer);
            }

            var createdMatch =
                await _matchRepository.CreateAsync(match);

            return MapToResponse(createdMatch);
        }

        public async Task<MatchResponseDto?> GetMatchByIdAsync(int id)
        {
            var match = await _matchRepository.GetByIdAsync(id);

            if (match == null)
            {
                return null;
            }

            return MapToResponse(match);
        }

        // Purpose:
        // Convert the domain match entity into the API response model used by the frontend.
        private MatchResponseDto MapToResponse(MatchEntity match)
        {
            return new MatchResponseDto
            {
                Id = match.Id,
                UmpireId = match.UmpireId,

                Team1Name = match.Team1Name,
                Team1Logo = match.Team1Logo,

                Team2Name = match.Team2Name,
                Team2Logo = match.Team2Logo,

                PlayersPerTeam = match.PlayersPerTeam,
                Overs = match.Overs,

                MatchDate = match.MatchDate,
                MatchTime = match.MatchTime,

                VenueName = match.VenueName,
                Address = match.Address,

                Latitude = match.Latitude,
                Longitude = match.Longitude,
                State = match.State,

                LiveStreamUrl = match.LiveStreamUrl,

                Status = match.Status.ToString(),

                CreatedAt = match.CreatedAt,
                UpdatedAt = match.UpdatedAt
            };
        }

        public async Task<List<MatchResponseDto>> GetAllMatchesAsync()
        {
            var matches = await _matchRepository.GetAllAsync();

            return matches
                .Select(MapToResponse)
                .ToList();
        }

        public async Task<List<MatchResponseDto>> GetNearbyMatchesAsync(
         double latitude,
         double longitude,
         double radiusInKm)
        {
            var nearbyMatches = await _matchRepository.GetNearbyMatchesAsync(
                latitude,
                longitude,
                radiusInKm);

            return nearbyMatches
                .Select(x =>
                {
                    var response = MapToResponse(x.Match);

                    response.DistanceInKm = x.DistanceInKm;

                    return response;
                })
                .ToList();
        }

        public async Task<List<MatchResponseDto>> GetMatchesByStateAsync(string state)
        {
            var matches = await _matchRepository.GetMatchesByStateAsync(state);

            return matches
                .Select(MapToResponse)
                .ToList();
        }


        // Purpose:
        // Update a scheduled match while preventing changes after the match has started.
        public async Task<MatchResponseDto?> UpdateMatchAsync(
            int matchId,
            int userId,
            UpdateMatchDto dto)
        {
            var match =
                await _matchRepository.GetByIdForUpdateAsync(matchId);

            if (match == null)
            {
                return null;
            }

            if (match.UmpireId != userId)
            {
                throw new UnauthorizedAccessException(
                    "Only the match umpire can update this match.");
            }

            if (match.Status != MatchStatus.Scheduled)
            {
                throw new InvalidOperationException(
                    "Only scheduled matches can be updated.");
            }

            match.Team1Name = dto.Team1Name;
            match.Team1Logo = dto.Team1Logo;

            match.Team2Name = dto.Team2Name;
            match.Team2Logo = dto.Team2Logo;

            match.Overs = dto.Overs;

            match.MatchDate = dto.MatchDate;
            match.MatchTime = dto.MatchTime;

            match.VenueName = dto.VenueName;
            match.Address = dto.Address;

            match.LiveStreamUrl = dto.LiveStreamUrl;

            match.UpdatedAt = DateTime.UtcNow;

            var updatedMatch =
                await _matchRepository.UpdateAsync(match);

            return updatedMatch == null
                ? null
                : MapToResponse(updatedMatch);
        }

        // Purpose:
        // Start a scheduled match, enforce its 24-hour start window,
        // capture its final physical location, and change its status to Live.
        public async Task<MatchResponseDto?> StartMatchAsync(
            int matchId,
            int userId,
            StartMatchDto dto)
        {
            var match =
                await _matchRepository.GetByIdForUpdateAsync(matchId);

            if (match == null)
            {
                return null;
            }

            if (match.UmpireId != userId)
            {
                throw new UnauthorizedAccessException(
                    "Only the match umpire can start this match.");
            }

            if (match.Status != MatchStatus.Scheduled)
            {
                throw new InvalidOperationException(
                    "Only scheduled matches can be started.");
            }

            var scheduledUtc = MatchTimeHelper.GetScheduledUtc(
                match.MatchDate,
                match.MatchTime);

            var cancellationDeadline = scheduledUtc.AddHours(24);

            if (DateTime.UtcNow > cancellationDeadline)
            {
                match.Status = MatchStatus.Cancelled;
                match.CancellationReason = "Umpire unavailable";
                match.UpdatedAt = DateTime.UtcNow;

                await _matchRepository.UpdateAsync(match);

                throw new InvalidOperationException(
                    "This match was automatically cancelled because it was not started within 24 hours of its scheduled time.");
            }

            var state = await _locationService.GetStateAsync(
                dto.Latitude,
                dto.Longitude);

            if (string.IsNullOrWhiteSpace(state))
            {
                throw new InvalidOperationException(
                    "Unable to determine match state from current location.");
            }

            // The device's current GPS location becomes the final match location.
            match.Latitude = dto.Latitude;
            match.Longitude = dto.Longitude;
            match.State = state;

            match.Status = MatchStatus.Live;
            match.StartedAt = DateTime.UtcNow;
            match.UpdatedAt = DateTime.UtcNow;

            var updatedMatch =
                await _matchRepository.UpdateAsync(match);

            return updatedMatch == null
                ? null
                : MapToResponse(updatedMatch);
        }

        // Purpose:
        // Cancel a scheduled or live match when requested by its assigned umpire.
        public async Task<bool> CancelMatchAsync(
            int matchId,
            int userId)
        {
            var match =
                await _matchRepository.GetByIdForUpdateAsync(matchId);

            if (match == null)
            {
                return false;
            }

            if (match.UmpireId != userId)
            {
                throw new UnauthorizedAccessException(
                    "Only the match umpire can cancel this match.");
            }

            if (match.Status != MatchStatus.Scheduled &&
                match.Status != MatchStatus.Live)
            {
                throw new InvalidOperationException(
                    "Only scheduled or live matches can be cancelled.");
            }

            return await _matchRepository.CancelAsync(matchId);
        }

        public async Task<List<MatchResponseDto>> GetMyMatchesAsync(int umpireId)
        {
            var matches = await _matchRepository
                .GetMatchesByUmpireAsync(umpireId);

            return matches
                .Select(MapToResponse)
                .ToList();
        }


        // Purpose:
        // Find a registered player by mobile number or indicate that onboarding is required.
        public async Task<PlayerLookupResponseDto>
            LookupPlayerByMobileAsync(
                int umpireId,
                string mobileNumber)
        {
            var normalizedMobile = mobileNumber.Trim();

            var umpire = await _userRepository
                .GetByIdAsync(umpireId);

            if (umpire == null)
            {
                throw new InvalidOperationException(
                    "Umpire account not found.");
            }

            // The umpire creating the match cannot add their own account as a player.
            if (umpire.MobileNumber == normalizedMobile)
            {
                throw new ConflictException(
                    "You cannot add yourself as a player in a match you are umpiring.");
            }

            var user = await _userRepository
                .GetByMobileNumberAsync(normalizedMobile);

            if (user == null)
            {
                return new PlayerLookupResponseDto
                {
                    IsRegistered = false,
                    IsOtpPending = false
                };
            }

            if (!user.IsMobileVerified)
            {
                return new PlayerLookupResponseDto
                {
                    IsRegistered = false,
                    IsOtpPending = true
                };
            }

            // A verified CricPulse user is automatically considered a player.
            // No separate Player profile is required for match assignment.
            var displayName = string.Join(
                " ",
                new[]
                {
            user.FirstName,
            user.LastName
                }
                .Where(x => !string.IsNullOrWhiteSpace(x)));

            return new PlayerLookupResponseDto
            {
                IsRegistered = true,
                IsOtpPending = false,
                PlayerId = user.Player?.Id,
                DisplayName = string.IsNullOrWhiteSpace(displayName)
                    ? "Player"
                    : displayName
            };
        }



        // Purpose:
        // Start OTP onboarding for a new player while preventing the match umpire
        // from onboarding their own mobile number as a player.
        public async Task<int> StartPlayerOnboardingAsync(
            int umpireId,
            string mobileNumber)
        {
            var normalizedMobile = mobileNumber.Trim();

            var umpire = await _userRepository.GetByIdAsync(umpireId);

            if (umpire == null)
            {
                throw new UnauthorizedAccessException(
                    "Umpire account could not be found.");
            }

            if (umpire.MobileNumber == normalizedMobile)
            {
                throw new ConflictException(
                    "You cannot add yourself as a player in a match you are umpiring.");
            }

            var existingUser =
                await _userRepository.GetByMobileNumberAsync(
                    normalizedMobile);

            if (existingUser != null)
            {
                if (existingUser.IsMobileVerified)
                {
                    throw new ConflictException(
                        "A verified account already exists for this mobile number.");
                }

                throw new ConflictException(
                    "OTP verification is already pending for this mobile number.");
            }

            // Generate a secure random temporary password.
            var temporaryPasswordBytes =
                System.Security.Cryptography.RandomNumberGenerator.GetBytes(24);

            var temporaryPassword =
                Convert.ToBase64String(temporaryPasswordBytes);

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

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    temporaryPassword);

            var createdUser =
                await _userRepository.CreateAsync(user);

            // Create placeholder name after we have the DB ID.
            createdUser.LastName = createdUser.Id.ToString();

            await _userRepository.UpdateAsync(createdUser);

            var otp = _otpService.GenerateOtp();

            var otpVerification =
                _otpService.CreateOtpVerification(
                    createdUser.Id,
                    otp,
                    Domain.Enums.OtpType.Mobile);

            await _otpRepository.CreateAsync(otpVerification);

            return createdUser.Id;
        }

        public async Task<bool> VerifyPlayerOnboardingAsync(
    int userId,
    string otpCode)
        {
            var verified = await _otpService.VerifyOtpAsync(
                userId,
                otpCode,
                Domain.Enums.OtpType.Mobile);

            if (!verified)
            {
                return false;
            }

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
            {
                return false;
            }

            if (!user.IsMobileVerified)
            {
                user.IsMobileVerified = true;
            }

            user.IsActive = true;

            await _userRepository.UpdateAsync(user);

            var existingPlayer =
                await _playerRepository.GetByUserIdAsync(user.Id);

            if (existingPlayer == null)
            {
                var player = new PlayerEntity
                {
                    UserId = user.Id,

                    // Placeholder values until player completes profile.
                    DateOfBirth = DateTime.UtcNow.Date,

                    Gender = string.Empty,
                    BattingStyle = string.Empty,
                    BowlingStyle = string.Empty,
                    PlayerRole = string.Empty,
                    State = string.Empty,
                    PinCode = 0
                };

                await _playerRepository.CreateAsync(player);
            }

            return true;
        }


        // Purpose:
        // Load the live match and map its current innings, teams, and ball history
        // into a response that the live screens can consume.
        public async Task<LiveMatchResponseDto?> GetLiveMatchAsync(
            int matchId)
        {
            var match = await _matchRepository
                .GetLiveMatchAsync(matchId);

            if (match == null ||
                match.Status != MatchStatus.Live)
            {
                return null;
            }

            var innings = match.Innings
                .OrderByDescending(i => i.InningsNumber)
                .FirstOrDefault();

            var response = new LiveMatchResponseDto
            {
                MatchId = match.Id,
                Team1Name = match.Team1Name,
                Team2Name = match.Team2Name,
                Team1Logo = match.Team1Logo,
                Team2Logo = match.Team2Logo,
                Status = match.Status.ToString(),
                TossWinnerTeam = match.TossWinnerTeam,
                TossDecision = match.TossDecision,
                BattingFirstTeam = match.BattingFirstTeam,
                InningsId = innings?.Id,
                InningsNumber = innings?.InningsNumber,
                BattingTeam = innings?.BattingTeam,
                BowlingTeam = innings?.BowlingTeam,
                StrikerMatchPlayerId = innings?.StrikerMatchPlayerId,
                NonStrikerMatchPlayerId = innings?.NonStrikerMatchPlayerId,
                CurrentBowlerMatchPlayerId =
                    innings?.CurrentBowlerMatchPlayerId,
                TotalRuns = innings?.TotalRuns,
                Wickets = innings?.Wickets,
                LegalBalls = innings?.LegalBalls
            };

            if (innings != null)
            {
                response.Balls = innings.Balls
                    .OrderBy(b => b.Id)
                    .Select(b => new LiveBallDto
                    {
                        Id = b.Id,
                        OverNumber = b.OverNumber,
                        BallNumber = b.BallNumber,
                        StrikerMatchPlayerId = b.StrikerMatchPlayerId,
                        NonStrikerMatchPlayerId = b.NonStrikerMatchPlayerId,
                        BowlerMatchPlayerId = b.BowlerMatchPlayerId,
                        Runs = b.Runs,
                        IsLegalDelivery = b.IsLegalDelivery,
                        ExtraType = b.ExtraType,
                        ExtraRuns = b.ExtraRuns,
                        WicketType = b.Wicket?.WicketType,
                        DismissedMatchPlayerId =
                            b.Wicket?.DismissedMatchPlayerId
                    })
                    .ToList();
            }

            return response;
        }
    }
}
