using CricPulse.Application.DTOs.Player;

namespace CricPulse.Application.Interfaces.Services;

public interface IPlayerStatisticsService
{
    Task<PlayerStatisticsDto?> GetStatisticsAsync(Guid userId);
}