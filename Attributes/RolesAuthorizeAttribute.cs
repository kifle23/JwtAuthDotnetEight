using System.Security.Claims;
using JwtAuthDotnetEight.Models;
using JwtAuthDotnetEight.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JwtAuthDotnetEight.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RolesAuthorizeAttribute(params string[] roles) : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string[] _roles = roles;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var username = user.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(username))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userRepository = context.HttpContext.RequestServices.GetService<IUserRepository>() ?? throw new InvalidOperationException("IUserRepository is not registered.");
            var dbUser = await userRepository.GetUserByUsernameAsync(username);
            if (dbUser == null || !UserHasRequiredRole(dbUser))
            {
                context.Result = new ForbidResult();
                return;
            }
        }

        private bool UserHasRequiredRole(User user)
        {
            foreach (var role in _roles)
            {
                if (user.UserRoles != null && user.UserRoles.Any(ur => ur.Role.Name == role))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
