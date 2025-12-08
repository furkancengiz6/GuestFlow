using GuestFlow.Application.Models;

namespace GuestFlow.Api.Models
{
    /// <summary>
    /// Sayfalanmış sonuç için generic sınıf (Application katmanındaki PagedResult'a alias)
    /// </summary>
    public class PagedResult<T> : Application.Models.PagedResult<T>
    {
        public PagedResult() : base()
        {
        }

        public PagedResult(System.Collections.Generic.List<T> data, int totalCount, int pageNumber, int pageSize) 
            : base(data, totalCount, pageNumber, pageSize)
        {
        }
    }

    /// <summary>
    /// Sayfalama parametreleri (Application katmanındaki PagingParameters'a alias)
    /// </summary>
    public class PagingParameters : Application.Models.PagingParameters
    {
    }
}

