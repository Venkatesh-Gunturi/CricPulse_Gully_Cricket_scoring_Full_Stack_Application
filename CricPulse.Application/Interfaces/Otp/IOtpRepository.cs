using CricPulse.Domain.Entities;
using CricPulse.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CricPulse.Application.Interfaces.Otp
{
    public interface IOtpRepository 
    {
        Task<OtpVerification> CreateAsync(OtpVerification otpVerification);

        Task<OtpVerification?> GetLatestAsync(int userId, OtpType otpType);

        Task UpdateAsync(OtpVerification otpVerification);
    }
}
