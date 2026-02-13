// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.IRepositories;
using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using Discounts.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrustructure.Repositories
{
    public class CouponRepository : BaseRepository<Coupon>, ICouponRepository
    {
        public CouponRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Coupon>> GetByUserIdAsync(string userId,
            CancellationToken cancellationToken = default) =>
            await _dbSet
                .Where(c => c.UserId == userId)
                .Include(c => c.Offer)
                .ThenInclude(o => o.Merchant)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<IEnumerable<Coupon>> GetByOfferIdAsync(int offerId,
            CancellationToken cancellationToken = default) =>
            await _dbSet
                .Where(c => c.OfferId == offerId)
                .Include(c => c.User)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            await _dbSet
                .Include(c => c.Offer)
                .FirstOrDefaultAsync(c => c.Code == code, cancellationToken).ConfigureAwait(false);

        public async Task<IEnumerable<Coupon>> GetExpiredCouponsAsync(CancellationToken cancellationToken = default) =>
            await _dbSet
                .Where(c => c.ExpiresAt <= DateTime.UtcNow && c.Status == CouponStatus.Active)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default) =>
            await _dbSet.AnyAsync(c => c.Code == code, cancellationToken).ConfigureAwait(false);
    }
}
