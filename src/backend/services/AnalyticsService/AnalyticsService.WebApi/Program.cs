using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
//using Microsoft.OpenApi.Models;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Context;
using StockMarketAssistant.AnalyticsService.Infrastructure.Repositories;
using StockMarketAssistant.AnalyticsService.Application.Services;

var builder = WebApplication.CreateBuilder(args);

// Добавление сервисов
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();



/*// Настройка Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Analytics Service API",
        Version = "v1",
        Description = "API для анализа рейтинга активов на фондовом рынке",
        Contact = new OpenApiContact
        {
            Name = "Stock Market Assistant Team",
            Email = "support@stockmarketassistant.com"
        }
    });

    // Добавление XML комментариев для автодокументации
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Настройка схемы для enum
    c.SchemaFilter<EnumSchemaFilter>();
});*/

// Настройка Entity Framework
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AnalyticsDb");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.MigrationsAssembly(typeof(AnalyticsDbContext).Assembly.FullName);
    });
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
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "AuthService API"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Миграция базы данных
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    try
    {
        context.Database.Migrate();
        Console.WriteLine("База данных успешно мигрирована");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при миграции базы данных: {ex.Message}");
    }
}

app.Run();

// Фильтр схемы для enum
public class EnumSchemaFilter : Microsoft.OpenApi.Any.OpenApiSchemaFilter
{
    public void Apply(Microsoft.OpenApi.Any.OpenApiSchema schema, Microsoft.OpenApi.Any.OpenApiSchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            foreach (var enumName in Enum.GetNames(context.Type))
            {
                schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(enumName));
            }
        }
    }
}


