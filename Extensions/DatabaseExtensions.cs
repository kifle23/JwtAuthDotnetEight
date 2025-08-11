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
            var useInMemory = configuration.GetValue<bool>("UseInMemory");
            services.AddDbContext<DataContext>(options =>
            {
                if (useInMemory)
                {
                    options.UseInMemoryDatabase("TaskDb");
                }
                else
                {
                    options.UseSqlServer(configuration.GetConnectionString("DevDB"));
                }
            });
            return services;
        }
        public static WebApplication SeedDatabase(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<DataContext>();
                context.Database.EnsureCreated();
                if (!context.Users.Any())
                {
                    var adminRole = new Role { Name = "Admin" };
                    var userRole = new Role { Name = "User" };

                    context.Roles.AddRange(adminRole, userRole);

                    var adminUser = new User
                    {
                        Username = "admin",
                        Email = "admin@example.com",
                        PasswordHash = PasswordHasher.HashPassword("password")
                    };

                    var normalUser = new User
                    {
                        Username = "user",
                        Email = "user@example.com",
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

                    if (!context.Tasks.Any())
                    {
                        var tasks = new List<TaskItem>
                        {
                            new TaskItem { Title = "Design database schema", Description = "Draft ERD and relations", Priority = TaskPriority.HIGH, Creator = adminUser, CreatorId = adminUser.Id, Assignee = adminUser, AssigneeId = adminUser.Id, Status = JwtAuthDotnetEight.Models.TaskStatus.TODO },
                            new TaskItem { Title = "Implement auth", Description = "JWT login/register", Priority = TaskPriority.HIGH, Creator = adminUser, CreatorId = adminUser.Id, Assignee = normalUser, AssigneeId = normalUser.Id, Status = JwtAuthDotnetEight.Models.TaskStatus.IN_PROGRESS },
                            new TaskItem { Title = "Build UI skeleton", Description = "Set up React router", Priority = TaskPriority.MEDIUM, Creator = normalUser, CreatorId = normalUser.Id, Assignee = normalUser, AssigneeId = normalUser.Id, Status = JwtAuthDotnetEight.Models.TaskStatus.TODO },
                            new TaskItem { Title = "Write unit tests", Description = "Cover services", Priority = TaskPriority.MEDIUM, Creator = adminUser, CreatorId = adminUser.Id, Assignee = adminUser, AssigneeId = adminUser.Id, Status = JwtAuthDotnetEight.Models.TaskStatus.DONE },
                            new TaskItem { Title = "Add drag and drop", Description = "Integrate DnD library", Priority = TaskPriority.MEDIUM, Creator = adminUser, CreatorId = adminUser.Id, Assignee = normalUser, AssigneeId = normalUser.Id, Status = JwtAuthDotnetEight.Models.TaskStatus.TODO },
                            new TaskItem { Title = "Wire SignalR", Description = "Realtime updates for tasks", Priority = TaskPriority.HIGH, Creator = adminUser, CreatorId = adminUser.Id, Assignee = adminUser, AssigneeId = adminUser.Id, Status = JwtAuthDotnetEight.Models.TaskStatus.IN_PROGRESS }
                        };
                        context.Tasks.AddRange(tasks);
                        context.SaveChanges();
                    }
                }
            }
            return app;
        }
    }

}
