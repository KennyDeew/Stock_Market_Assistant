using Autofac;
using Confluent.Kafka;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Serilog.Sinks.OpenSearch;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Caching;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Repositories;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Security;
using StockMarketAssistant.PortfolioService.Application.Services;
using StockMarketAssistant.PortfolioService.Infrastructure.Caching;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework;
using StockMarketAssistant.PortfolioService.Infrastructure.Repositories;
using StockMarketAssistant.PortfolioService.Infrastructure.Security;
using StockMarketAssistant.PortfolioService.WebApi.Options;
using ILoggerFactory = Microsoft.Extensions.Logging.ILoggerFactory;
using OptionsExtensions = Microsoft.Extensions.Options;

namespace StockMarketAssistant.PortfolioService.WebApi.Infrastructure.Autofac;

/// <summary>
/// Модуль Autofac для регистрации всех зависимостей приложения.
/// Обеспечивает централизованную настройку DI-контейнера с поддержкой
/// как production-окружения (PostgreSQL, Redis, Kafka), так и интеграционных тестов (InMemory).
/// </summary>
/// <remarks>
/// Инициализирует новый экземпляр модуля Autofac с указанными настройками конфигурации.
/// </remarks>
/// <param name="configuration">Конфигурация приложения</param>
public class AutofacModule(IConfiguration configuration) : Module
{
    // Общий InMemoryDatabaseRoot для ВСЕХ тестов в сборке
    // Обеспечивает общий пул данных между запросами
    private static readonly InMemoryDatabaseRoot _databaseRoot = new();
    private readonly IConfiguration _configuration = configuration;
    private readonly bool _isRunningIntegrationTests = Environment.GetEnvironmentVariable("INTEGRATION_TESTS") == "1";

    /// <summary>
    /// Загружает регистрации сервисов в контейнер Autofac.
    /// Метод вызывается автоматически при инициализации контейнера.
    /// </summary>
    /// <param name="builder">Строитель контейнера Autofac</param>
    protected override void Load(ContainerBuilder builder)
    {
        // Настройка Serilog
        ConfigureSerilog(builder);

        // HttpContextAccessor
        builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().InstancePerLifetimeScope();

        // UserContext
        builder.RegisterType<UserContext>().As<IUserContext>().InstancePerLifetimeScope();

        // DbContext
        RegisterDbContext(builder);

        // Кэш
        RegisterCache(builder);

        // Репозитории
        builder.RegisterType<PortfolioRepository>().As<IPortfolioRepository>().InstancePerLifetimeScope();
        builder.RegisterType<PortfolioAssetRepository>().As<IPortfolioAssetRepository>().InstancePerLifetimeScope();
        builder.RegisterType<AlertRepository>().As<IAlertRepository>().InstancePerLifetimeScope();
        builder.RegisterType<OutboxRepository>().As<IOutboxRepository>().InstancePerLifetimeScope();

        // Сервисы приложения
        builder.RegisterType<PortfolioAppService>().As<IPortfolioAppService>().InstancePerLifetimeScope();
        builder.RegisterType<PortfolioAssetAppService>().As<IPortfolioAssetAppService>().InstancePerLifetimeScope();
        builder.RegisterType<AlertAppService>().As<IAlertAppService>().InstancePerLifetimeScope();

        // Kafka Producer
        RegisterKafka(builder);

        // FluentValidation
        builder.RegisterAssemblyTypes(ThisAssembly)
            .Where(t => t.IsClosedTypeOf(typeof(IValidator<>)))
            .AsImplementedInterfaces()
            .InstancePerDependency();
    }

    /// <summary>
    /// Настраивает Serilog с поддержкой OpenSearch и консольного вывода
    /// </summary>
    /// <param name="builder">Строитель контейнера Autofac</param>
    private void ConfigureSerilog(ContainerBuilder builder)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName()
            .Enrich.WithProperty("Application", "PortfolioService");

        if (!_isRunningIntegrationTests)
        {
            // В production режиме добавляем OpenSearch
            var openSearchUrl = _configuration["OpenSearchConfig:Url"] ?? "http://opensearch:9200";

            var openSearchOptions = new OpenSearchSinkOptions(new Uri(openSearchUrl))
            {
                IndexFormat = "portfolioservice-logs-{0:yyyy.MM.dd}",
                ModifyConnectionSettings = x => x.BasicAuthentication("admin", "admin"),
                AutoRegisterTemplate = true,
                TemplateName = "serilog-events-template"
            };

            loggerConfiguration
                .WriteTo.OpenSearch(openSearchOptions)
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
        }
        else
        {
            // В тестовом режиме только консоль
            loggerConfiguration
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
        }

        // Создаём Serilog logger
        var logger = loggerConfiguration.CreateLogger();
        Log.Logger = logger;

        // Регистрируем Serilog как поставщика для Microsoft.Extensions.Logging
        builder.RegisterInstance(new SerilogLoggerFactory(logger))
               .As<ILoggerFactory>()
               .SingleInstance();

        // Универсальная регистрация ILogger<T> для любого типа
        builder.RegisterGeneric(typeof(Logger<>))
               .As(typeof(ILogger<>))
               .InstancePerLifetimeScope();
    }

    /// <summary>
    /// Регистрирует DbContext для Entity Framework Core.
    /// В production-режиме используется PostgreSQL, в тестах — InMemoryDatabase.
    /// Если строка подключения отсутствует в production, используется InMemory с предупреждением.
    /// </summary>
    /// <param name="builder">Строитель контейнера Autofac</param>
    private void RegisterDbContext(ContainerBuilder builder)
    {
        if (!_isRunningIntegrationTests)
        {
            var connectionString = _configuration.GetConnectionString("portfolio-db");
            if (!string.IsNullOrEmpty(connectionString))
            {
                // Используем метод расширения для Autofac
                builder.ConfigureContext(connectionString);
            }
            else
            {
                // Строка подключения отсутствует - используем InMemory с предупреждением
                Log.Warning(
                    "Строка подключения 'portfolio-db' не найдена в конфигурации. " +
                    "Используется InMemory база данных. Это не рекомендуется для production окружения!");

                builder.ConfigureInMemoryContext(_databaseRoot);
            }
        }
        else
        {
            // метод расширения для InMemory
            builder.ConfigureInMemoryContext(_databaseRoot);
        }
    }

    /// <summary>
    /// Регистрирует сервис кэширования.
    /// В production-режиме используется Redis, в тестах — InMemory кэш.
    /// </summary>
    /// <param name="builder">Строитель контейнера Autofac</param>
    private void RegisterCache(ContainerBuilder builder)
    {
        if (!_isRunningIntegrationTests)
        {
            // Правильная регистрация Redis через IDistributedCache
            builder.Register(c =>
            {
                var configuration = _configuration.GetConnectionString("cache");
                var options = new RedisCacheOptions
                {
                    Configuration = configuration
                };
                return new RedisCache(OptionsExtensions.Options.Create(options));
            }).As<IDistributedCache>().SingleInstance();

            // Затем используем IDistributedCache в RedisCacheService
            builder.RegisterType<RedisCacheService>().As<ICacheService>().InstancePerLifetimeScope();
        }
        else
        {
            // InMemory Cache для тестов
            builder.RegisterType<InMemoryCacheService>().As<ICacheService>().InstancePerLifetimeScope();
        }
    }

    /// <summary>
    /// Регистрирует Kafka Producer для отправки сообщений в брокер.
    /// Строка подключения берётся из конфигурации, с fallback на значение по умолчанию.
    /// </summary>
    /// <param name="builder">Строитель контейнера Autofac</param>
    private void RegisterKafka(ContainerBuilder builder)
    {
        var kafkaConnectionString = _configuration.GetConnectionString("kafka");
        if (string.IsNullOrEmpty(kafkaConnectionString))
        {
            var options = _configuration.Get<ApplicationOptions>();
            kafkaConnectionString = options?.KafkaOptions?.BootstrapServers ?? "kafka:9092";
        }

        builder.Register(c =>
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
        }).SingleInstance();
    }
}