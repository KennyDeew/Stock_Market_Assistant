using StockMarketAssistant.AnalyticsService.Application.DTOs;

namespace StockMarketAssistant.AnalyticsService.Application.DTOs.Responses
{
    /// <summary>
    /// DTO ответа для истории транзакций портфеля
    /// </summary>
    public class PortfolioHistoryResponseDto
    {
        /// <summary>
        /// Идентификатор портфеля
        /// </summary>
        public Guid PortfolioId { get; set; }

        /// <summary>
        /// Начальная дата периода
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Конечная дата периода
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Список транзакций за период
        /// </summary>
        public List<PortfolioTransactionResponseDto> Transactions { get; set; } = new();
    }

    /// <summary>
    /// DTO ответа для транзакции портфеля
    /// </summary>
    public class PortfolioTransactionResponseDto
    {
        /// <summary>
        /// Идентификатор транзакции
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор актива портфеля
        /// </summary>
        public Guid PortfolioAssetId { get; set; }

        /// <summary>
        /// Идентификатор актива (StockCardId)
        /// </summary>
        public Guid StockCardId { get; set; }

        /// <summary>
        /// Тип транзакции (1 = Buy, 2 = Sell)
        /// </summary>
        public int TransactionType { get; set; }

        /// <summary>
        /// Количество
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// Цена за единицу
        /// </summary>
        public decimal PricePerUnit { get; set; }

        /// <summary>
        /// Общая стоимость транзакции
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Дата и время транзакции
        /// </summary>
        public DateTime TransactionDate { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public string Currency { get; set; } = "RUB";
    }

    /// <summary>
    /// Расширения для маппинга PortfolioHistoryDto
    /// </summary>
    public static class PortfolioHistoryResponseDtoExtensions
    {
        /// <summary>
        /// Маппинг из PortfolioHistoryDto
        /// </summary>
        public static PortfolioHistoryResponseDto FromPortfolioHistoryDto(PortfolioHistoryDto history)
        {
            return new PortfolioHistoryResponseDto
            {
                PortfolioId = history.PortfolioId,
                StartDate = history.StartDate,
                EndDate = history.EndDate,
                Transactions = history.Transactions.Select(t => new PortfolioTransactionResponseDto
                {
                    Id = t.Id,
                    PortfolioAssetId = t.PortfolioAssetId,
                    StockCardId = t.StockCardId,
                    TransactionType = t.TransactionType,
                    Quantity = t.Quantity,
                    PricePerUnit = t.PricePerUnit,
                    TotalAmount = t.TotalAmount,
                    TransactionDate = t.TransactionDate,
                    Currency = t.Currency
                }).ToList()
            };
        }
    }
}

