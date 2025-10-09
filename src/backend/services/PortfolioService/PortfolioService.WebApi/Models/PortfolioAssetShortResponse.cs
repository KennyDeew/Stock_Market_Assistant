namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель контроллера для краткой версии возвращаемого значения по финансовому активу из портфеля ценных бумаг
    /// </summary>
    /// <param name="Id">Уникальный идентификатор актива</param>
    /// <param name="PortfolioId">Идентификатор портфеля</param>
    /// <param name="Ticker">Тикер ценной бумаги</param>
    /// <param name="TotalQuantity">Общее количество актива, шт.</param>
    /// <param name="AveragePurchasePrice">Средняя цена покупки</param>
    public record PortfolioAssetShortResponse(Guid Id, Guid PortfolioId, string Ticker, int TotalQuantity = 0, decimal AveragePurchasePrice = 0);
}
