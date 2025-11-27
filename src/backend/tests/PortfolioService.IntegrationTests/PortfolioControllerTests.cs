using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using StockMarketAssistant.PortfolioService.Application.DTOs;
using StockMarketAssistant.PortfolioService.Application.Interfaces;
using StockMarketAssistant.PortfolioService.Infrastructure.EntityFramework.Context;
using StockMarketAssistant.PortfolioService.IntegrationTests.Helpers;
using StockMarketAssistant.PortfolioService.WebApi;
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
            using var scope = _factory.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var createRequest = new
            {
                UserId = Guid.NewGuid(),
                Name = "Test Portfolio",
                Currency = "RUB"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/portfolios", createRequest);

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
            var scope = _factory.Services.CreateScope();
            var portfolioAppService = scope.ServiceProvider.GetRequiredService<IPortfolioAppService>();

            var dto = new CreatingPortfolioDto(userId, "Test", "RUB");
            var createdId = await portfolioAppService.CreateAsync(dto);

            // Установить токен владельца
            _client.DefaultRequestHeaders.Authorization = null;
            _client.DefaultRequestHeaders.Add("Authorization", "Bearer " + JwtHelper.GenerateTestToken(userId.ToString(), "USER"));

            // Act
            var response = await _client.GetAsync($"/api/v1/portfolios/{createdId}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

    }
}
