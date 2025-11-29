using Confluent.Kafka;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Caching;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Gateways;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Security;
using StockMarketAssistant.PortfolioService.Application.Services;
using StockMarketAssistant.PortfolioService.Infrastructure.BackgroundServices;
using StockMarketAssistant.PortfolioService.Infrastructure.Caching;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;
using StockMarketAssistant.PortfolioService.Infrastructure.Gateways;
using StockMarketAssistant.PortfolioService.Infrastructure.Repositories;
using StockMarketAssistant.PortfolioService.Infrastructure.Security;
using StockMarketAssistant.PortfolioService.WebApi.Infrastructure.Swagger;
using StockMarketAssistant.PortfolioService.WebApi.Middleware;
using StockMarketAssistant.PortfolioService.WebApi.Options;
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

            // Определяем, запущено ли приложение в тестовом контексте
            bool isRunningIntegrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS") == "1";

            if (!isRunningIntegrationTests)
            {
                // Добавляем Aspire-специфичные сервисы ТОЛЬКО если НЕ в тестах
                builder.AddServiceDefaults();
            }

            // Установка кодировки консоли
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            // Настройка логгера
            builder.Logging.AddConsole();
            builder.Logging.SetMinimumLevel(LogLevel.Information);

            // Add services to the container.
            var jwtSettings = builder.Configuration.GetSection("Jwt");

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme) // указание схемы аутентификации по умолчанию, именно по ней и будет происходить аутентификация
                .AddJwtBearer(options => // регистрация Jwt-схемы аутентификации
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
            builder.Services.AddScoped<IUserContext, UserContext>();
            builder.Services.AddAuthorization();

            // Регистрируем DbContext (EF Core) — только в production
            if (!isRunningIntegrationTests)
            {
                // Получаем строку подключения из Aspire
                var connectionString = builder.Configuration.GetConnectionString("portfolio-db");

                // Регистрируем NpgsqlDataSource (для низкоуровневых запросов)
                if (connectionString is not null)
                {
                    // Регистрируем DbContext (EF Core)
                    builder.Services.ConfigureContext(connectionString);
                }
            }

            // Регистрация сервисов в DI

            if (!isRunningIntegrationTests)
            {
                // Регистрация распределённого кэша Redis  — только в production
                builder.Services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = builder.Configuration.GetConnectionString("cache");
                });
                builder.Services.AddScoped<ICacheService, RedisCacheService>();
            }
            else
            {
                // Tests: In-Memory кэш
                builder.Services.AddScoped<ICacheService, InMemoryCacheService>();
            }

            builder.Services.AddScoped<IPortfolioRepository, PortfolioRepository>();
            builder.Services.AddScoped<IPortfolioAssetRepository, PortfolioAssetRepository>();
            builder.Services.AddScoped<IAlertRepository, AlertRepository>();
            builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();

            builder.Services.AddScoped<IPortfolioAppService, PortfolioAppService>();
            builder.Services.AddScoped<IPortfolioAssetAppService, PortfolioAssetAppService>();
            builder.Services.AddScoped<IAlertAppService, AlertAppService>();

            // Настройка Kafka Producer

            var kafkaConnectionString = builder.Configuration.GetConnectionString("kafka");
            if (string.IsNullOrEmpty(kafkaConnectionString))
            {
                var options = builder.Configuration.Get<ApplicationOptions>();
                // Fallback к appsettings.json
                kafkaConnectionString = options?.KafkaOptions?.BootstrapServers
                    ?? "kafka:9092";
            }

            builder.Services.AddSingleton(provider =>
            {
                var config = new ProducerConfig
                {
                    BootstrapServers = kafkaConnectionString,
                    Acks = Acks.All,
                    MessageSendMaxRetries = 3,
                    RetryBackoffMs = 1000,
                    LingerMs = 5,
                    BatchSize = 16384,
                    EnableIdempotence = true
                };
                return new ProducerBuilder<Null, string>(config).Build();
            });

            // Background Services
            builder.Services.AddHostedService<AlertProcessingService>();
            builder.Services.AddHostedService<KafkaOutboxProcessor>();

            var httpClientBuilder = builder.Services.AddHttpClient<IStockCardServiceGateway, StockCardServiceGateway>(httpClient =>
            {
                httpClient.BaseAddress = new Uri("http://stockcardservice-api");
            });

            if (!isRunningIntegrationTests)
            {
                httpClientBuilder.AddServiceDiscovery();
            }

            // Читаем origin из переменной окружения
            var frontendOrigin = builder.Configuration["FRONTEND_ORIGIN"] ?? "http://localhost:5173";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontendApp", policy =>
                {
                    policy
                        .WithOrigins(frontendOrigin) // Разрешить источник фронтенда
                        .AllowAnyHeader()            // Любой заголовок
                        .AllowAnyMethod();           // GET, POST, PUT, DELETE
                });
            });

            // Добавляем поддержку FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();
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

            // Получаем сервис для отслеживания жизненного цикла
            var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
            var logger = app.Services.GetRequiredService<ILogger<Program>>();

            // Регистрация обработчиков событий

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

            // Автоматическое применение миграций — только в production
            if (!isRunningIntegrationTests)
            {
                using var scope = app.Services.CreateScope();
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

            app.UseRouting();
            app.UseCors("AllowFrontendApp");

            if (!isRunningIntegrationTests)
            {
                app.MapDefaultEndpoints();
            }

            app.UseMiddleware<SecurityExceptionMiddleware>();
            //app.UseHttpsRedirection();

            // Аутентификация и авторизация
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
