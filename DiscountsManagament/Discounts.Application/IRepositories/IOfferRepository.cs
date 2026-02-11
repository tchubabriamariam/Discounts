using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
namespace Discounts.Application.IRepositories;

public interface IOfferRepository : IBaseRepository<Offer>
{
    Task<IEnumerable<Offer>> GetByMerchantIdAsync(int merchantId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Offer>> GetByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Offer>> GetByStatusAsync(OfferStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<Offer>> GetApprovedOffersAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Offer>> GetExpiredOffersAsync(CancellationToken cancellationToken = default);
    Task<Offer?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default); 
}