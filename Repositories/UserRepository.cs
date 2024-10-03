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
            var user = await _context.Users
                .Include(u => u.UserRoles!)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            return user ?? throw new InvalidOperationException("User not found");
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
