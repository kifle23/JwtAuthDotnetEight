using JwtAuthDotnetEight.Handlers;
using Microsoft.AspNetCore.Authentication;

namespace JwtAuthDotnetEight.Extensions
{
    public static class AuthExtensions
    {
        public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication("Scheme")
                    .AddScheme<AuthenticationSchemeOptions, CustomAuthenticationHandler>("Scheme", null);
            services.AddAuthorization();

            return services;
        }
        public static WebApplication AddAuthMiddlewares(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
            return app;
        }
    }

}
