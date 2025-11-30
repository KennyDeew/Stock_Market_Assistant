namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель контроллера для возвращаемого значения по портфелю ценных бумаг
    /// </summary>
    public record PortfolioResponse
    {
        /// <summary>
        /// Уникальный идентификатор портфеля
        /// </summary>
        public Guid Id { get; init; }

        /// <summary>
        /// Идентификатор пользователя-владельца портфеля
        /// </summary>
        public Guid UserId { get; init; }

        /// <summary>
        /// Наименование портфеля
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Валюта портфеля (RUB, USD и т.д.)
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Скрыть из публичной статистики
        /// </summary>
        public bool IsPrivate { get; init; }

        /// <summary>
        /// Перечень активов ценных бумаг в портфеле
        /// </summary>
        public IReadOnlyCollection<PortfolioAssetShortResponse> Assets { get; init; } = [];
    }
}
