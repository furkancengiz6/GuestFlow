using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GuestFlow.Domain.Entities.Core;
using GuestFlow.Domain.Entities.Repositories;
using GuestFlow.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace GuestFlow.Persistence.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        private readonly GuestFlowDbContext _context;
        private readonly DbSet<TEntity> _dbSet;

        public Repository(GuestFlowDbContext context)
        {
            _context = context;
            _dbSet = context.Set<TEntity>();
        }

        public async Task AddAsync(TEntity entity)
        {
            entity.CreatedDate = DateTime.UtcNow;
            await _dbSet.AddAsync(entity);
        }

        public async Task DeleteAsync(TEntity entity)
        {
            entity.IsDeleted = true;
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id, includeDeleted: true);
            if (entity != null)
            {
                await DeleteAsync(entity);
            }
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public async Task<TEntity> GetByIdAsync(int id, bool includeDeleted = false)
        {
            var query = _dbSet.AsQueryable();
            
            if (!includeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<TEntity> GetByIdAsync(int id, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _dbSet.AsQueryable().Where(e => !e.IsDeleted && e.Id == id);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false)
        {
            var query = _dbSet.AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            return await query.FirstOrDefaultAsync(predicate);
        }

        public async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _dbSet.AsQueryable().Where(e => !e.IsDeleted).Where(predicate);

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return await query.FirstOrDefaultAsync();
        }

        public IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate = null, bool includeDeleted = false)
        {
            // If includeDeleted is true, ignore the global query filter
            var query = includeDeleted 
                ? _dbSet.IgnoreQueryFilters().AsQueryable()
                : _dbSet.AsQueryable();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            return query;
        }

        public IQueryable<TEntity> GetAll(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
        {
            var query = _dbSet.AsQueryable().Where(e => !e.IsDeleted);

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            foreach (var include in includes)
            {
                query = query.Include(include);
            }

            return query;
        }

        public async Task<TEntity> GetBySpecificationAsync(ISpecification<TEntity> specification)
        {
            var query = ApplySpecification(specification);
            return await query.FirstOrDefaultAsync();
        }

        public async Task<List<TEntity>> GetAllBySpecificationAsync(ISpecification<TEntity> specification)
        {
            var query = ApplySpecification(specification);
            return await query.ToListAsync();
        }

        public IQueryable<TEntity> GetQueryableBySpecification(ISpecification<TEntity> specification)
        {
            return ApplySpecification(specification);
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null, bool includeDeleted = false)
        {
            var query = GetAll(predicate, includeDeleted);
            return await query.CountAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate = null, bool includeDeleted = false)
        {
            var query = GetAll(predicate, includeDeleted);
            return await query.AnyAsync();
        }

        /// <summary>
        /// Specification'ı query'ye uygular
        /// </summary>
        private IQueryable<TEntity> ApplySpecification(ISpecification<TEntity> specification)
        {
            var query = _dbSet.AsQueryable();

            // Soft delete filtreleme
            if (!specification.IncludeDeleted)
            {
                query = query.Where(e => !e.IsDeleted);
            }

            // Where koşulu
            if (specification.Criteria != null)
            {
                query = query.Where(specification.Criteria);
            }

            // Include'lar
            foreach (var include in specification.Includes)
            {
                query = query.Include(include);
            }

            // OrderBy
            if (specification.OrderBy != null)
            {
                query = query.OrderBy(specification.OrderBy);
            }
            else if (specification.OrderByDescending != null)
            {
                query = query.OrderByDescending(specification.OrderByDescending);
            }

            // Sayfalama
            if (specification.Skip.HasValue)
            {
                query = query.Skip(specification.Skip.Value);
            }

            if (specification.Take.HasValue)
            {
                query = query.Take(specification.Take.Value);
            }

            return query;
        }
    }
}
