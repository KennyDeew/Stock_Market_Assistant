using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework;

namespace StockMarketAssistant.AnalyticsService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Добавляем сервисы Aspire
            builder.AddServiceDefaults();

            // Получаем строку подключения из Aspire
            var connectionString = builder.Configuration.GetConnectionString("analytics-db");

            // Регистрируем DbContext (EF Core)
            if (connectionString is not null)
            {
                builder.Services.ConfigureContext(connectionString);
            }

            var app = builder.Build();

            app.MapDefaultEndpoints();
            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
