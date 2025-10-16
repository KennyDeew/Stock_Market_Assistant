using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Caching;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Gateways;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Application.Services;
using StockMarketAssistant.PortfolioService.Infrastructure.Caching;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;
using StockMarketAssistant.PortfolioService.Infrastructure.Gateways;
using StockMarketAssistant.PortfolioService.Infrastructure.Repositories;
using StockMarketAssistant.PortfolioService.WebApi.Infrastructure.Swagger;

namespace StockMarketAssistant.PortfolioService.WebApi
{
#pragma warning disable CS1591
    public class Program
#pragma warning restore CS1591
    {
        private static readonly string[] customControllersOrder = ["Portfolios", "PortfolioAssets"];

#pragma warning disable CS1591
        public static void Main(string[] args)
#pragma warning restore CS1591
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
            // Регистрация распределённого кэша Redis
            builder.Services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = builder.Configuration.GetConnectionString("cache");
            });

            builder.Services.AddScoped<ICacheService, RedisCacheService>();

            builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
            builder.Services.AddScoped<IPortfolioAssetRepository, PortfolioAssetRepository>();

            builder.Services.AddScoped<IPortfolioAppService, PortfolioAppService>();
            builder.Services.AddScoped<IPortfolioAssetAppService, PortfolioAssetAppService>();

            builder.Services.AddHttpClient<IStockCardServiceGateway, StockCardServiceGateway>(httpClient =>
            {
                httpClient.BaseAddress = new Uri("http://stockcardservice-api");
            })
            .AddServiceDiscovery();

            builder.Services.AddControllers();

            builder.Services.AddOpenApiDocument(config =>
            {
                config.SchemaSettings.SchemaProcessors.Add(new EnumDescriptionSchemaProcessor());
                config.SchemaSettings.SchemaProcessors.Add(new DefaultValueSchemaProcessor());
                config.PostProcess = (document) =>
                {
                    document.Info.Title = "Portfolio Service API";
                    document.Info.Version = "v1";
                    document.Tags = document.Tags?
                        .OrderBy(t =>
                        {
                            var index = Array.IndexOf(customControllersOrder, t.Name);
                            return index > -1 ? index : int.MaxValue;
                        }).ToList();
                };
            });

            var app = builder.Build();
            app.MapDefaultEndpoints();

            app.MapDefaultEndpoints();

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
