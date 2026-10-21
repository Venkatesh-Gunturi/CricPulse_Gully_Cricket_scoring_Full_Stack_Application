using CricPulse.Domain.Entities;

namespace CricPulse.Application.Interfaces.Repositories;

public interface IMatchRepository
{
    Task<Match?> GetByIdAsync(Guid matchId);

    Task AddAsync(Match match);

    Task UpdateAsync(Match match);

    Task DeleteAsync(Match match);

    // Geographic state, for example Telangana.
    Task<IReadOnlyList<Match>> GetByStateAsync(string state);

    // Lifecycle status, for example Upcoming or Finished.
    Task<IReadOnlyList<Match>> GetByStatusAsync(string status);

    Task<IReadOnlyList<Match>> GetForDiscoveryAsync(
        string? status,
        string? state,
        double? minLatitude,
        double? maxLatitude,
        double? minLongitude,
        double? maxLongitude);

    Task<IReadOnlyList<Match>> GetNearestAsync(
        double latitude,
        double longitude,
        int count);


    Task<IReadOnlyList<Match>> GetByCreatorAsync(
        Guid createdByUserId,
        string? status);

}