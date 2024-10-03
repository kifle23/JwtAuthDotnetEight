using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace JwtAuthDotnetEight.Extensions
{
    public static class AuthExtensions
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication("Bearer")
            .AddJwtBearer(options =>
            {
                var secret = configuration["AppSettings:Token"];
                if (string.IsNullOrEmpty(secret))
                {
                    throw new InvalidOperationException("JWT Secret is not configured.");
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

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
