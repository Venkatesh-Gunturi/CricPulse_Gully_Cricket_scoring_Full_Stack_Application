using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UserEntity = CricPulse.Domain.Entities.User;

namespace CricPulse.Application.Interfaces.Auth
{
    public interface IPasswordHasher
    {
        string HashPassword(UserEntity user,string password);

        bool VerifyPassword(UserEntity user,string password, string passwordHash);
    }
}
