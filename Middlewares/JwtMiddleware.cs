using JwtAuthDotnetEight.Attributes;
using JwtAuthDotnetEight.Services;
using System.Security.Claims;

namespace JwtAuthDotnetEight.Middlewares
{
    public class JwtMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context, ITokenFactory tokenFactory)
        {
            var token = GetTokenFromHeader(context);

            if (!string.IsNullOrEmpty(token))
            {
                var principal = tokenFactory.ValidateToken(token);
                if (principal != null)
                {
                    context.User = principal;
                }
                else
                {
                    await HandleUnauthorized(context, "Unauthorized: Invalid or expired token.");
                    return;
                }
            }

            if (context.User.Identity?.IsAuthenticated == true)
            {
                var endpoint = context.GetEndpoint();
                if (endpoint != null)
                {
                    var rolesRequired = endpoint.Metadata.GetMetadata<RolesAuthorizeAttribute>()?.Roles;

                    if (rolesRequired != null && rolesRequired.Length > 0)
                    {
                        var userRoles = context.User.FindAll(ClaimTypes.Role).Select(r => r.Value);

                        if (!rolesRequired.Any(role => userRoles.Contains(role)))
                        {
                            await HandleForbidden(context);
                            return;
                        }
                    }
                }
            }
            else
            {
                var endpoint = context.GetEndpoint();
                var rolesRequired = endpoint?.Metadata.GetMetadata<RolesAuthorizeAttribute>()?.Roles;

                if (rolesRequired != null && rolesRequired.Length > 0)
                {
                    await HandleUnauthorized(context, "Unauthorized: Authentication is required.");
                    return;
                }
            }

            await _next(context);
        }

        private static string? GetTokenFromHeader(HttpContext context)
        {
            return context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
        }

        private static async Task HandleUnauthorized(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(message);
        }

        private static async Task HandleForbidden(HttpContext context)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden: You do not have access to this resource.");
        }
    }
}
