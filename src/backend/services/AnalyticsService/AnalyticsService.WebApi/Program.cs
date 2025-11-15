using Microsoft.EntityFrameworkCore;
using NSwag.AspNetCore;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Context;

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

            // Регистрация контроллеров
            builder.Services.AddControllers();

            // Настройка OpenAPI/Swagger
            builder.Services.AddOpenApiDocument(options =>
            {
                options.Title = "Analytics Service API Doc";
                options.Version = "1.0";
            });

            var app = builder.Build();

            // Автоматическое применение миграций
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<DatabaseContext>();
                if (dbContext is not null)
                {
                    try
                    {
                        dbContext.Database.Migrate(); // Применяет все pending миграции
                    }
                    catch
                    {
                        // Игнорируем ошибки миграций при первом запуске без базы данных
                    }
                }
            }

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseOpenApi();
                app.UseSwaggerUi(x =>
                {
                    x.DocExpansion = "list";
                });
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.MapDefaultEndpoints();
            app.MapControllers();
            app.MapGet("/", () => "Analytics Service API - Use /swagger for API documentation");

            app.Run();
        }
    }
}
