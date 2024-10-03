using JwtAuthDotnetEight.Services;

namespace JwtAuthDotnetEight.Middlewares
{
    public class JwtMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context, ITokenFactory tokenFactory)
        {
            var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var principal = tokenFactory.ValidateToken(token);
                if (principal != null)
                {
                    context.User = principal;
                }
            }

            await _next(context);
        }
    }
}
