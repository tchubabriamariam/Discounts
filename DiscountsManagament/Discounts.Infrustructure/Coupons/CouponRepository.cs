using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using Discounts.Infrustructure.Repositories;
using Discounts.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrustructure.Coupons;

public class CouponRepository : BaseRepository<Coupon>, ICouponRepository
{
    public CouponRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Coupon>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.UserId == userId)
            .Include(c => c.Offer)
            .ThenInclude(o => o.Merchant)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Coupon>> GetByOfferIdAsync(int offerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.OfferId == offerId)
            .Include(c => c.User)
            .ToListAsync(cancellationToken);
    }

    public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Offer)
            .FirstOrDefaultAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<Coupon>> GetExpiredCouponsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ExpiresAt <= DateTime.UtcNow && c.Status == CouponStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(c => c.Code == code, cancellationToken);
    }
}