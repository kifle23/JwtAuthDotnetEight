using JwtAuthDotnetEight.Repositories;
using JwtAuthDotnetEight.Services;

namespace JwtAuthDotnetEight.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddDependencyInjection(this IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();
            services.AddScoped<ITokenFactory, JwtTokenFactory>();
            services.AddScoped<ITaskService, TaskService>();
            return services;
        }
    }

}
