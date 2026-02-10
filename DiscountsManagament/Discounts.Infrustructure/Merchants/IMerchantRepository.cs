using Discounts.Domain.Entity;
using Discounts.Infrustructure.Repositories;

namespace Discounts.Infrustructure.Merchants;


public interface IMerchantRepository : IBaseRepository<Merchant>
{
    Task<Merchant?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<Merchant?> GetWithOffersAsync(int merchantId, CancellationToken cancellationToken = default);
}