using AuthService.Application;
using AuthService.Infrastructure.Postgres;
using AuthService.Infrastructure.Postgres.Seeding;
using AuthService.WebApi.Middlewares;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.WebApi;

public abstract class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Добавляем сервисы Aspire
        builder.AddServiceDefaults();

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

        // Отключаем авто-валидацию ModelState (будем формировать ошибку сами)
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        // Инфраструктура БД + политика ретраев миграций (всё через Options)
        builder.Services
            .AddPostgresInfrastructure()
            .AddMigrationResilience();

        // Модуль аккаунтов (Infrastructure + Presentation)
        builder.Services.AddAccountsModule();

        // Аутентификация/авторизация (JwtBearer на Options, hot-reload)
        builder.Services.AddAuthServices();

        // Приложение (Use-cases, валидация и т.д.)
        builder.Services.AddApplication();

        // Базовые веб-зависимости (Controllers, OpenAPI)
        builder.Services.AddProgramDependencies();

        var app = builder.Build();
        
        // Глобальный обработчик исключений
        app.UseExceptionMiddleware();

        // Миграции -> сидинг
        await app.MigrateDatabaseAsync<PostgresDbContext>(CancellationToken.None);

        var accountsSeeder = app.Services.GetRequiredService<AccountsSeeder>();
        await accountsSeeder.SeedAsync();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/openapi/v1.json", "AuthService API");
            });
        }
        app.UseCors("AllowFrontendApp");

        app.MapDefaultEndpoints();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}