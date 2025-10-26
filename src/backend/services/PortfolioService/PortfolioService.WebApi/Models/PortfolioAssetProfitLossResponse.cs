using StockMarketAssistant.SharedLibrary.Enums;

namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель контроллера для расчета доходности отдельного актива портфеля
    /// </summary>
    public record PortfolioAssetProfitLossResponse
    {
        /// <summary>
        /// Уникальный идентификатор актива в портфеле
        /// </summary>
        public Guid AssetId { get; init; }

        /// <summary>
        /// Уникальный идентификатор портфеля, которому принадлежит актив
        /// </summary>
        public Guid PortfolioId { get; init; }

        /// <summary>
        /// Тикер актива
        /// </summary>
        public required string Ticker { get; init; }

        /// <summary>
        /// Наименование актива
        /// </summary>
        public required string AssetName { get; init; }

        /// <summary>
        /// Абсолютная прибыль или убыток актива
        /// </summary>
        public decimal AbsoluteReturn { get; init; }

        /// <summary>
        /// Процентная прибыль или убыток актива
        /// </summary>
        public decimal PercentageReturn { get; init; }

        /// <summary>
        /// Общая сумма инвестиций в этот актив
        /// </summary>
        public decimal InvestmentAmount { get; init; }

        /// <summary>
        /// Текущая рыночная стоимость активов этого типа в портфеле
        /// </summary>
        public decimal CurrentValue { get; init; }

        /// <summary>
        /// Валюта актива
        /// </summary>
        public required string Currency { get; init; }

        /// <summary>
        /// Количество единиц актива в портфеле
        /// </summary>
        public int Quantity { get; init; }

        /// <summary>
        /// Средневзвешенная цена покупки актива
        /// </summary>
        public decimal AveragePurchasePrice { get; init; }

        /// <summary>
        /// Текущая рыночная цена одной единицы актива
        /// </summary>
        public decimal CurrentPrice { get; init; }

        /// <summary>
        /// Тип расчета доходности
        /// </summary>
        public CalculationType CalculationType { get; init; }
    }
}
