// Copyright (C) TBC Bank. All Rights Reserved.

using System.Linq.Expressions;
using Discounts.Application.IRepositories;
using Discounts.Domain.Entity;
using Discounts.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrustructure.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
            await _dbSet.FindAsync([id], cancellationToken).ConfigureAwait(false);

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default) =>
            await _dbSet.ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default) =>
            await _dbSet.Where(predicate).ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default) =>
            await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken).ConfigureAwait(false);

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate,
            CancellationToken cancellationToken = default) =>
            await _dbSet.AnyAsync(predicate, cancellationToken).ConfigureAwait(false);

        public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null,
            CancellationToken cancellationToken = default)
        {
            if (predicate is null) return await _dbSet.CountAsync(cancellationToken).ConfigureAwait(false);

            return await _dbSet.CountAsync(predicate, cancellationToken).ConfigureAwait(false);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
            await _dbSet.AddAsync(entity, cancellationToken).ConfigureAwait(false);

        public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default) =>
            await _dbSet.AddRangeAsync(entities, cancellationToken).ConfigureAwait(false);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);

        public void SoftDelete(T entity)
        {
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }

        public IQueryable<T> Query() => _dbSet.AsQueryable();
    }
}
