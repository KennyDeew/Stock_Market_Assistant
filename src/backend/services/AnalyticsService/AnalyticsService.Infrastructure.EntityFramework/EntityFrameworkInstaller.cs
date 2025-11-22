using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework
{
    /// <summary>
    /// Установщик для настройки Entity Framework Core
    /// </summary>
    public static class EntityFrameworkInstaller
    {
        /// <summary>
        /// Настройка контекста базы данных
        /// </summary>
        /// <param name="services">Коллекция сервисов</param>
        /// <param name="connectionString">Строка подключения к базе данных</param>
        /// <returns>Коллекция сервисов</returns>
        public static IServiceCollection ConfigureContext(
            this IServiceCollection services,
            string connectionString)
        {
            // Регистрация DbContext
            services.AddDbContext<AnalyticsDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseSnakeCaseNamingConvention();
                // Отключаем предупреждение о незафиксированных изменениях модели
                options.ConfigureWarnings(warnings =>
                    warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                // options.UseLazyLoadingProxies();
            });

            return services;
        }
    }
}



