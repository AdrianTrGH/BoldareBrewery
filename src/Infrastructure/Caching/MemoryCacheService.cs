using BoldareBrewery.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace BoldareBrewery.Infrastructure.Caching
{
    public class MemoryCacheService: ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            var result = _memoryCache.Get<T>(key);
            return Task.FromResult(result);
        }

        public Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            _memoryCache.Set(key, value, options);
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            _memoryCache.Remove(key);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key)
        {
            var exists = _memoryCache.TryGetValue(key, out _);
            return Task.FromResult(exists);
        }
    }
}
