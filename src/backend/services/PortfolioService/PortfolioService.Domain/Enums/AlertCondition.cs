using System.ComponentModel;

namespace StockMarketAssistant.PortfolioService.Domain.Enums
{
    /// <summary>
    /// Условие срабатывания уведомления пользователя о достижении цены
    /// </summary>
    public enum AlertCondition : short
    {
        /// <summary>
        /// Выше
        /// </summary>
        [Description("Выше")]
        Above = 1,
        /// <summary>
        /// Ниже
        /// </summary>
        [Description("Ниже")]
        Below = 2
    }
}
