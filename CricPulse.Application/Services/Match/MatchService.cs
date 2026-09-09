using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CricPulse.Application.Interfaces.User;
using CricPulse.Application.DTOs.Match;
using CricPulse.Application.Interfaces.Match;
using CricPulse.Application.Interfaces.Location;

using MatchEntity=CricPulse.Domain.Entities.Match;

namespace CricPulse.Application.Services.Match
{
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _matchRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILocationService _locationService;


        public MatchService(IMatchRepository matchRepository, IUserRepository userRepository, ILocationService locationService)
        {
            _matchRepository = matchRepository;
            _userRepository = userRepository;
            _locationService = locationService;
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

            match.PlayersPerTeam = dto.PlayersPerTeam;
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


        public async Task<PlayerLookupResponseDto>
    LookupPlayerByMobileAsync(string mobileNumber)
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
    }
}
