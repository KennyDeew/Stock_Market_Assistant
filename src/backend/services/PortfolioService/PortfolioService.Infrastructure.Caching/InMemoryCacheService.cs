using Microsoft.Extensions.Caching.Memory;
using StockMarketAssistant.PortfolioService.Application.Interfaces.Caching;

namespace StockMarketAssistant.PortfolioService.Infrastructure.Caching
{
    public class InMemoryCacheService : ICacheService
    {
        private readonly MemoryCache _cache = new(new MemoryCacheOptions());

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(key, out T? value))
            {
                return Task.FromResult(value);
            }
            return Task.FromResult<T?>(default);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var options = expiration.HasValue
                ? new MemoryCacheEntryOptions().SetAbsoluteExpiration(expiration.Value)
                : null;

            _cache.Set(key, value, options);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}
