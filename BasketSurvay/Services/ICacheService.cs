namespace BasketSurvay.Services
{
    public interface ICacheService
    {
        Task<T?> GetAsync<T>(string CacheKey, CancellationToken cancellationToken = default) where T : class;
        Task SetAsync<T>(string CacheKey, T Value, CancellationToken cancellationToken = default) where T : class;
        Task RemoveAsync(string CacheKey, CancellationToken cancellationToken = default);
    }
}
