using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JwtAuthDotnetEight.Models;
using JwtAuthDotnetEight.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthDotnetEight.Services
{
    public class JwtTokenFactory(IConfiguration config, IUserRepository userRepository) : ITokenFactory
    {
        private readonly string _secretKey = config.GetSection("AppSettings:Token")?.Value ?? throw new ArgumentNullException("AppSettings:Token", "Secret key cannot be null");
        private readonly IUserRepository _userRepository = userRepository;

        public string CreateToken(User user)
        {
            var roles = _userRepository.GetUserRolesAsync(user.Id).Result;

            var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Username)
        };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            try
            {
                var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }

}
