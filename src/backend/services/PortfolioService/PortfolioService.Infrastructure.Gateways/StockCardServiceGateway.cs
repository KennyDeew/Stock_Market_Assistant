using Microsoft.Extensions.Logging;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Gateways;
using StockMarketAssistant.SharedLibrary.Models;
using System.Net;
using System.Net.Http.Json;

namespace StockMarketAssistant.PortfolioService.Infrastructure.Gateways
{
    public class StockCardServiceGateway(HttpClient httpClient, ILogger<StockCardServiceGateway> logger)
                : IStockCardServiceGateway
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly ILogger<StockCardServiceGateway> _logger = logger;

        public async Task<ShareCardShortModel?> GetShortShareCardModelByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/ShareCard/short/{id}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<ShareCardShortModel>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка при попытке получения информации об облигации с ID {Id}", id);
                throw;
            }
        }

        public async Task<BondCardShortModel?> GetShortBondCardModelByIdAsync(Guid id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"api/v1/BondCard/short/{id}");
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return null;
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<BondCardShortModel>();
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка при попытке получения информации об акции с ID {Id}", id);
                throw;
            }
        }


        public async Task UpdateAllPricesForShareCardsAsync()
        {
            try
            {
                var response = await _httpClient.PutAsync($"api/v1/ShareCard/UpdateAllPrices",
                new StringContent(string.Empty));

                response.EnsureSuccessStatusCode();
                _logger.LogInformation("Цены акций успешно актуализированы");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Ошибка при попытке обновления цен акций");
                throw;
            }
        }
    }
}
