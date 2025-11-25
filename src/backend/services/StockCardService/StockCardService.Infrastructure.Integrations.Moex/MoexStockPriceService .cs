using Microsoft.Extensions.Logging;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using System.Text.Json;

namespace StockCardService.Infrastructure.Integrations.Moex
{
    public class MoexStockPriceService : IStockPriceService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MoexStockPriceService> _logger;

        public MoexStockPriceService(HttpClient httpClient, ILogger<MoexStockPriceService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Определение цены актива
        /// </summary>
        /// <param name="ticker">Тикер актива</param>
        /// <param name="market">Тип актива (АКЦИИ - shares, ОБЛИГАЦИИ - bonds)</param>
        /// <param name="board">Режим торгов (АКЦИИ - TQBR, КорпОбл - TQCB, ОФЗ - TQOB)</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<decimal?> GetCurrentPriceAsync(string ticker, string market, string board, CancellationToken cancellationToken = default)
        {
            // Примерный запрос к API Мосбиржи (нужно адаптировать под нужный endpoint)
            var url = $"https://iss.moex.com/iss/engines/stock/markets/{market}/boards/{board}/securities/{ticker}.json?iss.meta=off";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            using var doc = JsonDocument.Parse(json);

            // Корень (root) является массивом
            JsonElement root = doc.RootElement;
            // Если корень — массив, ищем массив "marketdata" или "marketdata_yields"
            var fieldName = "securities";
            var propName = "PREVWAPRICE";
            if (board == "TQOB")
            {
                fieldName = "marketdata_yields";
                propName = "PRICE";
            }
            else if (board == "TQBR")
            {
                fieldName = "marketdata";
                propName = "LAST";
            }

            //var typestr = root.ValueKind.ToString();
            //_logger.LogInformation($"Start. fieldName - {fieldName},propName - {propName} type - {typestr}");

            if (!root.TryGetProperty(fieldName, out var marketData))
            {
                _logger.LogWarning("Response does not contain property '{fieldName}'", fieldName);
                return null;
            }

            // Проверяем наличие секций "columns" и "data"
            if (!marketData.TryGetProperty("columns", out var columnsElem) ||
                !marketData.TryGetProperty("data", out var dataElem))
            {
                _logger.LogWarning("'{fieldName}' does not contain expected fields", fieldName);
                return null;
            }

            var columns = columnsElem.EnumerateArray().Select(c => c.GetString()).ToList();
            var priceIndex = columns.IndexOf(propName);

            if (priceIndex < 0)
            {
                _logger.LogWarning("'{propName}' not found in columns", propName);
                return null;
            }
            // Берём первую строку данных (если есть)
            if (dataElem.ValueKind == JsonValueKind.Array && dataElem.GetArrayLength() > 0)
            {
                var firstRow = dataElem[0];
                if (firstRow.ValueKind == JsonValueKind.Array && firstRow.GetArrayLength() > priceIndex)
                {
                    var priceValue = firstRow[priceIndex];
                    if (priceValue.ValueKind == JsonValueKind.Number)
                    {
                        var price = priceValue.GetDecimal();
                        _logger.LogInformation("Fetched price for {Ticker}: {Price}", ticker, price);
                        return price;
                    }
                }
            }

            _logger.LogWarning("Could not extract price for {Ticker}", ticker);
            return null;
        }
    }
}
