namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO для портфеля ценных бумаг
    /// </summary>
    public record PortfolioDto
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
        /// Валюта портфеля (USD, RUB и т.д.)
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Коллекция финансовых активов в портфеле (только для чтения)
        /// </summary>
        public IReadOnlyCollection<PortfolioAssetDto> Assets { get; init; } = [];
    }
}
