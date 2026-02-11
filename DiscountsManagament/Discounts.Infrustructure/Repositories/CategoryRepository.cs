using Discounts.Application.IRepositories;
using Discounts.Domain.Entity;
using Discounts.Infrustructure.Repositories;
using Discounts.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrustructure.Repositories;
public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
{
    public CategoryRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Category>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.Name == name, cancellationToken);
    }
}