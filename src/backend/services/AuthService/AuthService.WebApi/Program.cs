using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.AuthService.Infrastructure.EntityFramework;
using StockMarketAssistant.AuthService.WebApi.Helper;

namespace StockMarketAssistant.AuthService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                options.UseNpgsql(connectionString);
            });

            builder.Services.AddControllers();

            builder.Services.AddOpenApi();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapGet("/", () => Results.Redirect("/swagger"));
                app.MapOpenApi();
                app.UseSwagger();
                app.UseSwaggerUI();

                app.MigrateDatabase<AppDbContext>();
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
