using System.Text.Json.Serialization;

namespace StockMarketAssistant.AnalyticsService.Application.DTOs
{
    /// <summary>
    /// DTO для ответа транзакции актива портфеля из PortfolioService
    /// </summary>
    public class PortfolioAssetTransactionResponseDto
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("portfolioAssetId")]
        public Guid PortfolioAssetId { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("pricePerUnit")]
        public decimal PricePerUnit { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "RUB";

        [JsonPropertyName("transactionDate")]
        public DateTime TransactionDate { get; set; }

        [JsonPropertyName("transactionType")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PortfolioAssetTransactionType TransactionType { get; set; }
    }

    /// <summary>
    /// Тип транзакции актива портфеля
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum PortfolioAssetTransactionType
    {
        Buy = 1,
        Sell = 2
    }
}

