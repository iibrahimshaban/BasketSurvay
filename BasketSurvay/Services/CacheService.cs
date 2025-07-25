
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace BasketSurvay.Services
{
    public class CacheService(IDistributedCache distributedCache) : ICacheService
    {
        private readonly IDistributedCache _distributedCache = distributedCache;

        public async Task<T?> GetAsync<T>(string CacheKey, CancellationToken cancellationToken = default) where T : class
        {
            var CachedValue = await _distributedCache.GetStringAsync(CacheKey, cancellationToken);

            return string.IsNullOrEmpty(CachedValue)
                ? null
                : JsonSerializer.Deserialize<T>(CachedValue);
        }
        public async Task SetAsync<T>(string CacheKey, T Value, CancellationToken cancellationToken = default) where T : class
        {
            await _distributedCache.SetStringAsync(CacheKey, JsonSerializer.Serialize(Value), cancellationToken);
        }
        public async Task RemoveAsync(string CacheKey, CancellationToken cancellationToken = default)
        {
            await _distributedCache.RemoveAsync(CacheKey, cancellationToken);
        }
    }
}
