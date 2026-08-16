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
    // communicates with the database using dbcontext 
    public class UserRepository : IUserRepository
    {
        private readonly CricPulseDbContext _context;

        public UserRepository(CricPulseDbContext context)
        {
            _context = context;
        }

        //checks Email exists in Db
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users
                .AnyAsync(u => u.Email == email);
        }

        //checks mobile  number exists in Db
        public async Task<bool> MobileExistsAsync(string mobileNumber)
        {
            return await _context.Users
                .AnyAsync(u => u.MobileNumber == mobileNumber);
        }

        //Creates user Record in Db
        public async Task<UserEntity> CreateAsync(UserEntity user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return user;
        }

        //checks User Id exists in Db
        public async Task<UserEntity?> GetByIdAsync(int id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        //checking all users records in Db
        public async Task<List<UserEntity>> GetAllAsync()
        {
            return await _context.Users.AsNoTracking().ToListAsync();
        }
    }
}
