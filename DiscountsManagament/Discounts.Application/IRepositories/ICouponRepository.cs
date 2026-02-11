using Discounts.Domain.Entity;
namespace Discounts.Application.IRepositories;

public interface ICouponRepository : IBaseRepository<Coupon>
{
    Task<IEnumerable<Coupon>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Coupon>> GetByOfferIdAsync(int offerId, CancellationToken cancellationToken = default);
    Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Coupon>> GetExpiredCouponsAsync(CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
}