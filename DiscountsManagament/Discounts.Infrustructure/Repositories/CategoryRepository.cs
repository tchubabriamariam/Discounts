// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.IRepositories;
using Discounts.Domain.Entity;
using Discounts.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrustructure.Repositories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Category>> GetActiveAsync(CancellationToken cancellationToken = default) =>
            await _dbSet
                .Where(c => c.IsActive)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default) =>
            await _dbSet.AnyAsync(c => c.Name == name, cancellationToken).ConfigureAwait(false);
    }
}
