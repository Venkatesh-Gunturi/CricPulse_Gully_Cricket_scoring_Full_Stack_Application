using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CricPulse.Application.Interfaces.Auth;
using CricPulse.Domain.Entities;
using CricPulse.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace CricPulse.Infrastructure.Authentication
{
    public class OtpService : IOtpService
    {
        public string GenerateOtp()
        {
            return Random.Shared.Next(100000,1000000).ToString();
        }

        public OtpVerification CreateOtpVerification(int userId,string otp,OtpType otpType)
        {
            return new OtpVerification
            {
                UserId = userId,
                OtpCodeHash=otp,
                OtpType = otpType,
                ExpiresAt=DateTime.UtcNow.AddMinutes(5),
                IsUsed=false,
                AttemptedCount=0,
                CreatedAt=DateTime.UtcNow

            };
        }
    }
}
