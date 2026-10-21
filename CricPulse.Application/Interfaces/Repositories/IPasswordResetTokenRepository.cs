using CricPulse.Domain.Entities;

namespace CricPulse.Application.Interfaces.Repositories;

public interface IPasswordResetTokenRepository
{
    Task AddAsync(PasswordResetToken token);

    Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash);

    Task UpdateAsync(PasswordResetToken token);

    Task InvalidateActiveTokensAsync(Guid userId);
}