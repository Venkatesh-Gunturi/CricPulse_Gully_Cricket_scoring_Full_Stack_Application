using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;

public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(CricPulseDbContext context) : base(context) { }

    public Task<User?> GetByIdAsync(Guid userId) =>
        DbSet.FirstOrDefaultAsync(x => x.Id == userId);

    public Task<User?> GetByMobileNumberAsync(string mobileNumber) =>
        DbSet.FirstOrDefaultAsync(x => x.MobileNumber == mobileNumber);

    public Task<User?> GetByEmailAsync(string email) =>
        DbSet.FirstOrDefaultAsync(x => x.Email == email.Trim().ToLowerInvariant());

    public Task<User?> GetByLoginIdentifierAsync(string loginIdentifier)
    {
        var value = loginIdentifier.Trim();
        return DbSet.FirstOrDefaultAsync(x =>
            x.MobileNumber == value || x.Email == value.ToLowerInvariant());
    }

    public Task AddAsync(User user) => AddEntityAsync(user);

    public Task UpdateAsync(User user)
    {
        UpdateEntity(user);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsByMobileNumberAsync(string mobileNumber) =>
        DbSet.AnyAsync(x => x.MobileNumber == mobileNumber);

    public Task<bool> ExistsByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return DbSet.AnyAsync(x => x.Email == normalizedEmail);
    }
}
