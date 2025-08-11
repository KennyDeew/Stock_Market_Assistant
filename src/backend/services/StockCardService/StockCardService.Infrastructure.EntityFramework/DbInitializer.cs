using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;

namespace StockCardService.Infrastructure.EntityFramework
{
    public static class DbInitializer
    {
        public static void Initialize(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<StockCardDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            
            using (var context = new StockCardDbContext(optionsBuilder.Options))
            {
                context.Database.Migrate();
            }
        }
    }
}

