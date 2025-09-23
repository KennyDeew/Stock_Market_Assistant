using System.ComponentModel;

namespace StockMarketAssistant.PortfolioService.Domain.Enums
{
    /// <summary>
    /// Тип транзакции для финансового актива из портфеля
    /// </summary>
    public enum PortfolioAssetTransactionType : short
    {
        /// <summary>
        /// Покупка
        /// </summary>
        [Description("Покупка")]
        Buy = 1,

        /// <summary>
        /// Продажа
        /// </summary>
        [Description("Продажа")]
        Sell = 2
    }
}
