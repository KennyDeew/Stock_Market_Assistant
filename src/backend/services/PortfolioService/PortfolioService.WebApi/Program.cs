using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Application.Services;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;
using StockMarketAssistant.PortfolioService.Infrastructure.Repositories;

namespace StockMarketAssistant.PortfolioService.WebApi
{
    public class Program
    {
        private static readonly string[] customControllersOrder = ["Portfolios", "PortfolioAssets"];

        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Добавляем сервисы Aspire
            builder.AddServiceDefaults();

            // Add services to the container.

            //if (builder.Environment.IsDevelopment())
            //{
            //    builder.Configuration.AddUserSecrets<Program>();
            //}

            // Получаем строку подключения из Aspire
            var connectionString = builder.Configuration.GetConnectionString("portfolio-db");
            
            // Регистрируем NpgsqlDataSource (для низкоуровневых запросов)
            if (connectionString is not null)
            {
                // Регистрируем DbContext (EF Core)
                builder.Services.ConfigureContext(connectionString);
            }

            // Регистрация сервисов в DI
            builder.Services.AddSingleton<IStockCardServiceClient, FakeStockCardServiceClient>(); // заглушка для сервиса StockCardService

            builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
            builder.Services.AddScoped<IPortfolioAssetRepository, PortfolioAssetRepository>();

            builder.Services.AddScoped<IPortfolioAppService, PortfolioAppService>();
            builder.Services.AddScoped<IPortfolioAssetAppService, PortfolioAssetAppService>();

            builder.Services.AddControllers();

            builder.Services.AddOpenApiDocument(options =>
            {
                options.Title = "Portfolio Service API Doc";
                options.Version = "1.0";
                options.PostProcess = (document) =>
                {
                    document.Tags = document.Tags?
                        .OrderBy(t =>
                        {
                            var index = Array.IndexOf(customControllersOrder, t.Name);
                            return index > -1 ? index : int.MaxValue;
                        }).ToList();
                };
            });

            var app = builder.Build();

            // Автоматическое применение миграций
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                dbContext.Database.Migrate(); // Применяет все pending миграции
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {

                app.UseOpenApi();
                app.UseSwaggerUi(x =>
                {
                    x.DocExpansion = "list";
                });

            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
