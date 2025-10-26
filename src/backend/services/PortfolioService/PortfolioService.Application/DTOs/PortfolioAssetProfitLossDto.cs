using StockMarketAssistant.SharedLibrary.Enums;

namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO, представляющий расчёт доходности (Profit & Loss) для одного актива в портфеле, как отдельный отчёт
    /// </summary>    
    public record PortfolioAssetProfitLossDto(
        /// <summary>
        /// Уникальный идентификатор актива в портфеле
        /// </summary>
        Guid AssetId,
        /// <summary>
        /// Уникальный идентификатор портфеля, которому принадлежит актив
        /// </summary>
        Guid PortfolioId,
        /// <summary>
        /// Тикер актива
        /// </summary>
        string Ticker,
        /// <summary>
        /// Наименование актива
        /// </summary>
        string AssetName,
        /// <summary>
        /// Абсолютная прибыль или убыток актива
        /// </summary>
        decimal? AbsoluteReturn,
        /// <summary>
        /// Процентная прибыль или убыток актива
        /// </summary>
        decimal? PercentageReturn,
        /// <summary>
        /// Общая сумма инвестиций в этот актив (база стоимости)
        /// </summary>
        decimal InvestmentAmount,
        /// <summary>
        /// Текущая рыночная стоимость активов этого типа в портфеле
        /// </summary>
        decimal? CurrentValue,
        /// <summary>
        /// Валюта, в которой представлены значения актива
        /// </summary>
        string Currency,
        /// <summary>
        /// Количество единиц актива в портфеле
        /// </summary>
        int Quantity,
        /// <summary>
        /// Средневзвешенная цена покупки актива
        /// </summary>
        decimal AveragePurchasePrice,
        /// <summary>
        /// Текущая рыночная цена одной единицы актива
        /// </summary>
        decimal? CurrentPrice,
        /// <summary>
        /// Тип расчёта доходности
        /// </summary>
        CalculationType CalculationType = CalculationType.Current
    );
}
