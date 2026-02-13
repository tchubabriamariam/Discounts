// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs.Coupons;

namespace Discounts.Application.Services.Interfaces
{
    public interface ICouponService
    {
        Task<IEnumerable<CouponResponseDto>> GetMyCouponsAsync(string userId,
            CancellationToken cancellationToken = default);

        Task<CouponResponseDto> GetCouponByCodeAsync(string userId, string code,
            CancellationToken cancellationToken = default);

        Task<CouponResponseDto> MarkCouponAsUsedAsync(string merchantUserId, MarkCouponAsUsedRequestDto request,
            CancellationToken cancellationToken = default);
    }
}
