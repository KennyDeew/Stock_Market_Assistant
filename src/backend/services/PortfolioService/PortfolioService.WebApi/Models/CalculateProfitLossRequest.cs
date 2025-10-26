using StockMarketAssistant.SharedLibrary.Enums;
using System.ComponentModel.DataAnnotations;

namespace StockMarketAssistant.PortfolioService.WebApi.Models
{

    /// <summary>
    /// Модель контроллера для запроса расчета доходности
    /// </summary>
    public record CalculateProfitLossRequest
    {
        /// <summary>
        /// Тип расчёта доходности:
        /// - Current: Текущая доходность (значение: 1)
        /// - Realized: Реализованная доходность (значение: 2)
        /// </summary>
        [EnumDataType(typeof(CalculationType))]
        public CalculationType CalculationType { get; init; } = CalculationType.Current;
    }
}
