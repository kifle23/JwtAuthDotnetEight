using JwtAuthDotnetEight.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwaggerConfiguration()
    .AddDatabaseContext(builder.Configuration)
    .AddDependencyInjection()
    .AddJwtAuth(builder.Configuration);

var app = builder.Build();

app.UseErrorHandlingMiddleware()
   .SeedDatabase()
   .UseJwtMiddleware() // Intercepts the request to validate JWT tokens; this step is optional
   .AddAuthMiddlewares()
   .UseSwaggerDocumentation();

app.MapControllers();
app.UseHttpsRedirection();
app.Run();
