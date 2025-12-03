using Newtonsoft.Json;
using StockMarketAssistant.AnalyticsService.Domain.Exceptions;

namespace StockMarketAssistant.AnalyticsService.WebApi.Middleware
{
    /// <summary>
    /// Глобальный middleware для обработки SecurityException
    /// </summary>
    /// <param name="next"></param>
    /// <param name="logger"></param>
    public class SecurityExceptionMiddleware(RequestDelegate next, ILogger<SecurityExceptionMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<SecurityExceptionMiddleware> _logger = logger;

        /// <summary>
        /// Асинхронный метод, вызываемый для обработки каждого HTTP-запроса.
        /// Перехватывает исключения типа <see cref="SecurityException"/> и возвращает клиенту ответ с кодом 403 Forbidden.
        /// Если исключение не является <see cref="SecurityException"/>, оно передаётся дальше по конвейеру middleware.
        /// </summary>
        /// <param name="context">Контекст текущего HTTP-запроса</param>
        /// <returns>Задача, представляющая асинхронную операцию</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) when (ex is SecurityException)
            {
                _logger.LogWarning("SecurityException: {Message}", ex.Message);
                await HandleSecurityExceptionAsync(context, ex);
            }
        }

        private static async Task HandleSecurityExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status403Forbidden;

            var response = new
            {
                error = "AccessDenied",
                message = "У вас недостаточно прав для выполнения этого действия",
                detail = exception.Message // Можно убрать в продакшене
            };

            var jsonResponse = JsonConvert.SerializeObject(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}

