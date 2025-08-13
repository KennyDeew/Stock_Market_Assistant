using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Context;
using StockMarketAssistant.AnalyticsService.Infrastructure.Repositories;
using StockMarketAssistant.AnalyticsService.Application.Services;

// Настройки для отключения проблемных функций рефлексии в .NET 9
AppContext.SetSwitch("System.Reflection.Metadata.MetadataUpdater.IsSupported", false);
AppContext.SetSwitch("System.Reflection.Metadata.MetadataUpdater.IsSupportedInAppHost", false);

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Настройка Swagger с исправлениями для .NET 9
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Analytics Service API",
        Version = "v1",
        Description = "API для анализа рейтинга активов информационной системы.",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Stock Market Assistant Team",
            Email = "support@stockmarketassistant.com"
        }
    });

    // Добавление XML комментариев для автодокументации (с проверкой существования)
    try
    {
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            c.IncludeXmlComments(xmlPath);
        }
    }
    catch (Exception ex)
    {
        // Логируем ошибку, но не прерываем работу приложения
        Console.WriteLine($"Предупреждение: Не удалось загрузить XML комментарии: {ex.Message}");
    }
});

// Настройка Entity Framework
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AnalyticsDb");
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsAssembly(typeof(AnalyticsDbContext).Assembly.FullName);
        });
    }
    else
    {
        // Fallback для разработки
        options.UseNpgsql("Host=localhost;Database=analytics_db;Username=postgres;Password=password");
    }
});

// Регистрация сервисов
builder.Services.AddScoped<IAssetRatingRepository, AssetRatingRepository>();
builder.Services.AddScoped<IAssetTransactionRepository, AssetTransactionRepository>();
builder.Services.AddScoped<IAssetRatingService, AssetRatingService>();
builder.Services.AddScoped<ITransactionConsumerService, TransactionConsumerService>();

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Настройка логирования
builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

// Настройка middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/swagger/v1/swagger.json", "Analytics Service API"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Миграция базы данных с улучшенной обработкой ошибок
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    try
    {
        // Проверяем, доступна ли база данных
        if (context.Database.CanConnect())
        {
            context.Database.Migrate();
            Console.WriteLine("База данных успешно мигрирована");
        }
        else
        {
            Console.WriteLine("Предупреждение: База данных недоступна для подключения");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при миграции базы данных: {ex.Message}");
        // В продакшене можно добавить более детальное логирование
    }
}

app.Run();


