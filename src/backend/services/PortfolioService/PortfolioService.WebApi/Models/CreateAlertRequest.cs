using StockMarketAssistant.PortfolioService.Domain.Enums;
using System.Text.Json.Serialization;

namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Запрос на создание уведомления о цене актива
    /// </summary>
    public record CreateAlertRequest
    {
        /// <summary>
        /// Идентификатор карточки актива
        /// </summary>
        public Guid StockCardId { get; init; }

        /// <summary>
        /// Тип актива
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PortfolioAssetType AssetType { get; init; }

        /// <summary>
        /// Тикер
        /// </summary>
        public string? AssetTicker { get; init; }

        /// <summary>
        /// Название
        /// </summary>
        public string? AssetName { get; init; }

        /// <summary>
        /// Целевая цена, при достижении которой сработает уведомление
        /// </summary>
        public decimal TargetPrice { get; init; }

        /// <summary>
        /// Валюта актива(RUB, USD и т.д.)
        /// </summary>
        public string? AssetCurrency { get; init; }

        /// <summary>
        /// Условие срабатывания: выше или ниже
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AlertCondition Condition { get; init; }
    }
}
