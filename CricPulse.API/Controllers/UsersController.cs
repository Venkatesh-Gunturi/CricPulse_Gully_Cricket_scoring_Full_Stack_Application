using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CricPulse.Application.DTOs.User;
using CricPulse.Application.Interfaces.User;

namespace CricPulse.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        //To create user Record
        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser(CreateUserDto dto)
        {
            var user = await _userService.CreateUserAsync(dto);

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = user.Id },
                
                user);
        }

        //To get user record by ID
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserResponseDto>> GetUserById(int id)
        {
            var user= await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }

        //To get records of all users
        [HttpGet]
        public async Task<ActionResult<List<UserResponseDto>>> GetAllUsers()
        {
            var users= await _userService.GetAllUsersAsync();

            return Ok(users);
        }
    }
}
