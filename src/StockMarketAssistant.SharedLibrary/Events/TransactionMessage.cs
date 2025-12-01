using System.Text.Json.Serialization;

namespace StockMarketAssistant.SharedLibrary.Events
{
    /// <summary>
    /// DTO для сообщения о транзакции из PortfolioService
    /// </summary>
    public class TransactionMessage
    {
        /// <summary>
        /// Идентификатор транзакции
        /// </summary>
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор портфеля
        /// </summary>
        [JsonPropertyName("portfolioId")]
        public Guid PortfolioId { get; set; }

        /// <summary>
        /// Идентификатор актива (StockCardId)
        /// </summary>
        [JsonPropertyName("stockCardId")]
        public Guid StockCardId { get; set; }

        /// <summary>
        /// Тип актива (1 = Share, 2 = Bond, 3 = Crypto)
        /// </summary>
        [JsonPropertyName("assetType")]
        public int AssetType { get; set; }

        /// <summary>
        /// Тип транзакции (1 = Buy, 2 = Sell)
        /// </summary>
        [JsonPropertyName("transactionType")]
        public int TransactionType { get; set; }

        /// <summary>
        /// Количество активов
        /// </summary>
        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Цена за единицу актива
        /// </summary>
        [JsonPropertyName("pricePerUnit")]
        public decimal PricePerUnit { get; set; }

        /// <summary>
        /// Общая стоимость транзакции
        /// </summary>
        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Дата и время транзакции
        /// </summary>
        [JsonPropertyName("transactionTime")]
        public DateTime TransactionTime { get; set; }

        /// <summary>
        /// Валюта транзакции
        /// </summary>
        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "RUB";

        /// <summary>
        /// Дополнительные метаданные (опционально)
        /// </summary>
        [JsonPropertyName("metadata")]
        public string? Metadata { get; set; }
    }
}
