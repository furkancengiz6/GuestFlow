using System;
using System.Linq;
using System.Linq.Expressions;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Interfaces;

namespace GuestFlow.Domain.Entities.Repositories
{
    /// <summary>
    /// Specification pattern base implementation
    /// </summary>
    public class Specification<TEntity> : ISpecification<TEntity> where TEntity : BaseEntity, ISoftDelete
    {
        public Expression<Func<TEntity, bool>>? Criteria { get; set; }
        public Expression<Func<TEntity, object>>[] Includes { get; set; } = Array.Empty<Expression<Func<TEntity, object>>>();
        public Expression<Func<object, object>>[] ThenIncludes { get; set; } = Array.Empty<Expression<Func<object, object>>>();
        public Expression<Func<TEntity, object>>? OrderBy { get; set; }
        public Expression<Func<TEntity, object>>? OrderByDescending { get; set; }
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public bool IncludeDeleted { get; set; } = false;
    }

    /// <summary>
    /// Specification builder
    /// </summary>
    public class SpecificationBuilder<TEntity> where TEntity : BaseEntity, ISoftDelete
    {
        private readonly Specification<TEntity> _specification;

        public SpecificationBuilder()
        {
            _specification = new Specification<TEntity>();
        }

        public SpecificationBuilder<TEntity> Where(Expression<Func<TEntity, bool>> criteria)
        {
            _specification.Criteria = criteria;
            return this;
        }

        public SpecificationBuilder<TEntity> Include(params Expression<Func<TEntity, object>>[] includes)
        {
            _specification.Includes = includes;
            return this;
        }

        public SpecificationBuilder<TEntity> OrderBy(Expression<Func<TEntity, object>> orderBy)
        {
            _specification.OrderBy = orderBy;
            return this;
        }

        public SpecificationBuilder<TEntity> OrderByDescending(Expression<Func<TEntity, object>> orderByDescending)
        {
            _specification.OrderByDescending = orderByDescending;
            return this;
        }

        public SpecificationBuilder<TEntity> Skip(int skip)
        {
            _specification.Skip = skip;
            return this;
        }

        public SpecificationBuilder<TEntity> Take(int take)
        {
            _specification.Take = take;
            return this;
        }

        public SpecificationBuilder<TEntity> IncludeDeleted(bool includeDeleted = true)
        {
            _specification.IncludeDeleted = includeDeleted;
            return this;
        }

        public ISpecification<TEntity> Build()
        {
            return _specification;
        }
    }
}

