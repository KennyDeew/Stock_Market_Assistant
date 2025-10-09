using System.ComponentModel;

namespace StockMarketAssistant.PortfolioService.Domain
{
    public enum PortfolioAssetType : short
    {
        [Description("Акция")]
        Share = 1,

        [Description("Облигация")]
        Bond = 2,

        [Description("Криптовалюта")]
        Crypto = 3,

        [Description("Неизвестный тип актива")]
        Unknown = 4
    }
}
