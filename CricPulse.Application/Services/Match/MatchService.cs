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
    }
}
