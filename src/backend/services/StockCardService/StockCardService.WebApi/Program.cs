using StockCardService.Infrastructure.EntityFramework;

namespace StockCardService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();

            var app = builder.Build();

            //app.MapGet("/", () => "Hello World!");

            var StockCardServisConnectionString = builder.Configuration.GetConnectionString("StockCardDb");
            DbInitializer.Initialize(StockCardServisConnectionString);
            

            app.Run();
        }
    }
}
