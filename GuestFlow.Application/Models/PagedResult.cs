using System;
using System.Collections.Generic;

namespace GuestFlow.Application.Models
{
    /// <summary>
    /// Sayfalanmış sonuç için generic sınıf
    /// </summary>
    public class PagedResult<T>
    {
        /// <summary>
        /// Sayfadaki veriler
        /// </summary>
        public List<T> Data { get; set; } = new List<T>();

        /// <summary>
        /// Toplam kayıt sayısı
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Sayfa numarası (1'den başlar)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Sayfa başına kayıt sayısı
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Toplam sayfa sayısı
        /// </summary>
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        /// <summary>
        /// Önceki sayfa var mı?
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Sonraki sayfa var mı?
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// İlk sayfa mı?
        /// </summary>
        public bool IsFirstPage => PageNumber == 1;

        /// <summary>
        /// Son sayfa mı?
        /// </summary>
        public bool IsLastPage => PageNumber == TotalPages || TotalPages == 0;

        public PagedResult()
        {
        }

        public PagedResult(List<T> data, int totalCount, int pageNumber, int pageSize)
        {
            Data = data;
            TotalCount = totalCount;
            PageNumber = pageNumber;
            PageSize = pageSize;
        }
    }

    /// <summary>
    /// Sayfalama parametreleri
    /// </summary>
    public class PagingParameters
    {
        private int _pageNumber = 1;
        private int _pageSize = 10;

        /// <summary>
        /// Sayfa numarası (varsayılan: 1)
        /// </summary>
        public int PageNumber
        {
            get => _pageNumber;
            set => _pageNumber = value < 1 ? 1 : value;
        }

        /// <summary>
        /// Sayfa başına kayıt sayısı (varsayılan: 10, maksimum: 100)
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value < 1 ? 10 : (value > 100 ? 100 : value);
        }

        /// <summary>
        /// Skip değeri (sayfalama için)
        /// </summary>
        public int Skip => (PageNumber - 1) * PageSize;

        /// <summary>
        /// Take değeri (sayfalama için)
        /// </summary>
        public int Take => PageSize;
    }
}

