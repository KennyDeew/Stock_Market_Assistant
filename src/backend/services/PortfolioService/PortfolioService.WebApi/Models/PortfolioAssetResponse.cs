using StockMarketAssistant.PortfolioService.Domain.Enums;
using System.Text.Json.Serialization;

namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель контроллера для возвращаемого значения по финансовому активу из портфеля ценных бумаг
    /// </summary>
    /// <param name="Id">Уникальный идентификатор актива</param>
    /// <param name="PortfolioId">Идентификатор портфеля</param>
    /// <param name="StockCardId">Идентификатор карточки ценной бумаги</param>
    /// <param name="Ticker">Тикер ценной бумаги</param>
    /// <param name="Name">Наименование актива</param>
    /// <param name="Description">Описание актива</param>
    /// <param name="TotalQuantity">Общее количество актива, шт.</param>
    /// <param name="AveragePurchasePrice">Средняя цена покупки</param>
    /// <param name="LastUpdated">Дата последнего обновления информации об активе</param>
    /// <param name="Currency">Валюта актива (RUB, USD и т.д.)</param>
    public record PortfolioAssetResponse(
        Guid Id,
        Guid PortfolioId,
        Guid StockCardId,
        string Ticker,
        string Name,
        string Description,
        int TotalQuantity,
        decimal AveragePurchasePrice,
        DateTime LastUpdated,
        string Currency)
    {
        /// <summary>
        /// Тип актива
        /// </summary>
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public PortfolioAssetType AssetType { get; init; }

        /// <summary>
        /// Список торговых операций по активу
        /// </summary>
        public IReadOnlyCollection<PortfolioAssetTransactionResponse> Transactions { get; init; } = [];
    }
}