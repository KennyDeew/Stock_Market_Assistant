using Autofac;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;

namespace StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework;

/// <summary>
/// Установщик Entity Framework Core для различных DI-контейнеров.
/// Поддерживает как стандартный IServiceCollection, так и Autofac ContainerBuilder.
/// </summary>
public static class EntityFrameworkInstaller
{
    /// <summary>
    /// Регистрирует DbContext в стандартном IServiceCollection (для совместимости).
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="connectionString">Строка подключения к PostgreSQL</param>
    /// <returns>Обновлённая коллекция сервисов</returns>
    public static IServiceCollection ConfigureContext(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseNpgsql(connectionString);
            options.UseSnakeCaseNamingConvention();
        });

        return services;
    }

    /// <summary>
    /// Регистрирует DbContext в Autofac ContainerBuilder.
    /// </summary>
    /// <param name="builder">Строитель контейнера Autofac</param>
    /// <param name="connectionString">Строка подключения к PostgreSQL</param>
    public static void ConfigureContext(
        this ContainerBuilder builder,
        string connectionString)
    {
        builder.Register(c =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.UseSnakeCaseNamingConvention();

            return new DatabaseContext(optionsBuilder.Options);
        }).InstancePerLifetimeScope();
    }

    /// <summary>
    /// Регистрирует InMemory DbContext в Autofac ContainerBuilder для тестов.
    /// </summary>
    /// <param name="builder">Строитель контейнера Autofac</param>
    public static void ConfigureInMemoryContext(this ContainerBuilder builder, InMemoryDatabaseRoot databaseRoot)
    {
        builder.Register(c =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            // Используем общий InMemoryDatabaseRoot для всех тестов
            optionsBuilder.UseInMemoryDatabase("IntegrationTestDb", databaseRoot);
            return new DatabaseContext(optionsBuilder.Options);
        }).InstancePerLifetimeScope();
    }
}