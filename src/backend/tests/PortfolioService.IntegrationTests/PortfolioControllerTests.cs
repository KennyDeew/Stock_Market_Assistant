using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;
using StockMarketAssistant.PortfolioService.IntegrationTests.Helpers;
using StockMarketAssistant.PortfolioService.WebApi;
using StockMarketAssistant.PortfolioService.WebApi.Models;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace StockMarketAssistant.PortfolioService.IntegrationTests
{
    public class PortfolioControllerTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
    {
        [Fact]
        public async Task CreatePortfolio_ReturnsCreated()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Обновляем токен
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Add("Authorization",
                "Bearer " + JwtHelper.GenerateTestToken(userId.ToString(), "USER"));

            var createRequest = new
            {
                UserId = userId,
                Name = "Test Portfolio",
                Currency = "RUB"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/portfolios", createRequest);

            // Debug: вывести тело ответа при ошибке
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Response status: {response.StatusCode}");
                Console.WriteLine($"Response body: {errorContent}");
            }

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<PortfolioDto>();

            Assert.NotNull(result);
            Assert.Equal("Test Portfolio", result.Name);
        }

        [Fact]
        public async Task GetPortfolioById_ReturnsOkWhenUserAccessesOwnPortfolio()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Устанавливаем авторизацию для владельца портфеля
            _client.DefaultRequestHeaders.Remove("Authorization");
            _client.DefaultRequestHeaders.Add("Authorization",
                "Bearer " + JwtHelper.GenerateTestToken(userId.ToString(), "USER"));

            // Создаём портфель через API (а не напрямую через сервис)
            var createRequest = new
            {
                UserId = userId,
                Name = "My Portfolio",
                Currency = "RUB"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/v1/portfolios", createRequest);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var createdPortfolio = await createResponse.Content.ReadFromJsonAsync<PortfolioShortResponse>();
            Assert.NotNull(createdPortfolio);
            var portfolioId = createdPortfolio.Id;

            // Act
            var getResponse = await _client.GetAsync($"/api/v1/portfolios/{portfolioId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var result = await getResponse.Content.ReadFromJsonAsync<PortfolioResponse>();
            Assert.NotNull(result);
            Assert.Equal(portfolioId, result.Id);
            Assert.Equal(userId, result.UserId);
            Assert.Equal("My Portfolio", result.Name);
        }
    }
}
