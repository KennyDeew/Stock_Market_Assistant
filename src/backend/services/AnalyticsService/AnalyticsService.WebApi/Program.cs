using Microsoft.EntityFrameworkCore;
using NSwag.AspNetCore;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Context;
using Microsoft.EntityFrameworkCore.Storage;

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
                    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
                    try
                    {
                        logger?.LogInformation("Проверка подключения к базе данных...");

                        // Проверяем подключение
                        if (!dbContext.Database.CanConnect())
                        {
                            logger?.LogWarning("Не удалось подключиться к базе данных. Миграции будут применены при следующем успешном подключении.");
                            return;
                        }

                        // Проверяем, существует ли таблица истории миграций, и создаем её, если нет
                        try
                        {
                            // Пытаемся прочитать из таблицы истории миграций
                            var _ = dbContext.Database.ExecuteSqlRaw("SELECT 1 FROM \"__EFMigrationsHistory\" LIMIT 1;");
                        }
                        catch
                        {
                            // Таблица не существует, создаем её
                            logger?.LogInformation("Таблица истории миграций не найдена. Создание таблицы...");
                            dbContext.Database.ExecuteSqlRaw(@"
                                CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                                    ""migration_id"" VARCHAR(150) NOT NULL,
                                    ""product_version"" VARCHAR(32) NOT NULL,
                                    CONSTRAINT ""pk___ef_migrations_history"" PRIMARY KEY (""migration_id"")
                                );");
                            logger?.LogInformation("Таблица истории миграций создана.");
                        }

                        logger?.LogInformation("Применение миграций базы данных...");

                        // Применяем миграции - они создадут таблицы, если их нет
                        dbContext.Database.Migrate();

                        logger?.LogInformation("Миграции успешно применены.");
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибку, но не прерываем запуск приложения
                        logger?.LogError(ex, "Не удалось применить миграции базы данных. Проверьте подключение к базе данных и настройки.");
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
