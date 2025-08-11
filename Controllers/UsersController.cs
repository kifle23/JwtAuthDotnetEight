using JwtAuthDotnetEight.Attributes;
using JwtAuthDotnetEight.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthDotnetEight.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController(IUserRepository userRepository) : ControllerBase
    {
        private readonly IUserRepository _userRepository = userRepository;

        [HttpGet]
        [RolesAuthorize("User", "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userRepository.GetAllUsersAsync();
            var result = users.Select(u => new { u.Id, u.Username, u.Email });
            return Ok(result);
        }
    }
}
