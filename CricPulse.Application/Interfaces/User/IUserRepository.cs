using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using UserEntity = CricPulse.Domain.Entities.User;

namespace CricPulse.Application.Interfaces.User
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email);

        Task<bool> MobileExistsAsync(string mobileNumber);

        Task<UserEntity> CreateAsync(UserEntity user);
    }
}
