// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Enums;

namespace Discounts.Domain.Entity
{
    public class Reservation : BaseEntity
    {
        // you have this until you pay then you get coupon
        public string UserId { get; set; } = string.Empty; // customer who booked
        public int OfferId { get; set; } // reservation belongs to one offer

        public int Quantity { get; set; } = 1; // amount of coupons, default to 1 because can't buy 0 coupons
        public ReservationStatus Status { get; set; } = ReservationStatus.Active;

        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } // set by admin's global setting, worker looks at this, if it is expired it expires

        // Navigation
        public ApplicationUser User { get; set; } = null!; // one to one, this is for customer
        public Offer Offer { get; set; } = null!;
    }
}
