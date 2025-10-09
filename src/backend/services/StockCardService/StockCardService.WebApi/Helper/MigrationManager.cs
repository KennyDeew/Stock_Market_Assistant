using Microsoft.EntityFrameworkCore;

namespace StockMarketAssistant.StockCardService.WebApi.Helper
{
    /// <summary>
    /// 
    /// </summary>
    public static class MigrationManager
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TDbContext"></typeparam>
        /// <param name="host"></param>
        public static void MigrateDatabase<TDbContext>(this IHost host) where TDbContext : DbContext
        {
            var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            context.Database.Migrate();
        }
    }
}
