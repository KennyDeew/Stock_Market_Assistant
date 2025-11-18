namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO, представляющий расчёт доходности (Profit & Loss) для портфеля
    /// </summary>    
    public record PortfolioProfitLossDto(
        /// <summary>
        /// Уникальный идентификатор портфеля
        /// </summary>
        Guid PortfolioId,
        /// <summary>
        /// Имя портфеля
        /// </summary>
        string PortfolioName,
        /// <summary>
        /// Общая абсолютная прибыль или убыток портфеля
        /// </summary>
        decimal TotalAbsoluteReturn,
        /// <summary>
        /// Общая процентная прибыль или убыток портфеля
        /// </summary>
        decimal TotalPercentageReturn,
        /// <summary>
        /// Общая сумма инвестиций в портфель (база стоимости)
        /// </summary>
        decimal TotalInvestment,
        /// <summary>
        /// Общая текущая рыночная стоимость активов в портфеле
        /// </summary>
        decimal TotalCurrentValue,
        /// <summary>
        /// Основная валюта, в которой представлены значения
        /// </summary>
        string BaseCurrency,
        /// <summary>
        /// Дата и время, когда был выполнен расчёт доходности
        /// </summary>
        DateTime CalculatedAt,
        /// <summary>
        /// Список DTO, представляющих доходность для каждого актива в портфеле
        /// </summary>
        IEnumerable<PortfolioAssetProfitLossItemDto> AssetBreakdown);
}
