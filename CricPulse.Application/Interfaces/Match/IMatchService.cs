using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CricPulse.Application.DTOs.Match;

namespace CricPulse.Application.Interfaces.Match
{
    public interface IMatchService
    {
        Task<MatchResponseDto> CreateMatchAsync(int umpireId, CreateMatchDto dto);
        Task<MatchResponseDto?> GetMatchByIdAsync(int id);
        Task<List<MatchResponseDto>> GetAllMatchesAsync();
        Task<List<MatchResponseDto>> GetNearbyMatchesAsync(
    double latitude,
    double longitude,
    double radiusInKm);

        Task<List<MatchResponseDto>> GetMatchesByStateAsync(string state);
    }
}
