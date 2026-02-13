// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Enums;

namespace Discounts.Domain.Entity
{
    public class Reservation : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public int OfferId { get; set; }

        public int Quantity { get; set; } = 1;
        public ReservationStatus Status { get; set; } = ReservationStatus.Active;

        public DateTime ReservedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } // Set by admin's global setting

        // Navigation
        public ApplicationUser User { get; set; } = null!;
        public Offer Offer { get; set; } = null!;
    }
}
