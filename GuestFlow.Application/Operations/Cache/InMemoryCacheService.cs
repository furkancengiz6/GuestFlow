using Microsoft.Extensions.Caching.Memory;

namespace GuestFlow.Application.Operations.Cache;

/// <summary>
/// Simple in-memory cache implementation
/// Used when Redis is not available
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public InMemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public T? Get<T>(string key) where T : class
    {
        _memoryCache.TryGetValue(key, out T? value);
        return value;
    }

    public void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }

        _memoryCache.Set(key, value, options);
    }

    public void Remove(string key)
    {
        _memoryCache.Remove(key);
    }

    public void RemoveByPattern(string pattern)
    {
        // In-memory cache doesn't support pattern removal
        // This is a no-op for compatibility
    }

    public void Clear()
    {
        // In-memory cache doesn't support clear operation
        // This is a no-op for compatibility
    }

    public bool Exists(string key)
    {
        return _memoryCache.TryGetValue(key, out _);
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        _memoryCache.TryGetValue(key, out T value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }

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
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }

    public Task ClearAsync()
    {
        // In-memory cache doesn't support clear operation
        // This is a no-op for compatibility
        return Task.CompletedTask;
    }

    public Task SetBatchAsync<T>(IEnumerable<KeyValuePair<string, T>> keyValuePairs, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }

        foreach (var kvp in keyValuePairs)
        {
            _memoryCache.Set(kvp.Key, kvp.Value, options);
        }

        return Task.CompletedTask;
    }

    public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
    {
        if (_memoryCache.TryGetValue(key, out T cachedValue))
        {
            return Task.FromResult(cachedValue);
        }

        // For simplicity, we'll just call the factory and cache the result
        // In a real implementation, you'd want to use GetOrCreateAsync
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
        {
            options.AbsoluteExpirationRelativeToNow = expiration.Value;
        }

        return factory().ContinueWith(task =>
        {
            var value = task.Result;
            _memoryCache.Set(key, value, options);
            return value;
        });
    }

    public Task<IDictionary<string, T>> GetBatchAsync<T>(IEnumerable<string> keys)
    {
        var result = new Dictionary<string, T>();
        foreach (var key in keys)
        {
            if (_memoryCache.TryGetValue(key, out T value))
            {
                result[key] = value;
            }
        }
        return Task.FromResult((IDictionary<string, T>)result);
    }
}