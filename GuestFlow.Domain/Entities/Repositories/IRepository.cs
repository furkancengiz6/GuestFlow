using GuestFlow.Domain.Entities.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GuestFlow.Domain.Entities.Repositories
{
    public interface IRepository<TEntity> where TEntity : BaseEntity
    {
        Task AddAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
        Task DeleteAsync(int id);
        Task UpdateAsync(TEntity entity);
        void Update(TEntity entity);
        
        // Temel Get metodları
        Task<TEntity> GetByIdAsync(int id, bool includeDeleted = false);
        Task<TEntity> GetByIdAsync(int id, params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);
        
        // GetAll metodları
        IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>>? predicate = null, bool includeDeleted = false);
        IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>>? predicate, params Expression<Func<TEntity, object>>[] includes);
        
        // Specification pattern desteği
        Task<TEntity> GetBySpecificationAsync(ISpecification<TEntity> specification);
        Task<List<TEntity>> GetAllBySpecificationAsync(ISpecification<TEntity> specification);
        IQueryable<TEntity> GetQueryableBySpecification(ISpecification<TEntity> specification);
        
        // Count ve Any metodları
        Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, bool includeDeleted = false);
        Task<bool> AnyAsync(Expression<Func<TEntity, bool>>? predicate = null, bool includeDeleted = false);
    }
}
