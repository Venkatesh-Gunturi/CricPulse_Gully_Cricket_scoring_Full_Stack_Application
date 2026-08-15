using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Namespaces which are communicated by UserRepository
using CricPulse.Application.Interfaces.User;
using CricPulse.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

using UserEntity = CricPulse.Domain.Entities.User;

namespace CricPulse.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly CricPulseDbContext _context;

        public UserRepository(CricPulseDbContext context)
        {
            _context = context;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        public async Task<bool> MobileExistsAsync(string mobileNumber)
        {
            return await _context.Users
                .AnyAsync(u => u.MobileNumber == mobileNumber);
        }

        public async Task<UserEntity> CreateAsync(UserEntity user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}
