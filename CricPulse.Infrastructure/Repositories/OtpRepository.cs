
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CricPulse.Application.Interfaces.otp;
using CricPulse.Domain.Entities;
using CricPulse.Infrastructure.Data;

namespace CricPulse.Infrastructure.Repositories
{
    public class OtpRepository:IOtprepository
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
    }
}
