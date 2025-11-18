using System.Text.Json;

namespace StockMarketAssistant.StockCardService.Application.Services
{
    public static class MoexIssClient
    {
        private static readonly HttpClient http = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        /// <summary>
        /// Возвращает текущую цену (LAST) для акции с указанным SECID на заданном режиме (по умолчанию TQBR).
        /// Значение может быть задержанным ~15 минут без аутентификации.
        /// </summary>
        public static async Task<decimal?> GetCurrentPriceWithoutAuthAsync(string secid, string board = "TQBR")
        {
            if (string.IsNullOrWhiteSpace(secid)) throw new ArgumentException("SECID is required", nameof(secid));

            // Пример запроса:
            // https://iss.moex.com/iss/engines/stock/markets/shares/boards/TQBR/securities/GAZP.json?iss.json=extended
            var url = $"https://iss.moex.com/iss/engines/stock/markets/shares/boards/{Uri.EscapeDataString(board)}/securities/{Uri.EscapeDataString(secid.ToUpper())}.json?iss.json=extended";

            using var req = new HttpRequestMessage(HttpMethod.Get, url);

            // Если у вас есть подписка и аутентификация через passport.moex.com,
            // добавьте cookie MicexPassportCert к запросу (пример):
            // req.Headers.Add("Cookie", "MicexPassportCert=...");

            using var resp = await http.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
            resp.EnsureSuccessStatusCode();

            using var stream = await resp.Content.ReadAsStreamAsync();
            using var doc = await JsonDocument.ParseAsync(stream);

            // Корень (root) является массивом
            JsonElement root = doc.RootElement;
            // Если корень — массив, ищем массив "marketdata"
            if (root.ValueKind == JsonValueKind.Array && root.GetArrayLength() > 1 && root[1].TryGetProperty("marketdata", out var marketDataArr))
            {
                if (marketDataArr.ValueKind == JsonValueKind.Array && marketDataArr.GetArrayLength() > 1)
                {
                    //в marketdata 2 массива: 1й - описание параметров, 2й - их значения. Выделяем значения
                    var propData = marketDataArr[1];
                    if (propData.ValueKind == JsonValueKind.Array && propData.GetArrayLength() > 0)
                    {
                        var propArr = propData[0];
                        if (propArr.TryGetProperty("LAST", out var lastProp) && lastProp.ValueKind == JsonValueKind.Number)
                        {
                            return lastProp.GetDecimal();
                        }
                    }

                }
            }
            return null;
        }
    }
}
