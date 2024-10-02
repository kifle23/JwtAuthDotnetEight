using JwtAuthDotnetEight.Models;

namespace JwtAuthDotnetEight.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetUserByUsernameAsync(string username);
        Task<IList<string>> GetUserRolesAsync(int userId);

    }
}
