using AnalyticsService.TestDataGenerator.Models;
using AnalyticsService.TestDataGenerator.Services;
using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Persistence;
using TestDataGenerator = AnalyticsService.TestDataGenerator.Services.TestDataGenerator;

namespace StockMarketAssistant.AnalyticsService.WebApi.Services;

/// <summary>
/// Сервис для работы с тестовыми данными аналитики
/// </summary>
public class TestDataService
{
    private readonly AnalyticsDbContext _context;
    private readonly IAssetTransactionRepository _transactionRepository;
    private readonly TestDataGenerator _generator;
    private readonly ILogger<TestDataService> _logger;

    public TestDataService(
        AnalyticsDbContext context,
        IAssetTransactionRepository transactionRepository,
        TestDataGenerator generator,
        ILogger<TestDataService> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Очистить базу данных аналитики
    /// </summary>
    public async Task<TestDataOperationResult> ClearDatabaseAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Начало очистки базы данных аналитики");

            // Очищаем таблицу транзакций
            var transactionsCount = await _context.AssetTransactions.CountAsync(cancellationToken);
            _context.AssetTransactions.RemoveRange(_context.AssetTransactions);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Удалено транзакций: {Count}", transactionsCount);

            // Очищаем таблицу рейтингов
            var ratingsCount = await _context.AssetRatings.CountAsync(cancellationToken);
            _context.AssetRatings.RemoveRange(_context.AssetRatings);
            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Удалено рейтингов: {Count}", ratingsCount);

            _logger.LogInformation("Очистка базы данных завершена успешно");

            return new TestDataOperationResult
            {
                Success = true,
                Message = $"База данных успешно очищена. Удалено транзакций: {transactionsCount}, рейтингов: {ratingsCount}",
                TransactionsDeleted = transactionsCount,
                RatingsDeleted = ratingsCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при очистке базы данных");
            return new TestDataOperationResult
            {
                Success = false,
                Message = $"Ошибка при очистке базы данных: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Заполнить базу данных тестовыми данными
    /// </summary>
    public async Task<TestDataOperationResult> SeedDatabaseAsync(
        int portfolioCount = 4,
        int overlapCount = 3,
        int daysBack = 90,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Начало заполнения базы данных тестовыми данными");

            // Создание компаний
            var companies = _generator.CreateCompanies();
            _logger.LogInformation("Создано компаний: {Count}", companies.Count);

            // Создание портфелей
            var portfolios = _generator.CreatePortfolios(companies, portfolioCount, overlapCount);
            _logger.LogInformation("Создано портфелей: {Count}", portfolios.Count);

            // Генерация транзакций
            var allTransactions = new List<AssetTransaction>();
            foreach (var portfolio in portfolios)
            {
                var transactions = _generator.GenerateTransactions(
                    portfolio.PortfolioId,
                    portfolio.AssetIds,
                    companies,
                    daysBack);

                allTransactions.AddRange(transactions);
                _logger.LogInformation("Портфель {PortfolioName}: {Count} транзакций", portfolio.Name, transactions.Count);
            }

            // Сохранение транзакций
            await _transactionRepository.AddRangeAsync(allTransactions, cancellationToken);
            var savedCount = await _transactionRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Заполнение базы данных завершено успешно. Сохранено транзакций: {Count}", savedCount);

            // Статистика
            var buyCount = allTransactions.Count(t => t.TransactionType == TransactionType.Buy);
            var sellCount = allTransactions.Count(t => t.TransactionType == TransactionType.Sell);

            return new TestDataOperationResult
            {
                Success = true,
                Message = $"База данных успешно заполнена тестовыми данными",
                CompaniesCreated = companies.Count,
                PortfoliosCreated = portfolios.Count,
                TransactionsCreated = savedCount,
                BuyTransactionsCount = buyCount,
                SellTransactionsCount = sellCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при заполнении базы данных тестовыми данными");
            return new TestDataOperationResult
            {
                Success = false,
                Message = $"Ошибка при заполнении базы данных: {ex.Message}"
            };
        }
    }
}

/// <summary>
/// Результат операции с тестовыми данными
/// </summary>
public class TestDataOperationResult
{
    /// <summary>
    /// Успешность операции
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Сообщение о результате
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Количество созданных компаний
    /// </summary>
    public int CompaniesCreated { get; set; }

    /// <summary>
    /// Количество созданных портфелей
    /// </summary>
    public int PortfoliosCreated { get; set; }

    /// <summary>
    /// Количество созданных транзакций
    /// </summary>
    public int TransactionsCreated { get; set; }

    /// <summary>
    /// Количество транзакций покупки
    /// </summary>
    public int BuyTransactionsCount { get; set; }

    /// <summary>
    /// Количество транзакций продажи
    /// </summary>
    public int SellTransactionsCount { get; set; }

    /// <summary>
    /// Количество удаленных транзакций
    /// </summary>
    public int TransactionsDeleted { get; set; }

    /// <summary>
    /// Количество удаленных рейтингов
    /// </summary>
    public int RatingsDeleted { get; set; }
}

