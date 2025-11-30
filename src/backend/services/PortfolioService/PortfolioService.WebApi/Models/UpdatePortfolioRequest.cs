namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель контроллера для редактируемого портфеля ценных бумаг
    /// </summary>
    /// <param name="Name">Наименование портфеля</param>
    /// <param name="Currency">Валюта портфеля (RUB, USD и т.д.)</param>
    /// <param name="IsPrivate">Сделать приватным для публичной статистики</param>
    public record UpdatePortfolioRequest(string Name, string Currency, bool IsPrivate);
}
