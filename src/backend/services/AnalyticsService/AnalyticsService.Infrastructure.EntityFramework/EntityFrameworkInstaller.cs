using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Context;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework
{
    public static class EntityFrameworkInstaller
    {
        public static IServiceCollection ConfigureContext(
            this IServiceCollection services,
            string connectionString)
        {
            // Регистрация DbContext
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseSnakeCaseNamingConvention();
                // options.UseLazyLoadingProxies();
            });

            return services;
        }
    }
}



