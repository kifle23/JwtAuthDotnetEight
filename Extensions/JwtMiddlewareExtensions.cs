using JwtAuthDotnetEight.Middlewares;

namespace JwtAuthDotnetEight.Extensions
{
    public static class JwtMiddlewareExtensions
    {
        public static WebApplication UseJwtMiddleware(this WebApplication app)
        {
            app.UseMiddleware<JwtMiddleware>();
            return app;
        }
    }
}
