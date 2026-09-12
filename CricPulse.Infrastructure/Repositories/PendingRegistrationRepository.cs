using CricPulse.Application.Interfaces.Auth;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CricPulse.Infrastructure.Repositories
{
    public class PendingRegistrationRepository : IPendingRegistrationRepository
    {
        private readonly CricPulseDbContext _context;

        public PendingRegistrationRepository(CricPulseDbContext context)
        {
            _context = context;
        }

        // Purpose:
        // Retrieve a pending registration using its temporary registration ID.
        public async Task<PendingRegistration?> GetByIdAsync(int id)
        {
            return await _context.PendingRegistrations
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        // Purpose:
        // Find an existing pending registration for a mobile number.
        public async Task<PendingRegistration?> GetByMobileNumberAsync(
            string mobileNumber)
        {
            return await _context.PendingRegistrations
                .FirstOrDefaultAsync(x => x.MobileNumber == mobileNumber);
        }

        // Purpose:
        // Store a new temporary registration until mobile OTP verification is completed.
        public async Task<PendingRegistration> CreateAsync(
            PendingRegistration registration)
        {
            await _context.PendingRegistrations.AddAsync(registration);
            await _context.SaveChangesAsync();

            return registration;
        }

        // Purpose:
        // Update temporary registration data such as OTP and attempt count.
        public async Task UpdateAsync(
            PendingRegistration registration)
        {
            _context.PendingRegistrations.Update(registration);
            await _context.SaveChangesAsync();
        }

        // Purpose:
        // Remove a temporary registration after it is no longer needed.
        public async Task DeleteAsync(
            PendingRegistration registration)
        {
            _context.PendingRegistrations.Remove(registration);
            await _context.SaveChangesAsync();
        }
    }
}