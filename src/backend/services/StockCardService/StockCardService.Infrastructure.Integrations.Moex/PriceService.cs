using Microsoft.Extensions.Logging;
using StockMarketAssistant.StockCardService.Application.DTOs.Moex;
using StockMarketAssistant.StockCardService.Application.Interfaces;
using System.Net.Http.Json;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StockCardService.Infrastructure.Integrations.Moex
{
    public partial class PriceService(HttpClient httpClient, ILogger<PriceService> logger) : IPriceService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<PriceService> _logger = logger;

        [GeneratedRegex("[.,]")]
        private static partial Regex DecimalSeparatorRegex();

        public async Task<PriceData?> GetPriceForTickerAsync(string ticker)
        {
            var boards = new[] { "TQBR", "TQOB", "TQTF", "TQOD", "TQIR" };
            bool dataFetchedSuccessfully = false;
            foreach (var board in boards)
            {
                try
                {
                    var url = $"https://iss.moex.com/iss/engines/stock/markets/shares/boards/{board}/securities/{ticker}.json?iss.only=marketdata";

                    var response = await _httpClient.GetFromJsonAsync<MoexTickerResponse>(url);

                    if (response?.MarketData?.Data == null || response.MarketData.Data.Count == 0)
                        continue;

                    var data = response.MarketData.Data[0];
                    var columns = response.MarketData.Columns;

                    var lastIndex = columns.IndexOf("LAST");
                    var volumeIndex = columns.IndexOf("VOLUME");
                    var numTradesIndex = columns.IndexOf("NUMTRADES");

                    if (lastIndex < 0 || data.Count <= lastIndex || data[lastIndex] == null)
                        continue;

                    if (!decimal.TryParse(DecimalSeparatorRegex().Replace(data[lastIndex].ToString(), CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator), out decimal price))
                        continue;

                    long volume = 0;
                    int numTrades = 0;

                    if (volumeIndex >= 0 && data[volumeIndex] != null)
                        _ = long.TryParse(data[volumeIndex].ToString(), out volume);

                    if (numTradesIndex >= 0 && data[numTradesIndex] != null)
                        _ = int.TryParse(data[numTradesIndex].ToString(), out numTrades);
                    dataFetchedSuccessfully = true;
                    return new PriceData(price, volume, numTrades);
                }
                catch
                {
                    continue;
                }
            }
            if (!dataFetchedSuccessfully)
                _logger.LogError("Ошибка при получении данных для {Ticker}", ticker);
            return null;
        }

    }
}
