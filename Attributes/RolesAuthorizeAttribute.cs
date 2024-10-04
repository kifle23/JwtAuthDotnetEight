using Microsoft.AspNetCore.Mvc.Filters;

namespace JwtAuthDotnetEight.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RolesAuthorizeAttribute(params string[] roles) : Attribute, IAuthorizationFilter
    {
        public string[] Roles { get; } = roles;

        public void OnAuthorization(AuthorizationFilterContext context)
        {
        }
    }
}
