using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CricPulse.Application.Interfaces.Auth;
using Microsoft.AspNetCore.Identity;
using UserEntity = CricPulse.Domain.Entities.User;

namespace CricPulse.Infrastructure.Authentication
{
    public class PasswordHasherService:IPasswordHasher
    {
        private readonly PasswordHasher<UserEntity> _passwordHasher;

        public PasswordHasherService()
        {
            _passwordHasher = new PasswordHasher<UserEntity>();
        }

        public string HashPassword(UserEntity user,string password)
        {
            return _passwordHasher.HashPassword(user,password);
        }

        public bool VerifyPassword(UserEntity user,string password,string passwordHash)
        {
            var result= _passwordHasher.VerifyHashedPassword(user,password,passwordHash);

            return result == PasswordVerificationResult.Success ||
                    result == PasswordVerificationResult.SuccessRehashNeeded;
        }
    }
}
