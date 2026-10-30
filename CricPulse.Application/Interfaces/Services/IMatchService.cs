using CricPulse.Application.DTOs.Match;

namespace CricPulse.Application.Interfaces.Services;

public interface IMatchService
{
    Task<MatchCreatedDto> CreateMatchAsync(CreateMatchDto dto);

    Task<IReadOnlyList<MatchDiscoveryDto>> GetMatchesAsync(
        MatchDiscoveryQueryDto query);

    Task<IReadOnlyList<string>> GetIndianStatesAsync();


    Task<IReadOnlyList<MatchDiscoveryDto>> GetMyMatchesAsync(
        Guid createdByUserId,
        string? status);

    Task<MatchCreatedDto?> GetMatchByIdAsync(
    Guid matchId,  
    Guid createdByUserId);

    Task<MatchCreatedDto> UpdateMatchAsync(
        Guid matchId,
        Guid createdByUserId,
        UpdateMatchDto dto);

    Task CancelMatchAsync(
        Guid matchId,
        Guid createdByUserId,
        CancelMatchDto dto);

    Task<PublicMatchDetailsDto?> GetPublicMatchByIdAsync(Guid matchId);

}