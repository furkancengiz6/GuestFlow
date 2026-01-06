using GuestFlow.Application.Configuration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Cache
{
    /// <summary>
    /// Cache servisi implementasyonu (MemoryCache)
    /// </summary>
    public class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheSettings _settings;
        private readonly ILogger<CacheService> _logger;
        private static readonly Dictionary<string, object> _cacheKeys = new Dictionary<string, object>();

        public CacheService(
            IMemoryCache memoryCache,
            IOptions<CacheSettings> settings,
            ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _settings = settings.Value;
            _logger = logger;
        }

        public T? Get<T>(string key) where T : class
        {
            if (!_settings.Enabled)
                return null;

            try
            {
                if (_memoryCache.TryGetValue(key, out var value))
                {
                    return value as T;
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cache'den değer getirilirken hata: {key}");
                return null;
            }
        }

        public void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            if (!_settings.Enabled)
                return;

            try
            {
                var cacheOptions = new MemoryCacheEntryOptions();

                if (expiration.HasValue)
                {
                    cacheOptions.AbsoluteExpirationRelativeToNow = expiration.Value;
                }
                else
                {
                    // Endpoint bazlı cache duration kontrolü
                    var duration = GetCacheDuration(key);
                    cacheOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(duration);
                }

                // Cache key'i kaydet (pattern matching için)
                _cacheKeys[key] = value;

                _memoryCache.Set(key, value, cacheOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cache'e değer eklenirken hata: {key}");
            }
        }

        public Task<T?> GetAsync<T>(string key) where T : class
        {
            return Task.FromResult(Get<T>(key));
        }

        public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            Set(key, value, expiration);
            return Task.CompletedTask;
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class
        {
            if (!_settings.Enabled)
            {
                return await factory();
            }

            var cached = Get<T>(key);
            if (cached != null)
            {
                _logger.LogDebug($"Cache hit: {key}");
                return cached;
            }

            _logger.LogDebug($"Cache miss: {key}");
            var value = await factory();
            Set(key, value, expiration);
            return value;
        }

        public void Remove(string key)
        {
            try
            {
                _memoryCache.Remove(key);
                _cacheKeys.Remove(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Cache'den değer kaldırılırken hata: {key}");
            }
        }

        public Task RemoveAsync(string key)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public void RemoveByPattern(string pattern)
        {
            try
            {
                var regex = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
                var keysToRemove = _cacheKeys.Keys.Where(key => regex.IsMatch(key)).ToList();

                foreach (var key in keysToRemove)
                {
                    _memoryCache.Remove(key);
                    _cacheKeys.Remove(key);
                }

                _logger.LogInformation($"Pattern '{pattern}' ile {keysToRemove.Count} cache key kaldırıldı.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Pattern ile cache temizlenirken hata: {pattern}");
            }
        }

        public void Clear()
        {
            try
            {
                // MemoryCache'in Clear metodu yok, bu yüzden reflection kullanarak temizliyoruz
                if (_memoryCache is MemoryCache mc)
                {
                    var field = typeof(MemoryCache).GetField("_entries", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (field?.GetValue(mc) is System.Collections.IDictionary cacheEntries)
                    {
                        cacheEntries.Clear();
                    }
                }

                _cacheKeys.Clear();
                _logger.LogInformation("Cache temizlendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache temizlenirken hata");
            }
        }

        public bool Exists(string key)
        {
            if (!_settings.Enabled)
                return false;

            return _memoryCache.TryGetValue(key, out _);
        }

        #region Private Methods

        private int GetCacheDuration(string key)
        {
            // Endpoint bazlı cache duration kontrolü
            foreach (var kvp in _settings.CacheDurations)
            {
                if (key.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return _settings.DefaultCacheDurationMinutes;
        }

        #endregion
    }
}

