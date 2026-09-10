using CricPulse.Application.DTOs.Match;
using CricPulse.Application.Interfaces.Auth;
using CricPulse.Application.Interfaces.Location;
using CricPulse.Application.Interfaces.Match;
using CricPulse.Application.Interfaces.Otp;
using CricPulse.Application.Interfaces.player;
using CricPulse.Application.Interfaces.User;
using CricPulse.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MatchEntity=CricPulse.Domain.Entities.Match;
using UserEntity = CricPulse.Domain.Entities.User;
using PlayerEntity = CricPulse.Domain.Entities.Player;
using MatchPlayerEntity = CricPulse.Domain.Entities.MatchPlayer;

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

        public async Task<MatchResponseDto> CreateMatchAsync(int umpireId, CreateMatchDto dto)
        {
            var user = await _userRepository.GetByIdAsync(umpireId);

            if (user == null)
            {
                throw new InvalidOperationException("User not found.");
            }

            if (!user.IsUmpire)
            {
                throw new UnauthorizedAccessException(
                    "Only umpires can create matches.");
            }

            if (dto.PlayersPerTeam < 4 || dto.PlayersPerTeam > 11)
            {
                throw new InvalidOperationException(
                    "Players per team must be between 4 and 11.");
            }

            if (dto.Players == null || dto.Players.Count == 0)
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

            if (dto.Players.Any(p =>
                !validTeams.Contains(p.Team)))
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

            if (normalizedMobiles.Count != normalizedMobiles.Distinct().Count())
            {
                throw new InvalidOperationException(
                    "A player cannot be assigned more than once.");
            }

            var playersToAdd = new List<MatchPlayerEntity>();

            foreach (var playerDto in dto.Players)
            {
                var userAccount =
                    await _userRepository.GetByMobileNumberAsync(
                        playerDto.MobileNumber.Trim());

                if (userAccount == null ||
                    !userAccount.IsMobileVerified ||
                    userAccount.Player == null)
                {
                    throw new InvalidOperationException(
                        $"Player with mobile number {playerDto.MobileNumber} is not onboarded.");
                }

                if (userAccount.Id == umpireId)
                {
                    throw new InvalidOperationException(
                        "The match umpire cannot participate as a player.");
                }

                var matchPlayer = new MatchPlayerEntity
                {
                    PlayerId = userAccount.Player.Id,
                    Team = playerDto.Team,
                    CreatedAt = DateTime.UtcNow
                };

                playersToAdd.Add(matchPlayer);
            }

            var state = await _locationService.GetStateAsync(dto.Latitude, dto.Longitude);



            var match = new MatchEntity
            {
                UmpireId = umpireId,

                Team1Name = dto.Team1Name,
                Team1Logo = dto.Team1Logo,

                Team2Name = dto.Team2Name,
                Team2Logo = dto.Team2Logo,

                PlayersPerTeam = dto.PlayersPerTeam,
                Overs = dto.Overs,

                MatchDate = dto.MatchDate,
                MatchTime = dto.MatchTime,

                VenueName = dto.VenueName,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                State = state ?? string.Empty,

                LiveStreamUrl = dto.LiveStreamUrl,

                Status = "Scheduled",
                CreatedAt = DateTime.UtcNow
            };

            foreach (var matchPlayer in playersToAdd)
            {
                match.MatchPlayers.Add(matchPlayer);
            }

            var createdMatch = await _matchRepository.CreateAsync(match);

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

                Status = match.Status,

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

            if (match.Status != "Scheduled")
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

        public async Task<MatchResponseDto?> StartMatchAsync(
    int matchId,
    int userId,
    StartMatchDto dto)
        {
            var match = await _matchRepository.GetByIdForUpdateAsync(matchId);

            if (match == null)
            {
                return null;
            }

            if (match.UmpireId != userId)
            {
                throw new UnauthorizedAccessException(
                    "Only the match umpire can start this match.");
            }

            if (match.Status != "Scheduled")
            {
                throw new InvalidOperationException(
                    "Only scheduled matches can be started.");
            }

            var state = await _locationService.GetStateAsync(
                dto.Latitude,
                dto.Longitude);

            if (string.IsNullOrEmpty(state))
            {
                throw new InvalidOperationException(
                    "Unable to determine match state from current location.");
            }

            match.Latitude = dto.Latitude;
            match.Longitude = dto.Longitude;
            match.State = state;

            match.Status = "Live";
            match.UpdatedAt = DateTime.UtcNow;

            var updatedMatch = await _matchRepository.UpdateAsync(match);

            return updatedMatch == null
                ? null
                : MapToResponse(updatedMatch);
        }

        public async Task<bool> CancelMatchAsync(
    int matchId,
    int userId)
        {
            var match = await _matchRepository.GetByIdForUpdateAsync(matchId);

            if (match == null)
            {
                return false;
            }

            if (match.UmpireId != userId)
            {
                throw new UnauthorizedAccessException(
                    "Only the match umpire can cancel this match.");
            }

            if (match.Status != "Scheduled" &&
                match.Status != "Live")
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


        public async Task<PlayerLookupResponseDto> LookupPlayerByMobileAsync(string mobileNumber)
        {
            var normalizedMobile =
                mobileNumber.Trim();

            var user =
                await _userRepository.GetByMobileNumberAsync(
                    normalizedMobile);

            if (user == null)
            {
                return new PlayerLookupResponseDto
                {
                    IsRegistered = false,
                    DisplayName = string.Empty
                };
            }

            if (user.Player == null)
            {
                return new PlayerLookupResponseDto
                {
                    IsRegistered = false,
                    DisplayName = string.Empty
                };
            }

            var displayName =
                string.Join(
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
                PlayerId = user.Player.Id,
                DisplayName = displayName
            };
        }

        

        public async Task<int> StartPlayerOnboardingAsync(string mobileNumber)
        {
            var normalizedMobile = mobileNumber.Trim();

            var existingUser =
                await _userRepository.GetByMobileNumberAsync(normalizedMobile);

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
    }
}
