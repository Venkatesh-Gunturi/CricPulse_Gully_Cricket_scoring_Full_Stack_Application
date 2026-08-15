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

        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser(CreateUserDto dto)
        {
            var user=await _userService.CreateUserAsync(dto);

            return Ok(user);
        }
    }
}
