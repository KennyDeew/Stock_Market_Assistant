using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using StockMarketAssistant.AnalyticsService.Application.DTOs;
using StockMarketAssistant.AnalyticsService.Application.Interfaces;

namespace StockMarketAssistant.AnalyticsService.Infrastructure.EntityFramework.Http
{
    /// <summary>
    /// HTTP клиент для работы с PortfolioService
    /// </summary>
    public class PortfolioServiceClient : IPortfolioServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PortfolioServiceClient> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private const int CacheTTLMinutes = 5;

        public PortfolioServiceClient(
            HttpClient httpClient,
            IMemoryCache cache,
            ILogger<PortfolioServiceClient> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        /// <summary>
        /// Получить историю транзакций портфеля за период (кэшируется)
        /// </summary>
        public async Task<PortfolioHistoryDto?> GetHistoryAsync(
            Guid portfolioId,
            DateTime startDate,
            DateTime endDate,
            CancellationToken cancellationToken = default)
        {
            // Генерируем ключ кэша
            var cacheKey = $"portfolio_history_{portfolioId}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";

            // Проверяем кэш
            if (_cache.TryGetValue(cacheKey, out PortfolioHistoryDto? cachedHistory))
            {
                _logger.LogInformation("Cache HIT для истории портфеля {PortfolioId}", portfolioId);
                return cachedHistory;
            }

            _logger.LogInformation("Cache MISS для истории портфеля {PortfolioId}", portfolioId);

            try
            {
                // Выполняем запрос (Polly политики применяются через HttpClientFactory)
                var portfolioUrl = $"/api/v1/portfolios/{portfolioId}";
                var response = await _httpClient.GetAsync(portfolioUrl, cancellationToken);

                // Обработка ответа
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Портфель {PortfolioId} не найден (404)", portfolioId);
                    return null;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError("PortfolioService недоступен (503) для портфеля {PortfolioId}", portfolioId);
                    response.EnsureSuccessStatusCode(); // Пробрасываем исключение
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var portfolioState = JsonSerializer.Deserialize<PortfolioStateDto>(content, _jsonOptions);

                if (portfolioState == null)
                {
                    return null;
                }

                // Получаем транзакции для каждого актива за период
                var allTransactions = new List<PortfolioTransactionDto>();

                foreach (var asset in portfolioState.Assets)
                {
                    try
                    {
                        var transactionsUrl = $"/api/v1/portfolio-assets/{asset.Id}/transactions/period?startDate={startDate:yyyy-MM-ddTHH:mm:ssZ}&endDate={endDate:yyyy-MM-ddTHH:mm:ssZ}";
                        var transactionsResponse = await _httpClient.GetAsync(transactionsUrl, cancellationToken);

                        if (transactionsResponse.IsSuccessStatusCode)
                        {
                            var transactionsContent = await transactionsResponse.Content.ReadAsStringAsync(cancellationToken);
                            var transactions = JsonSerializer.Deserialize<List<PortfolioAssetTransactionResponseDto>>(transactionsContent, _jsonOptions) ?? new List<PortfolioAssetTransactionResponseDto>();

                            foreach (var transaction in transactions)
                            {
                                allTransactions.Add(new PortfolioTransactionDto
                                {
                                    Id = transaction.Id,
                                    PortfolioAssetId = transaction.PortfolioAssetId,
                                    StockCardId = asset.StockCardId,
                                    TransactionType = (int)transaction.TransactionType,
                                    Quantity = transaction.Quantity,
                                    PricePerUnit = transaction.PricePerUnit,
                                    TotalAmount = transaction.Quantity * transaction.PricePerUnit,
                                    TransactionDate = transaction.TransactionDate,
                                    Currency = transaction.Currency
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Не удалось получить транзакции для актива {AssetId} портфеля {PortfolioId}", asset.Id, portfolioId);
                    }
                }

                var history = new PortfolioHistoryDto
                {
                    PortfolioId = portfolioId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Transactions = allTransactions
                };

                // Сохраняем в кэш на 5 минут
                _cache.Set(cacheKey, history, TimeSpan.FromMinutes(CacheTTLMinutes));
                _logger.LogInformation("История портфеля {PortfolioId} сохранена в кэш", portfolioId);

                return history;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка HTTP запроса к PortfolioService для портфеля {PortfolioId}", portfolioId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при получении истории портфеля {PortfolioId}", portfolioId);
                throw;
            }
        }

        /// <summary>
        /// Получить текущее состояние портфеля (не кэшируется)
        /// </summary>
        public async Task<PortfolioStateDto?> GetCurrentStateAsync(
            Guid portfolioId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Выполняем запрос (Polly политики применяются через HttpClientFactory)
                var url = $"/api/v1/portfolios/{portfolioId}";
                var response = await _httpClient.GetAsync(url, cancellationToken);

                // Обработка ответа
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Портфель {PortfolioId} не найден (404)", portfolioId);
                    return null;
                }

                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError("PortfolioService недоступен (503) для портфеля {PortfolioId}", portfolioId);
                    response.EnsureSuccessStatusCode(); // Пробрасываем исключение
                }

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var state = JsonSerializer.Deserialize<PortfolioStateDto>(content, _jsonOptions);

                return state;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка HTTP запроса к PortfolioService для портфеля {PortfolioId}", portfolioId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при получении состояния портфеля {PortfolioId}", portfolioId);
                throw;
            }
        }

        /// <summary>
        /// Получить состояние нескольких портфелей (не кэшируется)
        /// </summary>
        public async Task<Dictionary<Guid, PortfolioStateDto>> GetMultipleStatesAsync(
            IEnumerable<Guid> portfolioIds,
            CancellationToken cancellationToken = default)
        {
            var portfolioIdsList = portfolioIds.ToList();
            var result = new Dictionary<Guid, PortfolioStateDto>();

            if (!portfolioIdsList.Any())
            {
                return result;
            }

            try
            {
                // Выполняем параллельные запросы для каждого портфеля
                var tasks = portfolioIdsList.Select(async portfolioId =>
                {
                    try
                    {
                        var state = await GetCurrentStateAsync(portfolioId, cancellationToken);
                        if (state != null)
                        {
                            return (portfolioId, state);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Не удалось получить состояние портфеля {PortfolioId}", portfolioId);
                    }
                    return (portfolioId, (PortfolioStateDto?)null);
                });

                var results = await Task.WhenAll(tasks);

                foreach (var (portfolioId, state) in results)
                {
                    if (state != null)
                    {
                        result[portfolioId] = state;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении состояний нескольких портфелей");
                throw;
            }
        }

        /// <summary>
        /// Создать новый портфель
        /// </summary>
        public async Task<Guid> CreatePortfolioAsync(
            Guid userId,
            string name,
            string currency = "RUB",
            bool isPrivate = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var requestBody = new
                {
                    UserId = userId,
                    Name = name,
                    Currency = currency,
                    IsPrivate = isPrivate
                };

                var jsonContent = JsonSerializer.Serialize(requestBody, _jsonOptions);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                var url = "/api/v1/portfolios";
                var response = await _httpClient.PostAsync(url, content, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogError("PortfolioService недоступен (503) при создании портфеля");
                    response.EnsureSuccessStatusCode();
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    _logger.LogWarning("Ошибка при создании портфеля: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    return Guid.Empty;
                }

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var portfolioResponse = JsonSerializer.Deserialize<PortfolioShortResponse>(responseContent, _jsonOptions);

                if (portfolioResponse == null || portfolioResponse.Id == Guid.Empty)
                {
                    _logger.LogWarning("Не удалось получить ID созданного портфеля");
                    return Guid.Empty;
                }

                _logger.LogInformation("Портфель {PortfolioId} успешно создан для пользователя {UserId}", portfolioResponse.Id, userId);
                return portfolioResponse.Id;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка HTTP запроса к PortfolioService при создании портфеля");
                return Guid.Empty;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка при создании портфеля");
                return Guid.Empty;
            }
        }
    }

    /// <summary>
    /// DTO для ответа при создании портфеля
    /// </summary>
    internal record PortfolioShortResponse(Guid Id, Guid UserId, string Name, string Currency, bool IsPrivate);
}

