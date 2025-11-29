using AnalyticsService.TestDataGenerator.Models;
using AnalyticsService.TestDataGenerator.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence.Repositories;

namespace AnalyticsService.TestDataGenerator;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Генератор тестовых данных для AnalyticsService ===\n");

        // Настройка конфигурации
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("analytics-db")
            ?? Environment.GetEnvironmentVariable("ANALYTICS_DB_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=analytics_db;Username=postgres;Password=postgres";

        Console.WriteLine($"Подключение к БД: {connectionString.Split(';').First()}...\n");

        // Настройка DbContext
        var optionsBuilder = new DbContextOptionsBuilder<AnalyticsDbContext>();
        optionsBuilder.UseNpgsql(connectionString);
        optionsBuilder.UseSnakeCaseNamingConvention();

        using var context = new AnalyticsDbContext(optionsBuilder.Options);

        // Проверка подключения
        try
        {
            await context.Database.CanConnectAsync();
            Console.WriteLine("✓ Подключение к базе данных успешно\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Ошибка подключения к базе данных: {ex.Message}");
            return;
        }

        // Создание репозитория
        var transactionRepository = new AssetTransactionRepository(context);

        // Генератор данных
        var generator = new Services.TestDataGenerator();

        // Создание компаний
        Console.WriteLine("Генерация компаний...");
        var companies = generator.CreateCompanies();
        Console.WriteLine($"✓ Создано компаний: {companies.Count}");
        foreach (var company in companies)
        {
            Console.WriteLine($"  - {company.Ticker}: {company.Name} (ID: {company.StockCardId})");
        }
        Console.WriteLine();

        // Создание портфелей
        Console.WriteLine("Генерация портфелей...");
        var portfolios = generator.CreatePortfolios(companies, portfolioCount: 4, overlapCount: 3);
        Console.WriteLine($"✓ Создано портфелей: {portfolios.Count}");
        foreach (var portfolio in portfolios)
        {
            Console.WriteLine($"  - {portfolio.Name} (ID: {portfolio.PortfolioId}): {portfolio.AssetIds.Count} активов");
        }
        Console.WriteLine();

        // Генерация транзакций
        Console.WriteLine("Генерация транзакций...");
        var allTransactions = new List<AssetTransaction>();

        foreach (var portfolio in portfolios)
        {
            var transactions = generator.GenerateTransactions(
                portfolio.PortfolioId,
                portfolio.AssetIds,
                companies,
                daysBack: 90);

            allTransactions.AddRange(transactions);
            Console.WriteLine($"  - {portfolio.Name}: {transactions.Count} транзакций");
        }

        Console.WriteLine($"✓ Всего транзакций: {allTransactions.Count}\n");

        // Сохранение в БД
        Console.WriteLine("Сохранение данных в базу данных...");
        try
        {
            await transactionRepository.AddRangeAsync(allTransactions);
            var savedCount = await transactionRepository.SaveChangesAsync();
            Console.WriteLine($"✓ Сохранено транзакций: {savedCount}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Ошибка при сохранении: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"  Внутренняя ошибка: {ex.InnerException.Message}");
            }
            return;
        }

        // Статистика
        Console.WriteLine("=== Статистика ===");
        Console.WriteLine($"Компаний: {companies.Count}");
        Console.WriteLine($"Портфелей: {portfolios.Count}");
        Console.WriteLine($"Транзакций: {allTransactions.Count}");
        Console.WriteLine($"Покупок: {allTransactions.Count(t => t.TransactionType == TransactionType.Buy)}");
        Console.WriteLine($"Продаж: {allTransactions.Count(t => t.TransactionType == TransactionType.Sell)}");

        // Статистика по портфелям
        Console.WriteLine("\n=== Статистика по портфелям ===");
        foreach (var portfolio in portfolios)
        {
            var portfolioTransactions = allTransactions.Where(t => t.PortfolioId == portfolio.PortfolioId).ToList();
            var buyCount = portfolioTransactions.Count(t => t.TransactionType == TransactionType.Buy);
            var sellCount = portfolioTransactions.Count(t => t.TransactionType == TransactionType.Sell);
            var totalBuyAmount = portfolioTransactions
                .Where(t => t.TransactionType == TransactionType.Buy)
                .Sum(t => t.TotalAmount);
            var totalSellAmount = portfolioTransactions
                .Where(t => t.TransactionType == TransactionType.Sell)
                .Sum(t => t.TotalAmount);

            Console.WriteLine($"{portfolio.Name}:");
            Console.WriteLine($"  Активов: {portfolio.AssetIds.Count}");
            Console.WriteLine($"  Транзакций: {portfolioTransactions.Count} (Покупок: {buyCount}, Продаж: {sellCount})");
            Console.WriteLine($"  Объем покупок: {totalBuyAmount:N2} RUB");
            Console.WriteLine($"  Объем продаж: {totalSellAmount:N2} RUB");
        }

        // Статистика по активам (пересечения)
        Console.WriteLine("\n=== Пересекающиеся активы ===");
        var sharedAssets = portfolios
            .SelectMany(p => p.AssetIds)
            .GroupBy(id => id)
            .Where(g => g.Count() > 1)
            .Select(g => new { AssetId = g.Key, PortfolioCount = g.Count() })
            .ToList();

        foreach (var shared in sharedAssets)
        {
            var company = companies.FirstOrDefault(c => c.StockCardId == shared.AssetId);
            if (company != null)
            {
                Console.WriteLine($"  {company.Ticker} ({company.Name}): в {shared.PortfolioCount} портфелях");
            }
        }

        Console.WriteLine("\n✓ Генерация данных завершена успешно!");
    }
}


