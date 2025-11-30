namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель контроллера для создаваемого портфеля ценных бумаг
    /// </summary>
    /// <param name="UserId">Идентификатор пользователя-владельца портфеля</param>
    /// <param name="Name">Наименование портфеля</param>
    /// <param name="Currency">Валюта портфеля (RUB, USD и т.д.)</param>
    /// <param name="IsPrivate">Скрыть из публичной статистики</param>
    public record CreatePortfolioRequest(Guid UserId, string Name, string Currency, bool IsPrivate = false);
}
