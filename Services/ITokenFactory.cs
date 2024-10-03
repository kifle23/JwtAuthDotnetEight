using System.Security.Claims;
using JwtAuthDotnetEight.Models;

namespace JwtAuthDotnetEight.Services
{
    public interface ITokenFactory
    {
        string CreateToken(User user);
        ClaimsPrincipal? ValidateToken(string token);
    }
}
