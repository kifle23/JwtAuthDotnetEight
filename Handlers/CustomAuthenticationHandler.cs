using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using JwtAuthDotnetEight.Services;

namespace JwtAuthDotnetEight.Handlers
{
    public class CustomAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ITokenFactory tokenFactory) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authHeader = Request.Headers.Authorization.ToString();
            string? token = null;

            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader["Bearer ".Length..].Trim();
            }
            else
            {
                var accessToken = Request.Query["access_token"].ToString();
                var isHubPath = Request.Path.StartsWithSegments("/hub");
                if (!string.IsNullOrEmpty(accessToken) && isHubPath)
                {
                    token = accessToken;
                }
            }

            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var principal = tokenFactory.ValidateToken(token);
            return Task.FromResult(principal != null
                ? AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name))
                : AuthenticateResult.Fail("Invalid token"));
        }
    }
}
