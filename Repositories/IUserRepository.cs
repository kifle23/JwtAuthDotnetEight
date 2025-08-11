using JwtAuthDotnetEight.Models;

namespace JwtAuthDotnetEight.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<IList<string>> GetUserRolesAsync(int userId);
        Task<User?> FindByUsernameAsync(string username);
        Task<User?> FindByEmailAsync(string email);
        Task AddUserAsync(User user, string roleName);
        Task<List<User>> GetAllUsersAsync();

    }
}
