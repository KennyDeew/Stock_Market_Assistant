using Confluent.Kafka;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NSwag.AspNetCore;
using Polly;
using Polly.Extensions.Http;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;
using StockMarketAssistant.AnalyticsService.Domain.Events;
using StockMarketAssistant.AnalyticsService.Domain.Services;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Events;
using StockMarketAssistant.AnalyticsService.Application.Services;
using StockMarketAssistant.AnalyticsService.Application.UseCases;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Events.Handlers;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Http;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Jobs;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Kafka;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence;
using Microsoft.EntityFrameworkCore.Storage;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.OpenSearch;
using AutoRegisterTemplateVersion = Serilog.Sinks.OpenSearch.AutoRegisterTemplateVersion;
using CertificateValidations = OpenSearch.Net.CertificateValidations;

namespace StockMarketAssistant.AnalyticsService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Очищаем стандартные провайдеры логирования
            builder.Logging.ClearProviders();

            // Включаем отладочное логирование Serilog
            Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));

            // Добавляем сервисы Aspire
            builder.AddServiceDefaults();

            // Настройка Serilog с OpenSearch
            var username = builder.Configuration.GetSection("OpenSearchConfig:Username").Value;
            var password = builder.Configuration.GetSection("OpenSearchConfig:Password").Value;
            var openSearchUrl = builder.Configuration.GetSection("OpenSearchConfig:Url").Value ?? "https://localhost:9200";

            var serilogLogger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri(openSearchUrl))
                {
                    AutoRegisterTemplateVersion = AutoRegisterTemplateVersion.OSv1,
                    MinimumLogEventLevel = LogEventLevel.Verbose,
                    TypeName = "_doc",
                    InlineFields = false,
                    ModifyConnectionSettings = x =>
                        x.BasicAuthentication(username, password)
                            .ServerCertificateValidationCallback(CertificateValidations.AllowAll)
                            .ServerCertificateValidationCallback((o, certificate, chain, errors) => true),
                    IndexFormat = "analytics-service-{0:yyyy.MM.dd}",
                })
                .CreateLogger();

            // Добавляем Serilog в систему логирования
            builder.Logging.AddSerilog(serilogLogger);

            // Получаем строку подключения из Aspire
            var connectionString = builder.Configuration.GetConnectionString("analytics-db");

            // Регистрируем DbContext (EF Core)
            if (connectionString is not null)
            {
                builder.Services.ConfigureContext(connectionString);
            }

            // Регистрация FluentValidation
            builder.Services.AddValidatorsFromAssemblyContaining<Application.Validators.GetTopAssetsRequestValidator>();
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddFluentValidationClientsideAdapters();

            // Регистрация контроллеров
            builder.Services.AddControllers();

            // Настройка OpenAPI/Swagger
            builder.Services.AddOpenApiDocument(options =>
            {
                options.Title = "Analytics Service API Doc";
                options.Version = "1.0";
            });

            // Конфигурация Kafka
            builder.Services.Configure<KafkaConfiguration>(
                builder.Configuration.GetSection("Kafka"));

            // Регистрация Kafka Consumer и Producer через Aspire
            builder.AddKafkaProducer<string, string>("kafka");
            builder.AddKafkaConsumer<string, string>("kafka", options =>
            {
                var kafkaConfig = builder.Configuration.GetSection("Kafka").Get<KafkaConfiguration>();
                if (kafkaConfig != null)
                {
                    options.Config.GroupId = kafkaConfig.ConsumerGroup;
                    options.Config.AutoOffsetReset = AutoOffsetReset.Earliest;
                    options.Config.EnableAutoCommit = false;
                }
            });

            // Регистрация Domain Services
            builder.Services.AddScoped<RatingCalculationService>();

            // Регистрация Application Services
            builder.Services.AddScoped<AssetRatingAggregationService>();

            // Регистрация Use Cases
            builder.Services.AddScoped<GetTopBoughtAssetsUseCase>();
            builder.Services.AddScoped<GetTopSoldAssetsUseCase>();
            builder.Services.AddScoped<GetPortfolioHistoryUseCase>();
            builder.Services.AddScoped<ComparePortfoliosUseCase>();

            // Регистрация EventBus как Singleton
            builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

            // Регистрация Event Handlers
            builder.Services.AddScoped<IEventHandler<TransactionReceivedEvent>, TransactionReceivedEventHandler>();

            // Регистрация Memory Cache
            builder.Services.AddMemoryCache();

            // Регистрация HTTP клиента для PortfolioService
            var portfolioServiceUrl = builder.Configuration.GetSection("PortfolioService:BaseUrl").Value
                ?? builder.Configuration["Services:PortfolioService"]
                ?? "http://localhost:5000";

            builder.Services.AddHttpClient<IPortfolioServiceClient, PortfolioServiceClient>(client =>
            {
                client.BaseAddress = new Uri(portfolioServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

            // Регистрация Background Services
            builder.Services.AddHostedService<TransactionConsumer>();
            builder.Services.AddHostedService<AssetRatingAggregationJob>();

            var app = builder.Build();

            // Подписка обработчиков на события после построения приложения
            var eventBus = app.Services.GetRequiredService<IEventBus>();
            eventBus.Subscribe<TransactionReceivedEvent, TransactionReceivedEventHandler>();

            // Автоматическое применение миграций
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetService<AnalyticsDbContext>();
                if (dbContext is not null)
                {
                    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
                    try
                    {
                        logger?.LogInformation("Проверка подключения к базе данных...");

                        // Проверяем подключение
                        if (!dbContext.Database.CanConnect())
                        {
                            logger?.LogWarning("Не удалось подключиться к базе данных. Миграции будут применены при следующем успешном подключении.");
                            return;
                        }

                        // Проверяем, существует ли таблица истории миграций, и создаем её, если нет
                        try
                        {
                            // Пытаемся прочитать из таблицы истории миграций
                            var _ = dbContext.Database.ExecuteSqlRaw("SELECT 1 FROM \"__EFMigrationsHistory\" LIMIT 1;");
                        }
                        catch
                        {
                            // Таблица не существует, создаем её
                            logger?.LogInformation("Таблица истории миграций не найдена. Создание таблицы...");
                            dbContext.Database.ExecuteSqlRaw(@"
                                CREATE TABLE IF NOT EXISTS ""__EFMigrationsHistory"" (
                                    ""migration_id"" VARCHAR(150) NOT NULL,
                                    ""product_version"" VARCHAR(32) NOT NULL,
                                    CONSTRAINT ""pk___ef_migrations_history"" PRIMARY KEY (""migration_id"")
                                );");
                            logger?.LogInformation("Таблица истории миграций создана.");
                        }

                        logger?.LogInformation("Применение миграций базы данных...");

                        // Применяем миграции - они создадут таблицы, если их нет
                        dbContext.Database.Migrate();

                        logger?.LogInformation("Миграции успешно применены.");
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибку, но не прерываем запуск приложения
                        logger?.LogError(ex, "Не удалось применить миграции базы данных. Проверьте подключение к базе данных и настройки.");
                    }
                }
            }

            // Configure the HTTP request pipeline
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

            app.MapDefaultEndpoints();
            app.MapControllers();
            app.MapGet("/", () => "Analytics Service API - Use /swagger for API documentation");

            app.Run();
        }

        /// <summary>
        /// Получить политику Retry для HTTP клиента
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        // Логирование будет в PortfolioServiceClient
                    });
        }

        /// <summary>
        /// Получить политику Circuit Breaker для HTTP клиента
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    handledEventsAllowedBeforeBreaking: 5,
                    durationOfBreak: TimeSpan.FromSeconds(30));
        }
    }
}
