using Microsoft.EntityFrameworkCore;
using StockCardService.Abstractions.Repositories;
using StockCardService.Infrastructure.EntityFramework;
using StockCardService.Infrastructure.Repositories;
using StockMarketAssistant.StockCardService.Domain.Entities;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;

namespace StockMarketAssistant.StockCardService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();

            //EntityFramework
            builder.Services.AddDbContext<StockCardDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("StockCardDb"),
                    optionsBuilder => optionsBuilder.MigrationsAssembly("Infrastructure.EntityFramework"));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            builder.Services.AddControllers();
            builder.Services.AddScoped(typeof(IRepository<ShareCard, Guid>), typeof(ShareCardRepository));
            builder.Services.AddScoped(typeof(IRepository<BondCard, Guid>), typeof(BondCardRepository));
            builder.Services.AddScoped(typeof(IRepository<CryptoCard, Guid>), typeof(CryptoCardRepository));

            var app = builder.Build();

            //app.MapGet("/", () => "Hello World!");

            //var StockCardServisConnectionString = builder.Configuration.GetConnectionString("StockCardDb");
            //DbInitializer.Initialize(StockCardServisConnectionString);
            

            app.Run();
        }
    }
}
