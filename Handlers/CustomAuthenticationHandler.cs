using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace JwtAuthDotnetEight.Handlers
{
    public class CustomAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration config) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        private readonly string _secretKey = config["AppSettings:Token"]
            ?? throw new ArgumentNullException("AppSettings:Token", "Secret key cannot be null");

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var token = authHeader["Bearer ".Length..].Trim();

            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.Fail("Token is missing"));
            }

            var principal = ValidateToken(token);
            return Task.FromResult(principal != null
                ? AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name))
                : AuthenticateResult.Fail("Invalid token"));
        }

        private ClaimsPrincipal? ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_secretKey);

            try
            {
                return tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out _);
            }
            catch
            {
                return null;
            }
        }
    }
}
