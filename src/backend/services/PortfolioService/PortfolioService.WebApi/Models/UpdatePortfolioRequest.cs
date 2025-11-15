using System.ComponentModel.DataAnnotations;

namespace StockMarketAssistant.PortfolioService.WebApi.Models
{
    /// <summary>
    /// Модель контроллера для редактируемого портфеля ценных бумаг
    /// </summary>
    /// <param name="Name">Наименование портфеля</param>
    /// <param name="Currency">Валюта портфеля (RUB, USD и т.д.)</param>
    public record UpdatePortfolioRequest([Required] string Name, string Currency);
}
