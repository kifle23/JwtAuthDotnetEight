using System.Security.Claims;
using JwtAuthDotnetEight.Models;

namespace JwtAuthDotnetEight.Services
{
    public interface ITokenFactory
    {
        Task<string> CreateTokenAsync(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
