using Microsoft.AspNetCore.SignalR;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.WebApi.Hubs;
using StockMarketAssistant.StockCardService.WebApi.Models.Moex;
using System.Collections.Concurrent;

namespace StockMarketAssistant.StockCardService.WebApi.BackgroundServices
{
    public class PriceStreamingService(
        IHubContext<PriceHub> hubContext,
        IPriceService priceService,
        ILogger<PriceStreamingService> logger) : BackgroundService
    {
        private readonly IHubContext<PriceHub> _hubContext = hubContext;
        private readonly IPriceService _priceService = priceService;
        private readonly ILogger<PriceStreamingService> _logger = logger;

        // Храним активные подписки
        private readonly ConcurrentDictionary<string, HashSet<string>> _groups = new();
        private readonly Dictionary<string, decimal> _previousPrices = [];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("✅ PriceStreamingService: запущен");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Получаем список всех активных тикеров (групп)
                    var tickers = _groups.Keys.ToList();

                    if (tickers.Count == 0)
                    {
                        await Task.Delay(1500, stoppingToken);
                        continue;
                    }

                    // Опрос каждого тикера
                    foreach (var ticker in tickers)
                    {
                        var data = await _priceService.GetPriceForTickerAsync(ticker);
                        if (data == null) continue;

                        var currentPrice = data.Price;
                        var previousPrice = _previousPrices.GetValueOrDefault(ticker);
                        var change = currentPrice - previousPrice;
                        var changePercent = previousPrice != 0
                            ? Math.Round(change / previousPrice * 100, 2)
                            : 0;

                        var updateDto = new PriceUpdateDto
                        {
                            Ticker = ticker,
                            Price = currentPrice,
                            Change = change,
                            ChangePercent = changePercent,
                            Time = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                            Volume = data.Volume,
                            NumTrades = data.NumTrades
                        };

                        // Отправляем всем в группе
                        await _hubContext.Clients.Group(ticker.ToUpper()).SendAsync("PriceUpdate", updateDto, stoppingToken);
                        _logger.LogDebug("📈 Отправлено: {Ticker} = {Price} ₽", ticker, currentPrice);

                        _previousPrices[ticker] = currentPrice;
                    }

                    await Task.Delay(1500, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "🔴 Ошибка в сервисе стриминга");
                    await Task.Delay(5000, stoppingToken);
                }
            }
        }

        // Вызывается из PriceHub
        public async Task SubscribeAsync(string connectionId, IEnumerable<string> tickers)
        {
            foreach (var ticker in tickers)
            {
                var group = ticker.ToUpper();
                _groups.AddOrUpdate(group, _ => [connectionId],
                    (_, set) => { set.Add(connectionId); return set; });

                await _hubContext.Groups.AddToGroupAsync(connectionId, group);
                _logger.LogInformation("✅ Подключение {Conn} подписано на {Ticker}", connectionId, ticker);
            }
        }

        // Вызывается из PriceHub
        public async Task UnsubscribeAsync(string connectionId, IEnumerable<string> tickers)
        {
            foreach (var ticker in tickers)
            {
                var group = ticker.ToUpper();
                _groups.AddOrUpdate(group, _ => [],
                    (_, set) => { set.Remove(connectionId); return set; });

                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, group);
                _logger.LogInformation("❌ Подключение {Conn} отписано от {Ticker}", connectionId, ticker);
            }
        }
    }
}
