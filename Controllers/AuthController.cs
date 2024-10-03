using JwtAuthDotnetEight.Attributes;
using JwtAuthDotnetEight.Dtos;
using JwtAuthDotnetEight.Repositories;
using JwtAuthDotnetEight.Services;
using JwtAuthDotnetEight.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDotnetEight.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IUserRepository userRepository, ITokenFactory tokenFactory) : ControllerBase
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly ITokenFactory _tokenFactory = tokenFactory;

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _userRepository.GetUserByUsernameAsync(loginDto.Username);
            if (user == null || !PasswordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized();
            }

            var token = _tokenFactory.CreateToken(user);
            return Ok(new { Token = token });
        }

        [RolesAuthorize("Admin")]
        [HttpGet("admin-endpoint")]
        public IActionResult AdminOnly()
        {
            return Ok("Admin access granted.");
        }

        [RolesAuthorize("Admin", "User")]
        [HttpGet("admin-or-user-endpoint")]
        public IActionResult AdminUser()
        {
            return Ok("Admin Or User access granted.");
        }

        [RolesAuthorize("User")]
        [HttpGet("user-endpoint")]
        public IActionResult NormalUser()
        {
            return Ok("User access granted.");
        }

    }
}
