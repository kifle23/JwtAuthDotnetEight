using JwtAuthDotnetEight.Middlewares;

namespace JwtAuthDotnetEight.Extensions
{
    public static class MiddlewareExtensions
    {
        public static WebApplication UseJwtMiddleware(this WebApplication app)
        {
            app.UseMiddleware<JwtMiddleware>();
            return app;
        }

        public static WebApplication UseErrorHandlingMiddleware(this WebApplication app)
        {
            app.UseMiddleware<ErrorHandlingMiddleware>();
            return app;
        }
    }
}
