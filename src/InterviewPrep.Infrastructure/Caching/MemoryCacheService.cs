using InterviewPrep.Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace InterviewPrep.Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public bool TryGet<T>(string key, out T? value) => _cache.TryGetValue(key, out value);

    public void Set<T>(string key, T value, TimeSpan expiration) => _cache.Set(key, value, expiration);

    public void Remove(string key) => _cache.Remove(key);
}
