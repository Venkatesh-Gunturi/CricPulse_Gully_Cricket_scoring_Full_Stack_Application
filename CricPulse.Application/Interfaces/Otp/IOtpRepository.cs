using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CricPulse.Application.Interfaces.otp;
using CricPulse.Domain.Entities;

namespace CricPulse.Application.Interfaces.otp
{
    public interface IOtprepository 
    {
        Task<OtpVerification> CreateAsync(OtpVerification otpVerification);
    }
}
