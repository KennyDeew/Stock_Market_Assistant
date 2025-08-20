using Microsoft.EntityFrameworkCore;

namespace StockMarketAssistant.StockCardService.WebApi.Helper
{
    public static class MigrationManager
    {
        public static void MigrateDatabase<TDbContext>(this IHost host) where TDbContext : DbContext
        {
            var scope = host.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TDbContext>();
            context.Database.Migrate();
        }
    }
}
