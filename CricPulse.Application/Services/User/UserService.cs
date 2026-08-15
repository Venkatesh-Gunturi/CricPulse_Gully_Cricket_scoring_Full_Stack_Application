using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CricPulse.Application.DTOs.User;
using CricPulse.Application.Interfaces.User;
using UserEntity = CricPulse.Domain.Entities.User;

namespace CricPulse.Application.Services.User
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }


        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
        {
            //Check whether email already exists or not 
            bool emailExists=await _userRepository.EmailExistsAsync(dto.Email);

            if (emailExists)
            {
                throw new InvalidOperationException("An account with this email already exists. Please login in.");
            }

            //check whether mobile number already exist or not
            bool mobileExists = await _userRepository.MobileExistsAsync(dto.MobileNumber);

            if (mobileExists)
            {
                throw new InvalidOperationException("An account with this mobile number already exists. Please log in.");
            }

            //Create User Entity
            var user = new UserEntity
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                MobileNumber = dto.MobileNumber,

                IsEmailVerified = false,
                IsMobileVerified = false,
                IsUmpire = false,
                CreatedAt= DateTime.UtcNow
            };

            //Save user details
            var CreateUser=await _userRepository.CreateAsync(user);

            //Convert entity to response DTO
            return new UserResponseDto
            {
                Id = CreateUser.Id,
                Firstname = CreateUser.FirstName,
                LastName = CreateUser.LastName,
                Email = CreateUser.Email,
                MobileNumber = CreateUser.MobileNumber,
                IsEmailVerified = CreateUser.IsEmailVerified,
                IsMobileVerified = CreateUser.IsMobileVerified,
                ProfileImageUrl = CreateUser.ProfileImageUrl,
                IsActive = CreateUser.IsActive,
                CreatedAt = CreateUser.CreatedAt
            };
        }
    }
}
