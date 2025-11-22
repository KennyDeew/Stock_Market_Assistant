namespace StockMarketAssistant.AnalyticsService.Application.DTOs
{
    /// <summary>
    /// DTO для текущего состояния портфеля
    /// </summary>
    public class PortfolioStateDto
    {
        /// <summary>
        /// Идентификатор портфеля
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Название портфеля
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Валюта портфеля
        /// </summary>
        public string Currency { get; set; } = "RUB";

        /// <summary>
        /// Список активов в портфеле
        /// </summary>
        public List<PortfolioAssetStateDto> Assets { get; set; } = new();
    }

    /// <summary>
    /// DTO для состояния актива в портфеле
    /// </summary>
    public class PortfolioAssetStateDto
    {
        /// <summary>
        /// Идентификатор актива портфеля
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор портфеля
        /// </summary>
        public Guid PortfolioId { get; set; }

        /// <summary>
        /// Идентификатор актива (StockCardId)
        /// </summary>
        public Guid StockCardId { get; set; }

        /// <summary>
        /// Тикер актива
        /// </summary>
        public string Ticker { get; set; } = string.Empty;

        /// <summary>
        /// Название актива
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Тип актива (1 = Share, 2 = Bond, 3 = Crypto)
        /// </summary>
        public int AssetType { get; set; }

        /// <summary>
        /// Общее количество активов
        /// </summary>
        public int TotalQuantity { get; set; }

        /// <summary>
        /// Средняя цена покупки
        /// </summary>
        public decimal AveragePurchasePrice { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public string Currency { get; set; } = "RUB";
    }
}

