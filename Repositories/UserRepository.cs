using JwtAuthDotnetEight.Data;
using JwtAuthDotnetEight.Models;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthDotnetEight.Repositories
{
    public class UserRepository(DataContext context) : IUserRepository
    {
        private readonly DataContext _context = context;

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Username == username) ?? throw new Exception("User not found");
            return user;
        }

        public async Task<IList<string>> GetUserRolesAsync(int userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();
        }
    }
}
