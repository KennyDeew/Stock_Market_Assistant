using Microsoft.AspNetCore.SignalR;
using StockMarketAssistant.StockCardService.WebApi.BackgroundServices;

namespace StockMarketAssistant.StockCardService.WebApi.Hubs
{
    /// <summary>
    /// SignalR хаб для управления подписками клиентов на обновления цен по тикерам.
    /// </summary>
    public class PriceHub : Hub
    {
        private readonly PriceStreamingService _streamingService;
        private readonly ILogger<PriceHub> _logger;

        public PriceHub(PriceStreamingService streamingService, ILogger<PriceHub> logger)
        {
            _streamingService = streamingService;
            _logger = logger;
        }

        /// <summary>
        /// Подписка на тикеры
        /// </summary>
        /// <param name="tickers"></param>
        /// <returns></returns>
        public async Task Subscribe(IEnumerable<string> tickers)
        {
            await _streamingService.SubscribeAsync(Context.ConnectionId, tickers);
            _logger.LogInformation("Клиент {ClientId} подписан на: {Tickers}", Context.ConnectionId, string.Join(", ", tickers));
        }

        /// <summary>
        /// Отписка от тикеров
        /// </summary>
        /// <param name="tickers"></param>
        /// <returns></returns>
        public async Task Unsubscribe(IEnumerable<string> tickers)
        {
            await _streamingService.UnsubscribeAsync(Context.ConnectionId, tickers);
            _logger.LogInformation("Клиент {ClientId} отписан от: {Tickers}", Context.ConnectionId, string.Join(", ", tickers));
        }
    }
}
