using GuestFlow.Application.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace GuestFlow.Application.Extensions
{
    /// <summary>
    /// Sayfalama için extension metodları
    /// </summary>
    public static class PagingExtensions
    {
        /// <summary>
        /// IQueryable'ı sayfalanmış sonuca dönüştürür
        /// </summary>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            int pageNumber,
            int pageSize)
        {
            var totalCount = await query.CountAsync();
            var data = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>(data, totalCount, pageNumber, pageSize);
        }

        /// <summary>
        /// IQueryable'ı sayfalanmış sonuca dönüştürür (PagingParameters ile)
        /// </summary>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            PagingParameters paging)
        {
            return await query.ToPagedResultAsync(paging.PageNumber, paging.PageSize);
        }
    }
}

