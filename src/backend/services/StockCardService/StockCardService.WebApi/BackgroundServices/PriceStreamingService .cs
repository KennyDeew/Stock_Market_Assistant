using Microsoft.AspNetCore.SignalR;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using StockMarketAssistant.StockCardService.WebApi.Hubs;
using StockMarketAssistant.StockCardService.WebApi.Models;
using System.Collections.Concurrent;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StockMarketAssistant.StockCardService.WebApi.BackgroundServices
{
    /// <summary>
    /// Фоновый сервис, который периодически получает актуальные цены
    /// и передаёт их через интерфейс SignalR-клиента.
    /// </summary>
    public class PriceStreamingService : BackgroundService
    {
        private readonly IHubContext<PriceHub> _hubContext;
        private readonly IStockPriceService _priceService;
        private readonly ILogger<PriceStreamingService> _logger;

        // Словарь: группа (тикер) -> набор connectionId (в виде ConcurrentDictionary для потокобезопасности)
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _groups = new();

        // Предыдущие цены для расчёта изменений
        private readonly ConcurrentDictionary<string, decimal> _previousPrices = new();

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="hubContext"></param>
        /// <param name="priceService"></param>
        /// <param name="logger"></param>
        public PriceStreamingService(IHubContext<PriceHub> hubContext, IStockPriceService priceService,ILogger<PriceStreamingService> logger)
        {
            _hubContext = hubContext;
            _priceService = priceService;
            _logger = logger;
        }

        /// <summary>
        /// Основной цикл фонового сервиса: периодически опрашивать цены по всем активным тикерам
        /// и рассылать обновления клиентам в соответствующих группах.
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Сервис стриминга цен запущен");

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
                    foreach (var tickerAndBoard in tickers)
                    {
                        _logger.LogInformation($"попытка определить цену: {tickerAndBoard}");
                        if (tickerAndBoard.Split('_').Length == 2)
                        {
                            var tickerValue = tickerAndBoard.Split('_')[0];
                            var board = tickerAndBoard.Split('_')[1];
                            _logger.LogInformation($"Запрос {tickerValue}/SHARES/{board}");
                            var stockPrice = await _priceService.GetStockPricesAsync(tickerValue, "shares", board, stoppingToken);
                            var currentPrice = stockPrice.Price;
                            var previousPrice = _previousPrices.GetValueOrDefault(tickerAndBoard);
                            var changePrice = currentPrice - previousPrice;
                            stockPrice.ChangePrice = currentPrice - previousPrice;
                            _logger.LogInformation($"📈 Определено: {tickerAndBoard} = {stockPrice.Price} ₽. Изменение - {changePrice}");
                            // Отправляем всем в группе
                            await _hubContext.Clients.Group(tickerAndBoard.ToUpperInvariant()).SendAsync("PriceUpdate", stockPrice);
                            _logger.LogDebug("📈 Отправлено: {Ticker} = {Price} ₽", tickerAndBoard, stockPrice.Price);
                            _previousPrices[tickerAndBoard] = currentPrice;
                        }
                        //_logger.LogInformation("Broadcasted prices to clients");
                        //_logger.LogDebug("Цена {Ticker}: {Price}", price.Ticker, price.Price);
                    }

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка в PriceStreamingService");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("Сервис стриминга цен остановлен");
        }

        /// <summary>
        /// Подписка. Сбрасывает все текущие подписки клиента и подписывает на новые.
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="tickers"></param>
        /// <returns></returns>
        public async Task SubscribeAsync(string connectionId, IEnumerable<string> tickers)
        {
            if (tickers == null) tickers = Array.Empty<string>();

            // 1. Отписываемся от всех текущих групп клиента
            //var currentGroups = _groups
            //    .Where(kv => kv.Value.ContainsKey(connectionId))
            //    .Select(kv => kv.Key)
            //    .ToList();

            //foreach (var group in currentGroups)
            //{
            //    if (_groups.TryGetValue(group, out var connections))
            //    {
            //        connections.TryRemove(connectionId, out _);
            //        if (connections.IsEmpty)
            //        {
            //            _groups.TryRemove(group, out _);
            //        }
            //    }
            //    await _hubContext.Groups.RemoveFromGroupAsync(connectionId, group);
            //    _logger.LogInformation("Подключение {Conn} отписано от {Ticker}", connectionId, group);
            //}

            // 2. Подписываем на новые тикеры
            foreach (var rawTicker in tickers)
            {
                if (string.IsNullOrWhiteSpace(rawTicker)) continue;

                var group = rawTicker.ToUpperInvariant();
                var connections = _groups.GetOrAdd(group, _ => new ConcurrentDictionary<string, byte>());
                connections[connectionId] = 0;

                await _hubContext.Groups.AddToGroupAsync(connectionId, group);
                _logger.LogInformation("Подключение {Conn} подписано на {Ticker}", connectionId, group);
            }
        }

        /// <summary>
        /// Отписка. Вызывается из PriceHub
        /// </summary>
        /// <param name="connectionId"></param>
        /// <param name="tickers"></param>
        /// <returns></returns>
        public async Task UnsubscribeAsync(string connectionId, IEnumerable<string> tickers)
        {
            if (tickers == null) return;

            foreach (var rawTicker in tickers)
            {
                if (string.IsNullOrWhiteSpace(rawTicker)) continue;

                var group = rawTicker.ToUpperInvariant();

                if (_groups.TryGetValue(group, out var connections))
                {
                    connections.TryRemove(connectionId, out _);

                    // Если больше нет подключений — удаляем запись о группе
                    if (connections.IsEmpty)
                    {
                        _groups.TryRemove(group, out _);
                    }
                }

                await _hubContext.Groups.RemoveFromGroupAsync(connectionId, group);
                _logger.LogInformation("Подключение {Conn} отписано от {Ticker}", connectionId, group);
            }
        }
    }
}
