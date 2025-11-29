using AnalyticsService.TestDataGenerator.Models;
using StockMarketAssistant.AnalyticsService.Domain.Entities;
using StockMarketAssistant.AnalyticsService.Domain.Enums;

namespace AnalyticsService.TestDataGenerator.Services;

/// <summary>
/// Генератор тестовых данных для аналитики
/// </summary>
public class TestDataGenerator
{
    private readonly Random _random = new();

    /// <summary>
    /// Создать список компаний с активами
    /// </summary>
    public List<CompanyAsset> CreateCompanies()
    {
        return new List<CompanyAsset>
        {
            new CompanyAsset
            {
                StockCardId = Guid.NewGuid(),
                Ticker = "SBER",
                Name = "ПАО Сбербанк",
                BasePrice = 350.0m,
                MinPrice = 300.0m,
                MaxPrice = 400.0m
            },
            new CompanyAsset
            {
                StockCardId = Guid.NewGuid(),
                Ticker = "ALFA",
                Name = "ПАО Альфа-Банк",
                BasePrice = 45.0m,
                MinPrice = 40.0m,
                MaxPrice = 50.0m
            },
            new CompanyAsset
            {
                StockCardId = Guid.NewGuid(),
                Ticker = "RSHB",
                Name = "ПАО Россельхозбанк",
                BasePrice = 25.0m,
                MinPrice = 20.0m,
                MaxPrice = 30.0m
            },
            new CompanyAsset
            {
                StockCardId = Guid.NewGuid(),
                Ticker = "TCSG",
                Name = "ПАО Тинькофф Банк",
                BasePrice = 3800.0m,
                MinPrice = 3500.0m,
                MaxPrice = 4100.0m
            },
            new CompanyAsset
            {
                StockCardId = Guid.NewGuid(),
                Ticker = "GAZP",
                Name = "ПАО Газпром",
                BasePrice = 180.0m,
                MinPrice = 160.0m,
                MaxPrice = 200.0m
            },
            new CompanyAsset
            {
                StockCardId = Guid.NewGuid(),
                Ticker = "ROSN",
                Name = "ПАО Роснефть",
                BasePrice = 550.0m,
                MinPrice = 500.0m,
                MaxPrice = 600.0m
            },
            new CompanyAsset
            {
                StockCardId = Guid.NewGuid(),
                Ticker = "TATN",
                Name = "ПАО Татнефть",
                BasePrice = 420.0m,
                MinPrice = 380.0m,
                MaxPrice = 460.0m
            }
        };
    }

    /// <summary>
    /// Создать портфели с активами
    /// </summary>
    /// <param name="companies">Список компаний</param>
    /// <param name="portfolioCount">Количество портфелей (3-5)</param>
    /// <param name="overlapCount">Количество портфелей с пересекающимися активами (2-4)</param>
    public List<PortfolioData> CreatePortfolios(List<CompanyAsset> companies, int portfolioCount = 4, int overlapCount = 3)
    {
        var portfolios = new List<PortfolioData>();
        var allAssetIds = companies.Select(c => c.StockCardId).ToList();

        // Создаем портфели с пересекающимися активами
        var sharedAssets = companies.Take(3).Select(c => c.StockCardId).ToList(); // Первые 3 компании будут общими

        for (int i = 0; i < overlapCount; i++)
        {
            var portfolio = new PortfolioData
            {
                PortfolioId = Guid.NewGuid(),
                Name = $"Портфель {i + 1}"
            };

            // Добавляем общие активы
            portfolio.AssetIds.AddRange(sharedAssets);

            // Добавляем случайные уникальные активы (3-7 активов всего)
            var uniqueAssets = companies
                .Where(c => !sharedAssets.Contains(c.StockCardId))
                .OrderBy(_ => _random.Next())
                .Take(_random.Next(0, 4)) // 0-3 дополнительных актива
                .Select(c => c.StockCardId)
                .ToList();

            portfolio.AssetIds.AddRange(uniqueAssets);

            // Перемешиваем для разнообразия
            portfolio.AssetIds = portfolio.AssetIds.OrderBy(_ => _random.Next()).ToList();

            portfolios.Add(portfolio);
        }

        // Создаем портфели без пересечений (если нужно)
        for (int i = overlapCount; i < portfolioCount; i++)
        {
            var portfolio = new PortfolioData
            {
                PortfolioId = Guid.NewGuid(),
                Name = $"Портфель {i + 1}"
            };

            // Выбираем случайные активы (3-7 штук)
            var assetCount = _random.Next(3, 8);
            portfolio.AssetIds = companies
                .OrderBy(_ => _random.Next())
                .Take(assetCount)
                .Select(c => c.StockCardId)
                .ToList();

            portfolios.Add(portfolio);
        }

        return portfolios;
    }

    /// <summary>
    /// Генерировать транзакции для портфеля
    /// </summary>
    /// <param name="portfolioId">Идентификатор портфеля</param>
    /// <param name="assetIds">Список идентификаторов активов</param>
    /// <param name="companies">Список компаний</param>
    /// <param name="daysBack">Количество дней назад для генерации транзакций</param>
    public List<AssetTransaction> GenerateTransactions(
        Guid portfolioId,
        List<Guid> assetIds,
        List<CompanyAsset> companies,
        int daysBack = 90)
    {
        var transactions = new List<AssetTransaction>();
        var endDate = DateTime.UtcNow;
        var startDate = endDate.AddDays(-daysBack);

        foreach (var assetId in assetIds)
        {
            var company = companies.FirstOrDefault(c => c.StockCardId == assetId);
            if (company == null) continue;

            // Генерируем 5-15 транзакций на актив
            var transactionCount = _random.Next(5, 16);

            for (int i = 0; i < transactionCount; i++)
            {
                // Случайная дата в диапазоне
                var transactionDate = startDate.AddDays(_random.Next(0, daysBack))
                    .AddHours(_random.Next(0, 24))
                    .AddMinutes(_random.Next(0, 60));

                // Случайная цена в диапазоне компании
                var price = company.MinPrice + (decimal)(_random.NextDouble() * (double)(company.MaxPrice - company.MinPrice));
                price = Math.Round(price, 2);

                // Количество акций (10-1000)
                var quantity = _random.Next(10, 1001);

                // Тип транзакции (70% покупок, 30% продаж)
                var transactionType = _random.Next(0, 100) < 70
                    ? TransactionType.Buy
                    : TransactionType.Sell;

                var transaction = transactionType == TransactionType.Buy
                    ? AssetTransaction.CreateBuyTransaction(
                        portfolioId,
                        assetId,
                        AssetType.Share,
                        quantity,
                        price,
                        transactionDate)
                    : AssetTransaction.CreateSellTransaction(
                        portfolioId,
                        assetId,
                        AssetType.Share,
                        quantity,
                        price,
                        transactionDate);

                transactions.Add(transaction);
            }
        }

        // Сортируем по дате
        return transactions.OrderBy(t => t.TransactionTime).ToList();
    }

    /// <summary>
    /// Генерировать случайную цену для компании
    /// </summary>
    public decimal GenerateRandomPrice(CompanyAsset company)
    {
        var price = company.MinPrice + (decimal)(_random.NextDouble() * (double)(company.MaxPrice - company.MinPrice));
        return Math.Round(price, 2);
    }
}


