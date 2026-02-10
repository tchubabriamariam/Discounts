using Discounts.Domain.Entity;
using Discounts.Infrustructure.Repositories;

namespace Discounts.Infrustructure.Categoires;

public interface ICategoryRepository : IBaseRepository<Category>
{
    Task<IEnumerable<Category>> GetActiveAsync(CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
}