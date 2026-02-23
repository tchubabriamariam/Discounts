// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Enums;

namespace Discounts.Domain.Entity
{
    public class Coupon : BaseEntity
    {
        // customer owns coupon after purchase
        public string UserId { get; set; } = string.Empty; // this is guid
        public int OfferId { get; set; }

        public string Code { get; set; } = string.Empty; // this is unqiue
        public decimal PricePaid { get; set; }

        public CouponStatus Status { get; set; } = CouponStatus.Active;

        public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UsedAt { get; set; } // empty until custumer uses coupon, verified by merchant
        public DateTime ExpiresAt { get; set; } // this is for worker

        // Navigation
        public ApplicationUser User { get; set; } = null!; // who owns the coupon, this starts as null, it will be loaded by EF Core when needed
        public Offer Offer { get; set; } = null!; // which offer is the coupon from
    }
}
