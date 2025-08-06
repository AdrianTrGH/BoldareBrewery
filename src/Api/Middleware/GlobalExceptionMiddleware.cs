using System.Net;
using System.Text.Json;
namespace BoldareBrewery.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception occurred. Request: {Method} {Path} {QueryString}",
                   context.Request.Method,
                   context.Request.Path,
                   context.Request.QueryString);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var (statusCode, error) = exception switch
            {
                ArgumentException => (HttpStatusCode.BadRequest, new
                {
                    code = "BadRequest",
                    message = exception.Message
                }),
                InvalidOperationException => (HttpStatusCode.BadRequest, new
                {
                    code = "InvalidOperation",
                    message = exception.Message
                }),
                _ => (HttpStatusCode.InternalServerError, new
                {
                    code = "InternalServerError",
                    message = "An internal server error occurred"
                })
            };

            context.Response.StatusCode = (int)statusCode;

            var jsonResponse = JsonSerializer.Serialize(error, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
