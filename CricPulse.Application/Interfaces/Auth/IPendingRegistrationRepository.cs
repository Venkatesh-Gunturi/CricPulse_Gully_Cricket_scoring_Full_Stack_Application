using CricPulse.Domain.Entities;

namespace CricPulse.Application.Interfaces.Auth
{
    public interface IPendingRegistrationRepository
    {
        Task<PendingRegistration?> GetByIdAsync(int id);

        Task<PendingRegistration?> GetByMobileNumberAsync(
            string mobileNumber);

        Task<PendingRegistration> CreateAsync(
            PendingRegistration registration);

        Task UpdateAsync(
            PendingRegistration registration);

        Task DeleteAsync(
            PendingRegistration registration);
    }
}