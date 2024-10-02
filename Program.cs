using System.Text;
using JwtAuthDotnetEight.Data;
using JwtAuthDotnetEight.Models;
using JwtAuthDotnetEight.Repositories;
using JwtAuthDotnetEight.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Fill in the JWT token",
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new List<String>()
                    }
                });
            });

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DevDB")));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITokenFactory, JwtTokenFactory>();

// JWT Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration["AppSettings:Token"];
        Console.WriteLine($"JWT Secret: {secret}");
        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("JWT Secret is not configured.");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Seed the database
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    SeedDatabase(context);
}

// Configure the HTTP request pipeline.
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseHttpsRedirection();

app.Run();

static void SeedDatabase(DataContext context)
{
    // Seed users and roles
    if (!context.Users.Any())
    {
        var adminRole = new Role { Name = "Admin" };
        var userRole = new Role { Name = "User" };

        context.Roles.AddRange(adminRole, userRole);

        var adminUser = new User
        {
            Username = "admin",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
        };

        var normalUser = new User
        {
            Username = "user",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
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

