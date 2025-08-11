using JwtAuthDotnetEight.Attributes;
using JwtAuthDotnetEight.Dtos;
using JwtAuthDotnetEight.Repositories;
using JwtAuthDotnetEight.Services;
using JwtAuthDotnetEight.Utilities;
using Microsoft.AspNetCore.Mvc;
using JwtAuthDotnetEight.Models;

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
            var user = await _userRepository.FindByUsernameAsync(loginDto.Username);
            if (user == null || !PasswordHasher.VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return Unauthorized();
            }

            var token = await _tokenFactory.CreateTokenAsync(user);
            return Ok(new { token });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (await _userRepository.FindByUsernameAsync(registerDto.Username) != null)
                return Conflict("Username already exists");
            if (await _userRepository.FindByEmailAsync(registerDto.Email) != null)
                return Conflict("Email already exists");

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = PasswordHasher.HashPassword(registerDto.Password)
            };
            await _userRepository.AddUserAsync(user, registerDto.Role);
            return Created($"/api/users/{user.Id}", new { user.Id, user.Username, user.Email });
        }

    }
}
