using Microsoft.EntityFrameworkCore;

namespace StockMarketAssistant.AuthService.WebApi.Helper
{
    public static class MigrationManager
    {
        public static void MigrateDatabase<TDbContext>(this IHost host) where TDbContext : DbContext
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;

            var logger = services.GetRequiredService<ILogger<TDbContext>>();
            var context = services.GetRequiredService<TDbContext>();

            logger.LogInformation("⏳ Начата проверка и применение миграций для контекста {DbContext}...", typeof(TDbContext).Name);

            if (!context.Database.CanConnect())
            {
                logger.LogWarning("⚠️ Не удалось подключиться к базе данных для {DbContext}.", typeof(TDbContext).Name);
                return;
            }

            logger.LogInformation("🔌 Успешное подключение к базе данных для {DbContext}.", typeof(TDbContext).Name);

            try
            {
                var pendingMigrations = context.Database.GetPendingMigrations().ToList();

                if (!pendingMigrations.Any())
                {
                    logger.LogInformation("✅ Все миграции уже применены. База данных актуальна.");
                    return;
                }

                logger.LogInformation("📦 Обнаружены не применённые миграции:");

                foreach (var migration in pendingMigrations)
                {
                    logger.LogInformation("➡️ {Migration}", migration);
                }

                context.Database.Migrate();
                logger.LogInformation("✅ Миграции успешно применены для {DbContext}.", typeof(TDbContext).Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "❌ Ошибка при применении миграций для {DbContext}.", typeof(TDbContext).Name);
                throw;
            }
        }
    }
}
