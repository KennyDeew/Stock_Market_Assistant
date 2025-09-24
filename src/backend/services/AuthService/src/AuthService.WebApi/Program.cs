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

        // Отключена автоматическая проверка ModelState для ручной обработки ошибок
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        builder.Services.AddProgramDependencies();

        builder.Services
            .AddPostgresInfrastructure(builder.Configuration)
            .AddMigrationResilience();

        builder.Services.AddAccountsModule(builder.Configuration);

        builder.Services.AddAuthServices(builder.Configuration);

        builder.Services.AddApplication();

        var app = builder.Build();

        // Глобальный обработчик ошибок
        app.UseExceptionMiddleware();

        // Порядок: миграции -> сиды
        await app.MigrateDatabaseAsync<AccountsWriteDbContext>(CancellationToken.None);

        //var accountsSeeder = app.Services.GetRequiredService<AccountsSeeder>();
        //await accountsSeeder.SeedAsync();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "AuthService API"));
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}