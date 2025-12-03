using Microsoft.EntityFrameworkCore;

namespace StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context
{
    /// <summary>
    /// Контекст БД
    /// </summary>
    public class DatabaseContext(DbContextOptions<DatabaseContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DatabaseContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.LogTo(Console.WriteLine, LogLevel.Information);
        //}
    }
}
