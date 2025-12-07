using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StockCardService.Abstractions.Repositories;
using StockCardService.Infrastructure.EntityFramework;
using StockCardService.Infrastructure.Integrations.Moex;
using StockCardService.Infrastructure.Messaging.Kafka;
using StockCardService.Infrastructure.Messaging.Kafka.Options;
using StockCardService.Infrastructure.Repositories;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.Application.Services;
using StockMarketAssistant.StockCardService.Domain.Entities;
using StockMarketAssistant.StockCardService.Domain.Interfaces;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;
using StockMarketAssistant.StockCardService.Infrastructure.MongoDb;
using StockMarketAssistant.StockCardService.Infrastructure.MongoDb.Settings;
using StockMarketAssistant.StockCardService.Infrastructure.Repositories;
using StockMarketAssistant.StockCardService.WebApi.Helper;
using StockMarketAssistant.StockCardService.WebApi.Hubs;
using StockMarketAssistant.StockCardService.WebApi.BackgroundServices;

namespace StockMarketAssistant.StockCardService.WebApi
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Точка входа в приложение. Настройка сервисов, middleware. Запуск веб-приложения.
        /// </summary>
        /// <param name="args"></param>
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.AddServiceDefaults();

            //EntityFramework. Настройки подключения пробрасываем из Aspire. Aspire пробрасывает connection string (по имени базы).
            builder.Services.AddDbContext<StockCardDbContext>(options =>
            {
                options.UseNpgsql(builder.Configuration.GetConnectionString("stock-card-db"),
                    optionsBuilder => optionsBuilder.MigrationsAssembly("StockCardService.Infrastructure.EntityFramework"));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            //MongoDb. Настройки подключения пробрасываем из Aspire. Aspire пробрасывает connection string (по имени базы).
            builder.Services.Configure<MongoSettings>(options =>
            {
                var connStr = builder.Configuration.GetConnectionString("finantial-report-db");

                if (string.IsNullOrEmpty(connStr))
                {
                    throw new InvalidOperationException("Mongo connection string not provided by Aspire");
                }

                options.ConnectionString = connStr;
                options.DatabaseName = "finantial-report-db";
            });

            //Настройка конфигурации Kafka
            builder.Services.Configure<KafkaOptions>(
                builder.Configuration.GetSection("KafkaOptions"));

            //Найстройка CORs
            builder.Services.AddCors(opt =>
            {
                opt.AddPolicy("AllowFrontend", policy =>
                {
                    policy
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials()
                        .WithOrigins("http://localhost:3000");
                });
            });

            // Настройка логирования
            // ----------------------------------------------
            builder.Logging.ClearProviders();   // Убираем стандартные провайдеры (например, Debug)
            builder.Logging.AddConsole();       // Добавляем логирование в консоль
            builder.Logging.AddDebug();         // (опционально) логирование в Visual Studio Output

            //Добавление SignalR
            builder.Services.AddSignalR();

            //IOC
            builder.Services.AddControllers();
            builder.Services.AddScoped(typeof(IRepository<ShareCard, Guid>), typeof(ShareCardRepository));
            builder.Services.AddScoped(typeof(IRepository<BondCard, Guid>), typeof(BondCardRepository));
            builder.Services.AddScoped(typeof(IRepository<CryptoCard, Guid>), typeof(CryptoCardRepository));
            builder.Services.AddScoped(typeof(ISubRepository<Dividend, Guid>), typeof(DividendRepository));
            builder.Services.AddScoped(typeof(ISubRepository<Coupon, Guid>), typeof(CouponRepository));
            builder.Services.AddScoped(typeof(IMongoRepository<FinancialReport, Guid>), typeof(FinancialReportRepository));
            builder.Services.AddHttpClient<IMoexCardService, MoexCardService>();
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();
            builder.Services.AddHttpClient<IStockPriceService, MoexStockPriceService>();
            builder.Services.AddSingleton<IMongoDBContext, MongoDBContext>();
            builder.Services.AddSingleton<IKafkaProducerFactory, KafkaProducerFactory>();
            builder.Services.AddSingleton<IKafkaProducer<string, FinancialReportCreatedMessage>, FinReportCreatedMessageProducer>();
            builder.Services.AddScoped<IShareCardService, ShareCardService>();
            builder.Services.AddScoped<IBondCardService, BondCardservice>();
            builder.Services.AddScoped<ICryptoCardService, CryptoCardService>();
            builder.Services.AddScoped<IDividendService, DividendService>();
            builder.Services.AddScoped<ICouponService, CouponService>();
            builder.Services.AddScoped<IFinancialReportService, FinancialReportService>();
            //builder.Services.AddSingleton<IPriceHubClient, PriceHubClient>();
            //Добавляем бэкграун сервис
            builder.Services.AddSingleton<PriceStreamingService>();
            builder.Services.AddHostedService(sp => sp.GetRequiredService<PriceStreamingService>());

            builder.Services.AddEndpointsApiExplorer(); // Для генерации OpenAPI spec
            // Добавляет Swagger-сервисы. Настраиваем xml разметку
            builder.Services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.MapGet("/", () => Results.Redirect("/swagger"));
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            //app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("AllowFrontend"); ;
            app.UseAuthorization();
            app.MapControllers();

            //настройка хаба SignalR
            app.MapHub<PriceHub>("/stockPriceHub");

            app.MigrateDatabase<StockCardDbContext>();

            using (var scope = app.Services.CreateScope())
            {
                var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                initializer.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
            }

            app.Run();
        }
    }
}
