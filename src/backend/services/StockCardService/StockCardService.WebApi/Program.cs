using StockCardService.Infrastructure.EntityFramework;

namespace StockMarketAssistant.StockCardService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();

            var app = builder.Build();

            //app.MapGet("/", () => "Hello World!");

            var StockCardServiceConnectionString = builder.Configuration.GetConnectionString("StockCardDb");
            DbInitializer.Initialize(StockCardServiceConnectionString);
            

            app.Run();
        }
    }
}
