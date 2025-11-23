using StockMarketAssistant.AnalyticsService.Domain.Enums;

namespace StockMarketAssistant.AnalyticsService.Application.DTOs.Responses
{
    /// <summary>
    /// DTO ответа для рейтинга актива
    /// </summary>
    public class AssetRatingDto
    {
        /// <summary>
        /// Идентификатор рейтинга
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор актива
        /// </summary>
        public Guid StockCardId { get; set; }

        /// <summary>
        /// Тип актива
        /// </summary>
        public int AssetType { get; set; }

        /// <summary>
        /// Тикер актива
        /// </summary>
        public string Ticker { get; set; } = string.Empty;

        /// <summary>
        /// Название актива
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Период анализа (начало)
        /// </summary>
        public DateTime PeriodStart { get; set; }

        /// <summary>
        /// Период анализа (конец)
        /// </summary>
        public DateTime PeriodEnd { get; set; }

        /// <summary>
        /// Количество транзакций покупки
        /// </summary>
        public int BuyTransactionCount { get; set; }

        /// <summary>
        /// Количество транзакций продажи
        /// </summary>
        public int SellTransactionCount { get; set; }

        /// <summary>
        /// Общая сумма покупок
        /// </summary>
        public decimal TotalBuyAmount { get; set; }

        /// <summary>
        /// Общая сумма продаж
        /// </summary>
        public decimal TotalSellAmount { get; set; }

        /// <summary>
        /// Общее количество покупок
        /// </summary>
        public int TotalBuyQuantity { get; set; }

        /// <summary>
        /// Общее количество продаж
        /// </summary>
        public int TotalSellQuantity { get; set; }

        /// <summary>
        /// Ранг по количеству транзакций
        /// </summary>
        public int TransactionCountRank { get; set; }

        /// <summary>
        /// Ранг по сумме транзакций
        /// </summary>
        public int TransactionAmountRank { get; set; }

        /// <summary>
        /// Контекст анализа
        /// </summary>
        public int Context { get; set; }

        /// <summary>
        /// Идентификатор портфеля (для Portfolio контекста)
        /// </summary>
        public Guid? PortfolioId { get; set; }

        /// <summary>
        /// Время последнего обновления
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// Маппинг из доменной сущности
        /// </summary>
        public static AssetRatingDto FromEntity(Domain.Entities.AssetRating rating)
        {
            return new AssetRatingDto
            {
                Id = rating.Id,
                StockCardId = rating.StockCardId,
                AssetType = (int)rating.AssetType,
                Ticker = rating.Ticker,
                Name = rating.Name,
                PeriodStart = rating.PeriodStart,
                PeriodEnd = rating.PeriodEnd,
                BuyTransactionCount = rating.BuyTransactionCount,
                SellTransactionCount = rating.SellTransactionCount,
                TotalBuyAmount = rating.TotalBuyAmount,
                TotalSellAmount = rating.TotalSellAmount,
                TotalBuyQuantity = rating.TotalBuyQuantity,
                TotalSellQuantity = rating.TotalSellQuantity,
                TransactionCountRank = rating.TransactionCountRank,
                TransactionAmountRank = rating.TransactionAmountRank,
                Context = (int)rating.Context,
                PortfolioId = rating.PortfolioId,
                LastUpdated = rating.LastUpdated
            };
        }
    }
}

