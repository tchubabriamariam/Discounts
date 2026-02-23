// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Enums;

namespace Discounts.Application.DTOs.Coupons
{
    public class CouponResponseDto
    {
        // customer sees this when wants to look at his coupons
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string OfferTitle { get; set; } = string.Empty;
        public string MerchantCompanyName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal PricePaid { get; set; } // what customer paid
        public CouponStatus Status { get; set; }
        public DateTime PurchasedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsExpired { get; set; }
        public int DaysUntilExpiry { get; set; } // good for display
    }
}
