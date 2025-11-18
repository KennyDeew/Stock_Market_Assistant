using System.ComponentModel;

namespace StockMarketAssistant.SharedLibrary.Enums
{
    /// <summary>
    /// Тип расчёта доходности
    /// </summary>
    public enum CalculationType : short
    {
        /// <summary>
        /// Текущая доходность - разница между текущей стоимостью и общей суммой инвестиций
        /// </summary>
        [Description("Текущая доходность")]
        Current = 1,

        /// <summary>
        /// Реализованная доходность - доход от завершенных сделок (продаж)
        /// </summary>
        [Description("Реализованная доходность")]
        Realized = 2
    }
}
