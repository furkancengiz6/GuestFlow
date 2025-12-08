using System;
using System.Linq;
using System.Linq.Expressions;

using GuestFlow.Domain.Entities.Core;

namespace GuestFlow.Domain.Entities.Repositories
{
    /// <summary>
    /// Specification pattern interface
    /// Karmaşık sorgular için kullanılır
    /// </summary>
    public interface ISpecification<TEntity> where TEntity : BaseEntity
    {
        /// <summary>
        /// Where koşulu
        /// </summary>
        Expression<Func<TEntity, bool>>? Criteria { get; }

        /// <summary>
        /// Include edilecek navigation property'ler
        /// </summary>
        Expression<Func<TEntity, object>>[] Includes { get; }

        /// <summary>
        /// ThenInclude edilecek navigation property'ler (nested)
        /// </summary>
        Expression<Func<object, object>>[] ThenIncludes { get; }

        /// <summary>
        /// OrderBy ifadesi
        /// </summary>
        Expression<Func<TEntity, object>>? OrderBy { get; }

        /// <summary>
        /// OrderByDescending ifadesi
        /// </summary>
        Expression<Func<TEntity, object>>? OrderByDescending { get; }

        /// <summary>
        /// Skip değeri (sayfalama için)
        /// </summary>
        int? Skip { get; }

        /// <summary>
        /// Take değeri (sayfalama için)
        /// </summary>
        int? Take { get; }

        /// <summary>
        /// Soft delete kontrolü yapılsın mı?
        /// </summary>
        bool IncludeDeleted { get; }
    }
}

