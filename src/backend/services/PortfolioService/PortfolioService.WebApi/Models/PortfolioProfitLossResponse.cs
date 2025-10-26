namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель контроллера для расчета доходности портфеля
    /// </summary>
    public record PortfolioProfitLossResponse
    {
        /// <summary>
        /// Уникальный идентификатор портфеля
        /// </summary>
        public Guid PortfolioId { get; init; }

        /// <summary>
        /// Наименование портфеля
        /// </summary>
        public required string PortfolioName { get; init; }

        /// <summary>
        /// Общая абсолютная прибыль или убыток портфеля
        /// </summary>
        public decimal TotalAbsoluteReturn { get; init; }

        /// <summary>
        /// Общая процентная прибыль или убыток портфеля
        /// </summary>
        public decimal TotalPercentageReturn { get; init; }

        /// <summary>
        /// Общая сумма инвестиций в портфель
        /// </summary>
        public decimal TotalInvestment { get; init; }

        /// <summary>
        /// Общая текущая рыночная стоимость активов в портфеле
        /// </summary>
        public decimal TotalCurrentValue { get; init; }

        /// <summary>
        /// Основная валюта портфеля
        /// </summary>
        public required string BaseCurrency { get; init; }

        /// <summary>
        /// Дата и время расчета доходности
        /// </summary>
        public DateTime CalculatedAt { get; init; }

        /// <summary>
        /// Детализация доходности по активам портфеля
        /// </summary>
        public IReadOnlyCollection<PortfolioAssetProfitLossItemResponse> AssetBreakdown { get; init; } = [];
    }
}
