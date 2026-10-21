using CricPulse.Application.DTOs.Match;

namespace CricPulse.Application.Interfaces.Services;

public interface IMatchPlayerService
{
    Task<MatchPlayerDto> AddRegisteredPlayerAsync(
        Guid matchId,
        Guid createdByUserId,
        Guid userId,
        string team);

    Task<MatchPlayerDto> CreateAndAddUnregisteredPlayerAsync(
        Guid matchId,
        Guid createdByUserId,
        string team,
        string? displayName);

    Task<IReadOnlyList<MatchPlayerDto>> GetPlayersAsync(
        Guid matchId);

    Task RemovePlayerAsync(
        Guid matchId,
        Guid createdByUserId,
        Guid matchPlayerId);

    Task SetCaptainAsync(
        Guid matchId,
        Guid createdByUserId,
        Guid matchPlayerId);
}