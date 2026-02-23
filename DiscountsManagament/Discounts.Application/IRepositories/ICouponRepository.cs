// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;

namespace Discounts.Application.IRepositories
{
    public interface ICouponRepository : IBaseRepository<Coupon>
    {
        // returns all coupons belonging to specific customer
        Task<IEnumerable<Coupon>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        // lets merchant to see everyone who has purchased a specific deal
        Task<IEnumerable<Coupon>> GetByOfferIdAsync(int offerId, CancellationToken cancellationToken = default);

        // merchant uses code to see a coupon
        Task<Coupon?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

        // sees expired coupons, uses worker
        Task<IEnumerable<Coupon>> GetExpiredCouponsAsync(CancellationToken cancellationToken = default);

        //ensure generated coupons are unique
        Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
    }
}
