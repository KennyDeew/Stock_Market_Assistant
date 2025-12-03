using AnalyticsService.TestDataGenerator.Models;
using AnalyticsService.TestDataGenerator.Services;
using Microsoft.EntityFrameworkCore;
using StockMarketAssistant.AnalyticsService.Application.Interfaces.Repositories;
using StockMarketAssistant.AnalyticsService.Application.Services;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;
using StockMarketAssistant.AnalyticsService.Domain.ValueObjects;
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
    private readonly AssetRatingAggregationService _ratingAggregationService;

    public TestDataService(
        AnalyticsDbContext context,
        IAssetTransactionRepository transactionRepository,
        TestDataGenerator generator,
        ILogger<TestDataService> logger,
        AssetRatingAggregationService ratingAggregationService)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _ratingAggregationService = ratingAggregationService ?? throw new ArgumentNullException(nameof(ratingAggregationService));
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
    /// <param name="transactionId">ID конкретной транзакции для расчета рейтинга (опционально)</param>
    /// <param name="daysBack">Количество дней назад для генерации фейковых транзакций (если транзакций нет в БД)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public async Task<TestDataOperationResult> SeedDatabaseAsync(
        Guid? transactionId = null,
        int daysBack = 90,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Начало заполнения базы данных тестовыми данными. TransactionId: {TransactionId}", transactionId);

            List<AssetTransaction> transactionsToProcess = new();
            Period? periodForRating = null;

            // Режим 1: Если указан transactionId - используем его
            if (transactionId.HasValue && transactionId.Value != Guid.Empty)
            {
                _logger.LogInformation("Режим 1: Использование транзакции по ID {TransactionId}", transactionId.Value);
                var transaction = await _transactionRepository.GetByIdAsync(transactionId.Value, cancellationToken);

                if (transaction == null)
                {
                    return new TestDataOperationResult
                    {
                        Success = false,
                        Message = $"Транзакция с ID {transactionId.Value} не найдена в базе данных"
                    };
                }

                transactionsToProcess.Add(transaction);

                // Определяем период для расчета рейтинга (день транзакции)
                var transactionDate = transaction.TransactionTime;
                var dayStart = new DateTime(transactionDate.Year, transactionDate.Month, transactionDate.Day, 0, 0, 0, DateTimeKind.Utc);
                var dayEnd = dayStart.AddDays(1);
                periodForRating = Period.Custom(dayStart, dayEnd);

                _logger.LogInformation("Найдена транзакция {TransactionId} от {TransactionTime}. Период для расчета: {Start} - {End}",
                    transaction.Id, transaction.TransactionTime, periodForRating.Start, periodForRating.End);

                // Проверяем, что транзакция попадает в период
                var transactionsInPeriod = await _transactionRepository.GetByPeriodAsync(periodForRating, cancellationToken);
                _logger.LogInformation("Транзакций в периоде для расчета: {Count}", transactionsInPeriod.Count());
            }
            // Режим 2: Если транзакций нет в БД - создаем фейковые
            else
            {
                // Проверяем наличие транзакций в БД
                var existingTransactions = (await _transactionRepository.GetAllAsync(cancellationToken)).ToList();

                if (existingTransactions.Any())
                {
                    _logger.LogInformation("Режим 2: Использование существующих транзакций из БД. Найдено: {Count}", existingTransactions.Count);
                    transactionsToProcess = existingTransactions;

                    // Определяем период на основе всех транзакций
                    var minDate = transactionsToProcess.Min(t => t.TransactionTime);
                    var maxDate = transactionsToProcess.Max(t => t.TransactionTime);

                    _logger.LogInformation("Диапазон дат транзакций: {MinDate} - {MaxDate}", minDate, maxDate);

                    // Если все транзакции в один день, расширяем период на один день вперед
                    if (minDate.Date == maxDate.Date)
                    {
                        maxDate = minDate.AddDays(1);
                        _logger.LogInformation("Все транзакции в один день, расширяем период до: {MaxDate}", maxDate);
                    }

                    // Добавляем небольшой буфер для включения всех транзакций
                    var periodStart = minDate.AddHours(-1);
                    var periodEnd = maxDate.AddHours(1);
                    periodForRating = Period.Custom(periodStart, periodEnd);
                    _logger.LogInformation("Период для расчета рейтинга: {Start} - {End}", periodForRating.Start, periodForRating.End);

                    // Проверяем, что транзакции попадают в период
                    var transactionsInPeriod = await _transactionRepository.GetByPeriodAsync(periodForRating, cancellationToken);
                    _logger.LogInformation("Транзакций в периоде для расчета: {Count}", transactionsInPeriod.Count());
                }
                else
                {
                    _logger.LogInformation("Режим 3: Создание фейковых транзакций. Транзакций в БД не найдено");

                    // Создание компаний
                    var companies = _generator.CreateCompanies();
                    _logger.LogInformation("Создано компаний: {Count}", companies.Count);

                    // Генерация структуры портфелей с активами
                    var portfolios = _generator.CreatePortfolios(companies, portfolioCount: 4, overlapCount: 3);

                    // Генерация фейковых транзакций
                    foreach (var portfolio in portfolios)
                    {
                        var transactions = _generator.GenerateTransactions(
                            portfolio.PortfolioId,
                            portfolio.AssetIds,
                            companies,
                            daysBack);

                        transactionsToProcess.AddRange(transactions);
                        _logger.LogInformation("Портфель {PortfolioName} (ID: {PortfolioId}): {Count} транзакций",
                            portfolio.Name, portfolio.PortfolioId, transactions.Count);
                    }

                    // Сохранение фейковых транзакций
                    await _transactionRepository.AddRangeAsync(transactionsToProcess, cancellationToken);
                    var savedCount = await _transactionRepository.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Сохранено фейковых транзакций: {Count}", savedCount);

                    // Проверяем, что транзакции действительно сохранены в БД
                    // Используем тот же контекст, что и репозиторий
                    var actualCount = await _context.AssetTransactions.CountAsync(cancellationToken);
                    _logger.LogInformation("Транзакций в БД после сохранения (прямой запрос): {Count}", actualCount);

                    // Проверяем через репозиторий
                    var allTransactions = await _transactionRepository.GetAllAsync(cancellationToken);
                    var allTransactionsList = allTransactions.ToList();
                    _logger.LogInformation("Транзакций в БД после сохранения (через репозиторий): {Count}", allTransactionsList.Count);

                    // Логируем несколько примеров дат транзакций
                    if (allTransactionsList.Any())
                    {
                        var sampleDates = allTransactionsList.Take(5).Select(t => t.TransactionTime).ToList();
                        _logger.LogInformation("Примеры дат транзакций: {Dates}", string.Join(", ", sampleDates));
                    }

                    // Определяем период для расчета рейтинга на основе реальных дат транзакций
                    if (transactionsToProcess.Any())
                    {
                        var minDate = transactionsToProcess.Min(t => t.TransactionTime);
                        var maxDate = transactionsToProcess.Max(t => t.TransactionTime);

                        _logger.LogInformation("Диапазон дат транзакций: {MinDate} - {MaxDate}", minDate, maxDate);

                        // Если все транзакции в один день, расширяем период на один день вперед
                        if (minDate.Date == maxDate.Date)
                        {
                            maxDate = minDate.AddDays(1);
                            _logger.LogInformation("Все транзакции в один день, расширяем период до: {MaxDate}", maxDate);
                        }

                        // Добавляем небольшой буфер для включения всех транзакций
                        var periodStart = minDate.AddHours(-1);
                        var periodEnd = maxDate.AddHours(1);
                        periodForRating = Period.Custom(periodStart, periodEnd);
                        _logger.LogInformation("Период для расчета рейтинга (режим 3): {Start} - {End}", periodForRating.Start, periodForRating.End);

                        // Проверяем, что транзакции попадают в период
                        var transactionsInPeriod = await _transactionRepository.GetByPeriodAsync(periodForRating, cancellationToken);
                        _logger.LogInformation("Транзакций в периоде для расчета: {Count}", transactionsInPeriod.Count());
                    }
                    else
                    {
                        // Fallback: используем daysBack
                        var endDate = DateTime.UtcNow;
                        var startDate = endDate.AddDays(-daysBack);
                        periodForRating = Period.Custom(startDate, endDate);
                        _logger.LogInformation("Период для расчета рейтинга (fallback): {Start} - {End}", periodForRating.Start, periodForRating.End);
                    }
                }
            }

            // Расчет рейтинга для транзакций
            if (transactionsToProcess.Any() && periodForRating != null)
            {
                await CalculateRatingsAsync(periodForRating, transactionsToProcess.Count, cancellationToken);
            }
            else
            {
                _logger.LogWarning("=== Расчет рейтинга НЕ ВЫПОЛНЕН ===");
                if (!transactionsToProcess.Any())
                {
                    _logger.LogWarning("Причина: нет транзакций для обработки (Count: {Count})", transactionsToProcess.Count);
                }
                if (periodForRating == null)
                {
                    _logger.LogWarning("Причина: период не определен");
                }
            }

            // Статистика
            var buyCount = transactionsToProcess.Count(t => t.TransactionType == TransactionType.Buy);
            var sellCount = transactionsToProcess.Count(t => t.TransactionType == TransactionType.Sell);

            return new TestDataOperationResult
            {
                Success = true,
                Message = $"База данных успешно обработана. Обработано транзакций: {transactionsToProcess.Count}",
                CompaniesCreated = 0, // Не создаем компании, используем существующие или фейковые
                PortfoliosCreated = 0, // Портфели не создаются
                TransactionsCreated = transactionsToProcess.Count,
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

    /// <summary>
    /// Рассчитать рейтинги активов за указанный период
    /// </summary>
    /// <param name="period">Период для расчета рейтингов</param>
    /// <param name="expectedTransactionsCount">Ожидаемое количество транзакций (для логирования)</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task CalculateRatingsAsync(
        Period period,
        int expectedTransactionsCount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Проверяем количество рейтингов до расчета
            var ratingsCountBefore = await _context.AssetRatings.CountAsync(cancellationToken);

            _logger.LogInformation("=== Начало расчета рейтинга ===");
            _logger.LogInformation("Ожидаемое количество транзакций для обработки: {Count}", expectedTransactionsCount);
            _logger.LogInformation("Период расчета: {Start} - {End}", period.Start, period.End);
            _logger.LogInformation("Рейтингов в БД до расчета: {Count}", ratingsCountBefore);

            // ДОПОЛНИТЕЛЬНАЯ ПРОВЕРКА: убеждаемся, что транзакции видны в БД
            var transactionsInDb = await _transactionRepository.GetByPeriodAsync(period, cancellationToken);
            var transactionsInDbList = transactionsInDb.ToList();
            _logger.LogInformation("ВАЖНО: Транзакций в БД в указанном периоде: {Count}", transactionsInDbList.Count);

            if (!transactionsInDbList.Any())
            {
                _logger.LogWarning("ПРОБЛЕМА: В БД нет транзакций в указанном периоде!");
                _logger.LogWarning("Период: {Start} - {End}", period.Start, period.End);

                // Проверяем все транзакции в БД
                var allInDb = await _transactionRepository.GetAllAsync(cancellationToken);
                var allInDbList = allInDb.ToList();
                _logger.LogWarning("Всего транзакций в БД: {Count}", allInDbList.Count);

                if (allInDbList.Any())
                {
                    var minDbDate = allInDbList.Min(t => t.TransactionTime);
                    var maxDbDate = allInDbList.Max(t => t.TransactionTime);
                    _logger.LogWarning("Диапазон дат всех транзакций в БД: {MinDate} - {MaxDate}", minDbDate, maxDbDate);
                    _logger.LogWarning("Период для расчета: {Start} - {End}", period.Start, period.End);

                    // Пробуем расширить период
                    var extendedStart = minDbDate.AddHours(-1);
                    var extendedEnd = maxDbDate.AddHours(1);
                    period = Period.Custom(extendedStart, extendedEnd);
                    _logger.LogWarning("Расширенный период: {Start} - {End}", period.Start, period.End);

                    // Проверяем снова
                    var transactionsInExtendedPeriod = await _transactionRepository.GetByPeriodAsync(period, cancellationToken);
                    _logger.LogWarning("Транзакций в расширенном периоде: {Count}", transactionsInExtendedPeriod.Count());
                }
                else
                {
                    _logger.LogError("В БД нет транзакций вообще! Расчет рейтинга невозможен.");
                    return;
                }
            }

            // Запускаем агрегацию рейтингов
            _logger.LogInformation("Запуск AggregateRatingsAsync с периодом: {Start} - {End}", period.Start, period.End);
            await _ratingAggregationService.AggregateRatingsAsync(period, cancellationToken);

            // Проверяем, что рейтинги были созданы
            var ratingsCountAfter = await _context.AssetRatings.CountAsync(cancellationToken);
            var ratingsCreated = ratingsCountAfter - ratingsCountBefore;
            _logger.LogInformation("=== Расчет рейтинга завершен успешно ===");
            _logger.LogInformation("Рейтингов в БД после расчета: {Count}", ratingsCountAfter);
            _logger.LogInformation("Создано новых рейтингов: {Count}", ratingsCreated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== ОШИБКА при расчете рейтинга ===");
            _logger.LogError("Тип ошибки: {ExceptionType}", ex.GetType().Name);
            _logger.LogError("Сообщение: {Message}", ex.Message);
            _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);
            if (ex.InnerException != null)
            {
                _logger.LogError("Внутренняя ошибка: {InnerException}", ex.InnerException.Message);
            }
            // Не прерываем выполнение, но логируем детали
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

