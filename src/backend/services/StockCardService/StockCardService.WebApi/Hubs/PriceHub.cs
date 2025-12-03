namespace StockMarketAssistant.StockCardService.WebApi.Hubs
{
    using Microsoft.AspNetCore.SignalR;
    using StockMarketAssistant.StockCardService.WebApi.BackgroundServices;

    public class PriceHub(PriceStreamingService streamingService, ILogger<PriceHub> logger) : Hub
    {
        private readonly PriceStreamingService _streamingService = streamingService;
        private readonly ILogger<PriceHub> _logger = logger;

        // Подписка на тикеры
        public async Task Subscribe(IEnumerable<string> tickers)
        {
            await _streamingService.SubscribeAsync(Context.ConnectionId, tickers);
            _logger.LogInformation("Клиент {ClientId} подписан на: {Tickers}", Context.ConnectionId, string.Join(", ", tickers));
        }

        // Отписка от тикеров
        public async Task Unsubscribe(IEnumerable<string> tickers)
        {
            await _streamingService.UnsubscribeAsync(Context.ConnectionId, tickers);
            _logger.LogInformation("Клиент {ClientId} отписан от: {Tickers}", Context.ConnectionId, string.Join(", ", tickers));
        }
    }
}
