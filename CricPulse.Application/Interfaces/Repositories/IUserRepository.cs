using CricPulse.Domain.Entities;

namespace CricPulse.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid userId);

    Task<User?> GetByMobileNumberAsync(string mobileNumber);

    Task<User?> GetByEmailAsync(string email);

    Task<User?> GetByLoginIdentifierAsync(string loginIdentifier);

    Task AddAsync(User user);

    Task UpdateAsync(User user);

    Task<bool> ExistsByMobileNumberAsync(string mobileNumber);

    Task<bool> ExistsByEmailAsync(string email);
}