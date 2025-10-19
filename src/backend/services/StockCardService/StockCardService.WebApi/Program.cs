using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using StockCardService.Abstractions.Repositories;
using StockCardService.Infrastructure.EntityFramework;
using StockCardService.Infrastructure.Repositories;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.Application.Services;
using StockMarketAssistant.StockCardService.Domain.Entities;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework;
using StockMarketAssistant.StockCardService.Infrastructure.EntityFramework.Settings;
using StockMarketAssistant.StockCardService.Infrastructure.Repositories;
using StockMarketAssistant.StockCardService.WebApi.Helper;

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

            //IOC
            builder.Services.AddControllers();
            builder.Services.AddScoped(typeof(IRepository<ShareCard, Guid>), typeof(ShareCardRepository));
            builder.Services.AddScoped(typeof(IRepository<BondCard, Guid>), typeof(BondCardRepository));
            builder.Services.AddScoped(typeof(IRepository<CryptoCard, Guid>), typeof(CryptoCardRepository));
            builder.Services.AddScoped(typeof(ISubRepository<Dividend, Guid>), typeof(DividendRepository));
            builder.Services.AddScoped(typeof(ISubRepository<Coupon, Guid>), typeof(CouponRepository));
            builder.Services.AddScoped(typeof(IMongoRepository<FinancialReport, Guid>), typeof(FinancialReportRepository));
            builder.Services.AddSingleton<IMongoDBContext, MongoDBContext>();
            builder.Services.AddScoped<IShareCardService, ShareCardService>();
            builder.Services.AddScoped<IBondCardService, BondCardservice>();
            builder.Services.AddScoped<ICryptoCardService, CryptoCardService>();
            builder.Services.AddScoped<IDividendService, DividendService>();
            builder.Services.AddScoped<ICouponService, CouponService>();
            builder.Services.AddScoped<IFinancialReportService, FinancialReportService>();

            builder.Services.AddEndpointsApiExplorer(); // Для генерации OpenAPI spec
            // Добавляет Swagger-сервисы. Настраиваем xml разметку
            builder.Services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            var app = builder.Build();

            app.MapGet("/", () => Results.Redirect("/swagger"));

            //var StockCardServisConnectionString = builder.Configuration.GetConnectionString("pg-stock-card-db");
            //DbInitializer.Initialize(StockCardServisConnectionString);



            // Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.UseHttpsRedirection();


            app.UseRouting();
            app.UseCors();
            app.UseAuthorization();
            app.MapControllers();

            //app.MigrateDatabase<StockCardDbContext>();
            //Заполняем БД объектами из FakeDataFactory
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<StockCardDbContext>();
                DbInitializer.Initialize(dbContext);
            }
            app.Run();
        }
    }
}
