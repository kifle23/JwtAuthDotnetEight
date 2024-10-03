using JwtAuthDotnetEight.Data;
using JwtAuthDotnetEight.Models;
using JwtAuthDotnetEight.Utilities;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthDotnetEight.Extensions
{
    public static class DatabaseExtensions
    {
        public static IServiceCollection AddDatabaseContext(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DevDB")));
            return services;
        }
        public static WebApplication SeedDatabase(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                if (!context.Users.Any())
                {
                    var adminRole = new Role { Name = "Admin" };
                    var userRole = new Role { Name = "User" };

                    context.Roles.AddRange(adminRole, userRole);

                    var adminUser = new User
                    {
                        Username = "admin",
                        PasswordHash = PasswordHasher.HashPassword("password")
                    };

                    var normalUser = new User
                    {
                        Username = "user",
                        PasswordHash = PasswordHasher.HashPassword("password")
                    };

                    context.Users.AddRange(adminUser, normalUser);

                    context.UserRoles.AddRange(new UserRole
                    {
                        User = adminUser,
                        Role = adminRole
                    },
                    new UserRole
                    {
                        User = normalUser,
                        Role = userRole
                    });

                    context.SaveChanges();
                }
            }
            return app;
        }
    }

}
