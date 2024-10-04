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
            var token = context.Request.Headers.Authorization.FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                var principal = tokenFactory.ValidateToken(token);
                if (principal != null)
                {
                    context.User = principal;
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
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            await context.Response.WriteAsync("Forbidden: You do not have access to this resource.");
                            return;
                        }
                    }
                }
            }

            await _next(context);
        }
    }
}
