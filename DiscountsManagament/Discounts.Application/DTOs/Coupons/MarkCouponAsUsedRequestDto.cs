// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.DTOs.Coupons
{
    public class MarkCouponAsUsedRequestDto
    {
        // used by merchant to find coupon with this code, input dto only needs code
        public string Code { get; set; } = string.Empty;
    }
}
