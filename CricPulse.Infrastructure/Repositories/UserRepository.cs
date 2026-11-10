using CricPulse.Application.Interfaces.Repositories;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories;
// Handles database operations for User entities.
public class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(CricPulseDbContext context) : base(context) { }
    // Retrieves a user by their unique ID.
    public Task<User?> GetByIdAsync(Guid userId) =>
        DbSet.FirstOrDefaultAsync(x => x.Id == userId);
    // Retrieves a user by their registered mobile number.
    public Task<User?> GetByMobileNumberAsync(string mobileNumber) =>
        DbSet.FirstOrDefaultAsync(x => x.MobileNumber == mobileNumber);
    // Retrieves a user by email after trimming whitespace and normalizing its casing.
    public Task<User?> GetByEmailAsync(string email) =>
        DbSet.FirstOrDefaultAsync(x => x.Email == email.Trim().ToLowerInvariant());
    // Retrieves a user using either their mobile number or email address.
    public Task<User?> GetByLoginIdentifierAsync(string loginIdentifier)
    {
        // Trim whitespace before comparing the login identifier.
        var value = loginIdentifier.Trim();
        return DbSet.FirstOrDefaultAsync(x =>
            x.MobileNumber == value || x.Email == value.ToLowerInvariant());
    }
    // Adds a new user to the current database context.
    public Task AddAsync(User user) => AddEntityAsync(user);
    // Marks the specified user as modified in the current database context.
    public Task UpdateAsync(User user)
    {
        UpdateEntity(user);
        return Task.CompletedTask;
    }
    // Checks whether the mobile number is already registered.
    public Task<bool> ExistsByMobileNumberAsync(string mobileNumber) =>
        DbSet.AnyAsync(x => x.MobileNumber == mobileNumber);
    // Checks whether an email address is already registered after normalization.
    public Task<bool> ExistsByEmailAsync(string email)
    {
        // Normalize the email to match the format used when storing user records.
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return DbSet.AnyAsync(x => x.Email == normalizedEmail);
    }
}
