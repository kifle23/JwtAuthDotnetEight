using JwtAuthDotnetEight.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSwaggerConfiguration()
    .AddDatabaseContext(builder.Configuration)
    .AddDependencyInjection()
    .AddJwtAuth(builder.Configuration);

var app = builder.Build();

app.UseSwaggerDocumentation()
   .SeedDatabase()
   .UseJwtMiddleware()
   .AddAuthMiddlewares();

app.MapControllers();
app.UseHttpsRedirection();
app.Run();
