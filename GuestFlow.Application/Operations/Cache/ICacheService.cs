using System;
using System.Threading.Tasks;

namespace GuestFlow.Application.Operations.Cache
{
    /// <summary>
    /// Cache servisi interface'i
    /// </summary>
    public interface ICacheService
    {
        /// <summary>
        /// Cache'den değer getirir
        /// </summary>
        T? Get<T>(string key) where T : class;

        /// <summary>
        /// Cache'e değer ekler
        /// </summary>
        void Set<T>(string key, T value, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// Cache'den değer getirir, yoksa factory metodunu çalıştırıp cache'e ekler
        /// </summary>
        Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiration = null) where T : class;

        /// <summary>
        /// Cache'den değer kaldırır
        /// </summary>
        void Remove(string key);

        /// <summary>
        /// Cache'den değer kaldırır (async)
        /// </summary>
        Task RemoveAsync(string key);

        /// <summary>
        /// Belirli bir pattern'e uyan tüm cache key'lerini kaldırır
        /// </summary>
        void RemoveByPattern(string pattern);

        /// <summary>
        /// Cache'i temizler
        /// </summary>
        void Clear();

        /// <summary>
        /// Cache'de key var mı kontrol eder
        /// </summary>
        bool Exists(string key);

        /// <summary>
        /// Cache'den değer getirir (async)
        /// </summary>
        Task<T?> GetAsync<T>(string key) where T : class;

        /// <summary>
        /// Cache'e değer ekler (async)
        /// </summary>
        Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    }
}