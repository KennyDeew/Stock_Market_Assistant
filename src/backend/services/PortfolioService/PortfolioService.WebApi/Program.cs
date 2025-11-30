using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Gateways;
using StockMarketAssistant.PortfolioService.Infrastructure.BackgroundServices;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;
using StockMarketAssistant.PortfolioService.Infrastructure.Gateways;
using StockMarketAssistant.PortfolioService.WebApi.Infrastructure.Autofac;
using StockMarketAssistant.PortfolioService.WebApi.Infrastructure.Swagger;
using StockMarketAssistant.PortfolioService.WebApi.Middleware;
using System.Text;

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

            // Настройка Autofac (включая Serilog)
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterModule(new AutofacModule(builder.Configuration));
            });

            // Определяем, запущено ли приложение в тестовом контексте
            bool isRunningIntegrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS") == "1";

            if (!isRunningIntegrationTests)
            {
                builder.AddServiceDefaults();
            }

            // Установка кодировки консоли
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            // Add services to the container.
            var jwtSettings = builder.Configuration.GetSection("Jwt");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        RoleClaimType = "Role",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
                    };
                });

            builder.Services.AddHttpContextAccessor();
            builder.Services.AddAuthorization();

            // Background Services
            builder.Services.AddHostedService<AlertProcessingService>();
            builder.Services.AddHostedService<KafkaOutboxProcessor>();

            // Регистрация HttpClient и gateway для взаимодействия с сервисом карточек ценных бумаг
            var httpClientBuilder = builder.Services.AddHttpClient<IStockCardServiceGateway, StockCardServiceGateway>(httpClient =>
            {
                httpClient.BaseAddress = new Uri("http://stockcardservice-api");
            });

            if (!isRunningIntegrationTests)
            {
                httpClientBuilder.AddServiceDiscovery();
            }

            // CORS
            var frontendOrigin = builder.Configuration["FRONTEND_ORIGIN"] ?? "http://localhost:5173";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontendApp", policy =>
                {
                    policy
                        .WithOrigins(frontendOrigin)
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            // Controllers
            builder.Services.AddControllers();

            // OpenAPI
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

            // Lifecycle events - используем Serilog из Autofac
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            lifetime.ApplicationStarted.Register(() =>
            {
                logger.LogInformation("=== Сервис портфелей успешно запущен ===");
                logger.LogInformation("Время запуска: {StartUpTime}", DateTime.UtcNow);
            });

            lifetime.ApplicationStopping.Register(() =>
            {
                logger.LogWarning("=== Сервис портфелей останавливается ===");
                logger.LogInformation("Выполнение завершающих операций...");
            });

            lifetime.ApplicationStopped.Register(() =>
            {
                logger.LogInformation("=== Сервис портфелей полностью остановлен ===");
                logger.LogInformation("Время остановки: {ShutdownTime}", DateTime.UtcNow);
            });

            // Migrations
            if (!isRunningIntegrationTests)
            {
                using var scope = app.Services.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
                dbContext.Database.Migrate();
            }

            // Pipeline
            if (app.Environment.IsDevelopment())
            {
                app.UseOpenApi();
                app.UseSwaggerUi(x => x.DocExpansion = "list");
            }

            app.UseRouting();
            app.UseCors("AllowFrontendApp");

            if (!isRunningIntegrationTests)
            {
                app.MapDefaultEndpoints();
            }

            app.UseMiddleware<SecurityExceptionMiddleware>();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}