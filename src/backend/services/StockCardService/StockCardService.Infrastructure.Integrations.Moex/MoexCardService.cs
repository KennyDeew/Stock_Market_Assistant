using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver.Search;
using StockCardService.Abstractions.Repositories;
using StockMarketAssistant.StockCardService.Domain.Entities;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading;
using System.Xml.Linq;

namespace StockCardService.Infrastructure.Integrations.Moex
{
    /// <summary>
    /// Класс получения перечня акаций, ОФЗ и Корп. облигаций
    /// </summary>
    public class MoexCardService : IMoexCardService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<MoexStockPriceService> _logger;

        public MoexCardService(HttpClient httpClient, ILogger<MoexStockPriceService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        private readonly string moexShareCardsUrl = "https://iss.moex.com/iss/engines/stock/markets/shares/boards/TQBR/securities.json?iss.only=securities&iss.meta=off&securities.columns=SECID,SHORTNAME,PREVPRICE,SECNAME";
        private readonly string moexOfzBondsUrl = "https://iss.moex.com/iss/securities.json?q=SU26&group_by=type&group_by_filter=ofz_bond&limit=80&start=0&securities.columns=secid,shortname,name,primary_boardid&iss.meta=off";
        private readonly string moexCorpBondsUrl = "https://iss.moex.com/iss/securities.json?group_by=type&group_by_filter=corporate_bond&limit=60&start=0&securities.columns=secid,shortname,name,primary_boardid&iss.meta=off3";
        
        public async Task<IEnumerable<BondCard>> GetCorporateBondCardsFromMoex(CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(moexCorpBondsUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = doc.RootElement;
            //_logger.LogError($"Start CorpBond SEC");
            if (!root.TryGetProperty("securities", out var securitiesElem))
            {
                _logger.LogWarning("MOEX response does not contain 'securities'");
                return Enumerable.Empty<BondCard>();
            }

            if (!securitiesElem.TryGetProperty("columns", out var columnsElem) ||
                !securitiesElem.TryGetProperty("data", out var dataElem))
            {
                _logger.LogWarning("'securities' section is missing 'columns' or 'data'");
                return Enumerable.Empty<BondCard>();
            }
            //_logger.LogError($"Start CorpBond Get Prop");
            // Определяем индексы колонок
            var columns = columnsElem.EnumerateArray().Select(c => c.GetString()).ToList();

            int idxSecId = columns.IndexOf("secid");
            int idxShortName = columns.IndexOf("shortname");
            int idxName = columns.IndexOf("name");
            int idxBoard = columns.IndexOf("primary_boardid");

            if (idxSecId < 0 || idxShortName < 0)
            {
                _logger.LogError("Required columns not found in MOEX response");
                return Enumerable.Empty<BondCard>();
            }

            var result = new List<BondCard>();
            foreach (var row in dataElem.EnumerateArray())
            {
                if (row.ValueKind != JsonValueKind.Array)
                    continue;

                var board = row[idxBoard].GetString() ?? string.Empty;
                if (board != "TQCB")
                    continue;

                var ticker = row[idxSecId].GetString() ?? string.Empty;
                var bondName = row[idxShortName].GetString() ?? string.Empty;
                var bondDesc = row[idxName].GetString() ?? string.Empty;
                //_logger.LogError($"firstCorpBond - {ticker}/{board}/{bondName}/{bondDesc}");
                var bondCard = await GetBondCardFromMoex(ticker, board, bondName, bondDesc, cancellationToken);
                if (bondCard != null)
                    result.Add(bondCard);
            }
            return result;
        }

        public async Task<IEnumerable<BondCard>> GetOFZBondCardsFromMoex(CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(moexOfzBondsUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = doc.RootElement;
            //_logger.LogError($"Start Bond SEC");
            if (!root.TryGetProperty("securities", out var securitiesElem))
            {
                _logger.LogWarning("MOEX response does not contain 'securities'");
                return Enumerable.Empty<BondCard>();
            }

            if (!securitiesElem.TryGetProperty("columns", out var columnsElem) ||
                !securitiesElem.TryGetProperty("data", out var dataElem))
            {
                _logger.LogWarning("'securities' section is missing 'columns' or 'data'");
                return Enumerable.Empty<BondCard>();
            }
            //_logger.LogError($"Start Bond Get Prop");
            // Определяем индексы колонок
            var columns = columnsElem.EnumerateArray().Select(c => c.GetString()).ToList();

            int idxSecId = columns.IndexOf("secid");
            int idxShortName = columns.IndexOf("shortname");
            int idxName = columns.IndexOf("name");
            int idxBoard = columns.IndexOf("primary_boardid");

            if (idxSecId < 0 || idxShortName < 0)
            {
                _logger.LogError("Required columns not found in MOEX response");
                return Enumerable.Empty<BondCard>();
            }
            
            var result = new List<BondCard>();
            foreach (var row in dataElem.EnumerateArray())
            {
                //_logger.LogError($"type - {row.ValueKind.ToString()}");
                if (row.ValueKind != JsonValueKind.Array)
                    continue;

                var ticker = row[idxSecId].GetString() ?? string.Empty;
                var board = row[idxBoard].GetString() ?? string.Empty;
                var bondName = row[idxShortName].GetString() ?? string.Empty;
                var bondDesc = row[idxName].GetString() ?? string.Empty;
                //_logger.LogError($"firstBond - {ticker}/{board}/{bondName}/{bondDesc}");
                var bondCard = await GetBondCardFromMoex(ticker, board, bondName, bondDesc, cancellationToken);
                if (bondCard != null)
                    result.Add(bondCard);
            }
            return result;
        }

        public async Task<IEnumerable<ShareCard>> GetShareCardsFromMoex(CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync(moexShareCardsUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = doc.RootElement;

            if (!root.TryGetProperty("securities", out var securitiesElem))
            {
                _logger.LogWarning("MOEX response does not contain 'securities'");
                return Enumerable.Empty<ShareCard>();
            }

            if (!securitiesElem.TryGetProperty("columns", out var columnsElem) ||
                !securitiesElem.TryGetProperty("data", out var dataElem))
            {
                _logger.LogWarning("'securities' section is missing 'columns' or 'data'");
                return Enumerable.Empty<ShareCard>();
            }

            // Определяем индексы колонок
            var columns = columnsElem.EnumerateArray().Select(c => c.GetString()).ToList();

            int idxSecId = columns.IndexOf("SECID");
            int idxShortName = columns.IndexOf("SHORTNAME");
            int idxPrice = columns.IndexOf("PREVPRICE");
            int idxDescription = columns.IndexOf("SECNAME");

            if (idxSecId < 0 || idxShortName < 0)
            {
                _logger.LogError("Required columns not found in MOEX response");
                return Enumerable.Empty<ShareCard>();
            }

            var result = new List<ShareCard>();
            foreach (var row in dataElem.EnumerateArray())
            {
                if (row.ValueKind != JsonValueKind.Array)
                    continue;

                var tickerV = GetSafeString(row[idxSecId]);
                var nameV = GetSafeString(row[idxShortName]);
                var descV = GetSafeString(row[idxDescription]);
                var curPrStr = GetSafeString(row[idxPrice]);
                var curPrice = decimal.TryParse(curPrStr, out var price) ? price : 0m;

                var shareCard = new ShareCard
                {
                    Ticker = tickerV,
                    Name = nameV,
                    Description = descV,
                    CurrentPrice = curPrice,
                    Currency = "RUB"
                };

                result.Add(shareCard);
            }

            return result;
        }

        public async Task<IEnumerable<Dividend>> GetDividendsForShareCardFromMoex(ShareCard shareCard, CancellationToken cancellationToken)
        {
            var moexDividendsForShareCardUrl = $"http://iss.moex.com/iss/securities/{shareCard.Ticker}/dividends.json?iss.meta=off";
            var response = await _httpClient.GetAsync(moexDividendsForShareCardUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = doc.RootElement;

            if (!root.TryGetProperty("dividends", out var dividendsElem))
            {
                _logger.LogWarning("MOEX response does not contain 'dividends'");
                return Enumerable.Empty<Dividend>();
            }

            if (!dividendsElem.TryGetProperty("columns", out var columnsElem) ||
                !dividendsElem.TryGetProperty("data", out var dataElem))
            {
                _logger.LogWarning("'dividends' section is missing 'columns' or 'data'");
                return Enumerable.Empty<Dividend>();
            }

            // Определяем индексы колонок
            var columns = columnsElem.EnumerateArray().Select(c => c.GetString()).ToList();

            int idxCutOffDate = columns.IndexOf("registryclosedate");
            int idxValue = columns.IndexOf("value");

            if (idxCutOffDate < 0 || idxValue < 0)
            {
                _logger.LogError("Required columns not found in MOEX response");
                return Enumerable.Empty<Dividend>();
            }

            var result = new List<Dividend>();
            foreach (var row in dataElem.EnumerateArray())
            {
                if (row.ValueKind != JsonValueKind.Array)
                    continue;

                var cutOffStr = GetSafeString(row[idxCutOffDate]);
                DateTime cutOffDate = DateTime.MinValue;
                if (!string.IsNullOrWhiteSpace(cutOffStr) &&
                    DateTime.TryParse(cutOffStr, out var parsed))
                {
                    cutOffDate = DateTime.SpecifyKind(DateTime.Parse(cutOffStr), DateTimeKind.Utc);

                }
                var valueStr = GetSafeString(row[idxValue]);
                decimal divValue = 0m;
                if (decimal.TryParse(valueStr, out var value))
                {
                    divValue = value;
                }
                var dividend = new Dividend
                {
                    ParentId = shareCard.Id,
                    Period = "",
                    CutOffDate = cutOffDate,
                    Value = divValue,
                    Currency = "RUB"
                };

                result.Add(dividend);
            }

            return result;
        }

        private async Task<BondCard?> GetBondCardFromMoex(string ticker, string board, string name, string desc, CancellationToken cancellationToken)
        {
            var moexBondUrl = $"https://iss.moex.com/iss/engines/stock/markets/bonds/boards/{board}/securities/{ticker}.json?iss.meta=off";
            var response = await _httpClient.GetAsync(moexBondUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
            var root = doc.RootElement;

            if (!root.TryGetProperty("securities", out var jsonSecurElement))
            {
                _logger.LogWarning("MOEX response does not contain 'securities'");
                return null;
            }
            if (!jsonSecurElement.TryGetProperty("columns", out var columnsSecurElem) ||
                !jsonSecurElement.TryGetProperty("data", out var dataSecurElem))
            {
                _logger.LogWarning("'securities' section of bondResponse Secur is missing 'columns' or 'data'");
                return null;
            }
            // Определяем индексы колонок
            var securColumns = columnsSecurElem.EnumerateArray().Select(c => c.GetString()).ToList();

            int idxFaceValue = securColumns.IndexOf("FACEVALUE");
            int idxStatus = securColumns.IndexOf("STATUS");
            int idxMatDate = securColumns.IndexOf("MATDATE");
            if (idxFaceValue < 0 || idxStatus < 0 || idxMatDate < 0)
            {
                _logger.LogError("Required columns of bondResponse Secur is not found in MOEX response");
                return null;
            }
            //Securites - FACEVALUE, STATUS, MATDATE

            var marketProperty = board == "TQOB" ? "marketdata_yields" : "securities";
            var priceProperty = board == "TQOB" ? "PRICE" : "PREVWAPRICE";
            if (!root.TryGetProperty(marketProperty, out var jsonMarketElement))
            {
                _logger.LogWarning("MOEX response does not contain 'securities'");
                return null;
            }
            if (!jsonMarketElement.TryGetProperty("columns", out var columnsMarketElem) ||
                !jsonMarketElement.TryGetProperty("data", out var dataMarketElem))
            {
                _logger.LogWarning("'securities' section of bondResponse Market_yields is missing 'columns' or 'data'");
                return null;
            }
            // Определяем индексы колонок
            var marketColumns = columnsMarketElem.EnumerateArray().Select(c => c.GetString()).ToList();

            int idxPrice = marketColumns.IndexOf(priceProperty);

            if (idxPrice < 0)
            {
                _logger.LogError("Required columns of bondResponse Market_yields is not found in MOEX response");
                return null;
            }
            var securRows = dataSecurElem.EnumerateArray().ToList();
            if (securRows.Count == 0)
            {
                _logger.LogWarning($"MOEX bond {ticker} returned EMPTY securities data.");
                return null;
            }
            var marketRows = dataMarketElem.EnumerateArray().ToList();
            if (marketRows.Count == 0)
            {
                _logger.LogWarning($"MOEX bond {ticker} returned EMPTY marketdata_yields.");
                return null;
            }
            var secur = securRows[0];
            if (secur.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning($"MOEX bond {ticker}: securities row is NOT Array but {secur.ValueKind}");
                return null;
            }
            var market = marketRows[0];
            if (market.ValueKind != JsonValueKind.Array)
            {
                _logger.LogWarning($"MOEX bond {ticker}: marketdata_yields row is NOT Array but {market.ValueKind}");
                return null;
            }
            var cutOffStr = GetSafeString(secur[idxMatDate]);
            DateTime cutOffDate = DateTime.MinValue;
            if (!string.IsNullOrWhiteSpace(cutOffStr) &&
                DateTime.TryParse(cutOffStr, out var parsed))
            {
                cutOffDate = DateTime.SpecifyKind(DateTime.Parse(cutOffStr), DateTimeKind.Utc);
            }
            var statusV = GetSafeString(secur[idxStatus]);
            var faceValueV = secur[idxFaceValue].TryGetDecimal(out var faceValue) ? faceValue : 0m;
            var curPriceV = GetSafeString(market[idxPrice]);
            decimal priceValue = 0m;
            if (decimal.TryParse(curPriceV, out var pv))
            {
                priceValue = pv > 0 ? pv / 100 * faceValueV : 0m;
            }
            _logger.LogError($"Bond - {ticker}/{name}/{desc}/{board}/{statusV}/{faceValueV.ToString()}/{curPriceV.ToString()}/{cutOffDate.ToString()}");
            return new BondCard()
            {
                Ticker = ticker,
                Name = name,
                Description = desc,
                Board = board,
                Currency = "RUB",
                Rating = statusV,
                FaceValue = faceValueV,
                CurrentPrice = priceValue,
                MaturityPeriod = cutOffDate
            };
            //marketdata_yields - PRICE
        }

        private string GetSafeString(JsonElement el)
        {
            return el.ValueKind switch
            {
                JsonValueKind.String => el.GetString()!,
                JsonValueKind.Number => el.GetRawText(), // число → "123.45"
                JsonValueKind.Null => "",
                JsonValueKind.Undefined => "",
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => el.GetRawText()
            };
        }
    }
}
