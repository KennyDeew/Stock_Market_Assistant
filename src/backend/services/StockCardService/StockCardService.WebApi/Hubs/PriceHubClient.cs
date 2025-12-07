using Microsoft.AspNetCore.SignalR;
using StockMarketAssistant.StockCardService.Application.DTOs;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.WebApi.Mappers;

namespace StockMarketAssistant.StockCardService.WebApi.Hubs
{
    /// <summary>
    /// Клиент SignalR, реализующий отправку обновлений цен подписанным клиентам.
    /// </summary>
    public class PriceHubClient : IPriceHubClient
    {
        private readonly IHubContext<PriceHub> _hub;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="hub"></param>
        public PriceHubClient(IHubContext<PriceHub> hub)
        {
            _hub = hub;
        }

        /// <summary>
        /// Отправляет обновление цены клиентам, подписанным на указанную группу (тикер).
        /// </summary>
        /// <param name="ticker"></param>
        /// <param name="stockPriceDto"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public Task SendPriceUpdateAsync(string ticker, StockPriceDto stockPriceDto, CancellationToken token = default)
        {
            var stockPriceModel = StockPriceMapper.ToModel(stockPriceDto);
            return _hub.Clients.Group(ticker).SendAsync("PriceUpdate", stockPriceModel);
        }
    }
}