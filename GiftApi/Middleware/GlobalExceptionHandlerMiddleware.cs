using GiftApi.Common.Exceptions;
using System.Net;
using System.Text.Json;

namespace GiftApi.Middleware
{
    public record ErrorResponse(bool Success, int StatusCode, string Message);

    public class GlobalExceptionHandlerMiddleware
    {
        readonly RequestDelegate _next;
        readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        readonly IHostEnvironment _env;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");

                var (statusCode, message) = ex switch
                {
                    NotFoundException => (HttpStatusCode.NotFound, ex.Message),
                    ValidationException => (HttpStatusCode.BadRequest, ex.Message),
                    InternalServerException => (HttpStatusCode.InternalServerError, ex.Message),
                    _ => (HttpStatusCode.InternalServerError,
                          _env.IsDevelopment() ? ex.Message : "An unexpected error occurred. Please try again later.")
                };

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)statusCode;

                var response = new ErrorResponse(false, (int)statusCode, message);

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                await context.Response.WriteAsJsonAsync(response, options);
            }
        }
    }
}
