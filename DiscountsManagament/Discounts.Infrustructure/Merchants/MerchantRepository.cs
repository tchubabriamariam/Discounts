using Discounts.Domain.Entity;
using Discounts.Infrustructure.Repositories;
using Discounts.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrustructure.Merchants;

public class MerchantRepository : BaseRepository<Merchant>, IMerchantRepository
{
    public MerchantRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Merchant?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken);
    }

    public async Task<Merchant?> GetWithOffersAsync(int merchantId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(m => m.Offers)
            .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);
    }
}