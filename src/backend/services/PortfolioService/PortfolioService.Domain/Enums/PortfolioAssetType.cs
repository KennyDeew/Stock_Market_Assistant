using System.ComponentModel;

namespace StockMarketAssistant.PortfolioService.Domain.Enums
{
    /// <summary>
    /// Тип финансового актива из портфеля
    /// </summary>
    public enum PortfolioAssetType : short
    {
        /// <summary>
        /// Акция
        /// </summary>
        [Description("Акция")]
        Share = 1,

        /// <summary>
        /// Облигация
        /// </summary>
        [Description("Облигация")]
        Bond = 2,

        /// <summary>
        /// Криптовалюта
        /// </summary>
        [Description("Криптовалюта")]
        Crypto = 3,

        /// <summary>
        /// Неизвестный тип актива
        /// </summary>
        [Description("Неизвестный тип актива")]
        Unknown = 4
    }
}
