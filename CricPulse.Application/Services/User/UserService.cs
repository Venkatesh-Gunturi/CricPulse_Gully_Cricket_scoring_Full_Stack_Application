
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CricPulse.Application.DTOs.User;
using CricPulse.Application.Interfaces.User;
using CricPulse.Domain.Exceptions;
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

        //method for captilizing the first letter of the word in name 
        private static string FormatName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return name;

            return string.Join(" ", name.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower())
            );
        }

        //Creates user record
        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto dto)
        {
            //Normalizing the email and mobile number to avoid redundent mail cconflict and for formal user names
            var normalizedFirstName = FormatName(dto.FirstName.Trim());

            var normalizedLastName = string.IsNullOrWhiteSpace(dto.LastName) ? null : FormatName(dto.LastName.Trim());

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var normalizedMobileNumber = dto.MobileNumber.Trim();

            //Check whether email already exists or not 
            bool emailExists=await _userRepository.EmailExistsAsync(normalizedEmail);

            if (emailExists)
            {
                throw new ConflictException("An account with this email already exists. Please log in.");

            }

            //check whether mobile number already exist or not
            bool mobileExists = await _userRepository.MobileExistsAsync(normalizedMobileNumber);

            if (mobileExists)
            {
                throw new ConflictException("An account with this mobile number already exists. Please log in.");

            }

            //Create User Entity
            var user = new UserEntity
            {
                FirstName = normalizedFirstName,
                LastName = normalizedLastName,
                Email = normalizedEmail,
                MobileNumber = normalizedMobileNumber,

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
                FirstName = CreateUser.FirstName,
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

        //for fetching user by Id from Repository
        public async Task<UserResponseDto> GetUserByIdAsync(int id)
        {
            var user= await _userRepository.GetByIdAsync(id);

            if(user == null)
            {
                throw new NotFoundException("User not found");
            }

            return new UserResponseDto
            {
                Id = user.Id,
                FirstName= user.FirstName,
                LastName= user.LastName,
                Email = user.Email,
                MobileNumber = user.MobileNumber,
                IsEmailVerified = user.IsEmailVerified,
                IsMobileVerified = user.IsMobileVerified,
                ProfileImageUrl= user.ProfileImageUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt

            };
        }

        //to get all users records from db via repository
        public async Task<List<UserResponseDto>> GetAllUsersAsync()
        {
            var users=await _userRepository.GetAllAsync();

            return users.Select(user => new UserResponseDto
            {
                Id=user.Id,
                FirstName=user.FirstName,
                LastName=user.LastName,
                Email=user.Email,
                MobileNumber=user.MobileNumber,
                IsEmailVerified=user.IsEmailVerified,
                IsMobileVerified=user.IsMobileVerified,
                ProfileImageUrl=user.ProfileImageUrl,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            }).ToList();
        }
    }
}
