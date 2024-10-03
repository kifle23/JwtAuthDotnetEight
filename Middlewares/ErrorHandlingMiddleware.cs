using System.Net;

namespace JwtAuthDotnetEight.Middlewares
{
    public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IHostEnvironment environment)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;
        private readonly IHostEnvironment _environment = environment;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred during request processing.");

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            dynamic response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error. Please try again later."
            };

            if (_environment.IsDevelopment())
            {
                response = new
                {
                    StatusCode = context.Response.StatusCode,
                    Message = exception.Message,
                    Detailed = exception.StackTrace
                };
            }

            return context.Response.WriteAsJsonAsync((object)response);
        }
    }
}
