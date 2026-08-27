
using CricPulse.Application.Interfaces.Otp;
using CricPulse.Domain.Entities;
using CricPulse.Domain.Enums;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricPulse.Infrastructure.Repositories
{
    public class OtpRepository:IOtpRepository
    {
        private readonly CricPulseDbContext _context;

        public OtpRepository(CricPulseDbContext context)
        {
            _context = context;
        }

        public async Task<OtpVerification> CreateAsync(OtpVerification otpVerification)
        {
            await _context.OtpVerification.AddAsync(otpVerification);

            await _context.SaveChangesAsync();

            return otpVerification;
        }

        public async Task<OtpVerification?> GetLatestAsync(int userId, OtpType otpType)
        {
            return await _context.OtpVerification
                .Where(o => o.UserId == userId &&
                            o.OtpType == otpType &&
                            !o.IsUsed)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(OtpVerification otpVerification)
        {
            _context.OtpVerification.Update(otpVerification);
            await _context.SaveChangesAsync();
        }
    }
}
