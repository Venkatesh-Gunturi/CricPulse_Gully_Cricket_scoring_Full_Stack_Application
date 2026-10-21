using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;

public class PasswordResetTokenRepository
    : RepositoryBase<PasswordResetToken>,
      IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(CricPulseDbContext context)
        : base(context)
    {
    }

    public async Task AddAsync(PasswordResetToken token)
    {
        await AddEntityAsync(token);
    }

    public async Task<PasswordResetToken?> GetByTokenHashAsync(
        string tokenHash)
    {
        return await DbSet
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
    }

    public Task UpdateAsync(PasswordResetToken token)
    {
        UpdateEntity(token);
        return Task.CompletedTask;
    }

    public async Task InvalidateActiveTokensAsync(Guid userId)
    {
        var activeTokens = await DbSet
            .Where(x =>
                x.UserId == userId &&
                x.UsedAt == null &&
                x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var token in activeTokens)
        {
            token.UsedAt = DateTime.UtcNow;
        }
    }
}