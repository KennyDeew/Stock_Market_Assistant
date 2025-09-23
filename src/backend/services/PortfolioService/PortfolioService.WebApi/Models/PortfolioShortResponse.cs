namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель контроллера для краткой версии возвращаемого значения по портфелю ценных бумаг
    /// </summary>
    /// <param name="Id">Уникальный идентификатор портфеля</param>
    /// <param name="UserId">Идентификатор пользователя-владельца портфеля</param>
    /// <param name="Name">Наименование портфеля</param>
    /// <param name="Currency">Валюта портфеля (RUB, USD и т.д.)</param>
    public record PortfolioShortResponse(Guid Id, Guid UserId, string Name, string Currency);
}