using JwtAuthDotnetEight.Extensions;
using JwtAuthDotnetEight.Hubs;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("client", p => p
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .WithOrigins(
            "http://localhost:5173",
            "https://localhost:5173",
            "http://localhost:5174",
            "https://localhost:5174",
            
            "http://127.0.0.1:5173",
            "https://127.0.0.1:5173",
            "http://127.0.0.1:5174",
            "https://127.0.0.1:5174",
            
            "http://localhost:3000",
            "https://localhost:3000"
        ));
});

builder.Services.AddSwaggerConfiguration()
    .AddDatabaseContext(builder.Configuration)
    .AddDependencyInjection()
    .AddJwtAuth(builder.Configuration);
builder.Services.AddSignalR().AddJsonProtocol(options =>
{
    options.PayloadSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

app.UseErrorHandlingMiddleware()
    .SeedDatabase()
    .UseSwaggerDocumentation();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors("client");

app.UseAuthentication();
app.UseAuthorization();

app.UseJwtMiddleware();
app.MapControllers();
app.MapHub<TaskHub>("/hub/tasks").RequireCors("client");
app.Run();

public partial class Program { }
