using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;
// Handles database operations for password reset tokens.
public class PasswordResetTokenRepository : RepositoryBase<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(CricPulseDbContext context) : base(context)
    {
    }
    // Adds a new password reset token to the current database context.
    public async Task AddAsync(PasswordResetToken token)
    {
        await AddEntityAsync(token);
    }
    // Retrieves a password reset token by its hash, including its associated user.
    public async Task<PasswordResetToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await DbSet
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.TokenHash == tokenHash);
    }
    // Marks the specified password reset token as modified.
    public Task UpdateAsync(PasswordResetToken token)
    {
        UpdateEntity(token);
        return Task.CompletedTask;
    }
    // Marks all unused, unexpired password reset tokens for the user as used.
    public async Task InvalidateActiveTokensAsync(Guid userId)
    {
        // Find tokens that belong to the user and are still active.
        var activeTokens = await DbSet
            .Where(x =>
                x.UserId == userId &&
                x.UsedAt == null &&
                x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();
        // Invalidate each active token by recording when it was used.
        foreach (var token in activeTokens)
        {
            token.UsedAt = DateTime.UtcNow;
        }
    }
}