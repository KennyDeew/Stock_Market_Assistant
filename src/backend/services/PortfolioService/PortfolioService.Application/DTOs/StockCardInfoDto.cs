namespace StockMarketAssistant.PortfolioService.Application.DTOs
{
    /// <summary>
    /// DTO для информации о ценной бумаге из сервиса StockCardService
    /// </summary>
    public record StockCardInfoDto(string Ticker, string Name, string Description, string Currency = "RUB");
}
