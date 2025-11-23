using System.Net;
using System.Text.Json;

namespace StockMarketAssistant.AnalyticsService.WebApi.Middleware
{
    /// <summary>
    /// Middleware для глобальной обработки исключений
    /// </summary>
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
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
                _logger.LogError(ex, "Необработанное исключение: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = exception switch
            {
                ArgumentException or ArgumentOutOfRangeException => HttpStatusCode.BadRequest, // 400
                UnauthorizedAccessException => HttpStatusCode.Unauthorized, // 401
                KeyNotFoundException => HttpStatusCode.NotFound, // 404
                HttpRequestException => HttpStatusCode.ServiceUnavailable, // 503
                _ => HttpStatusCode.InternalServerError // 500
            };

            var response = new
            {
                error = statusCode switch
                {
                    HttpStatusCode.BadRequest => "BadRequest",
                    HttpStatusCode.Unauthorized => "Unauthorized",
                    HttpStatusCode.NotFound => "NotFound",
                    HttpStatusCode.ServiceUnavailable => "ServiceUnavailable",
                    _ => "InternalServerError"
                },
                message = exception.Message,
                statusCode = (int)statusCode
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            var json = JsonSerializer.Serialize(response, jsonOptions);
            return context.Response.WriteAsync(json);
        }
    }
}

