// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Enums;

namespace Discounts.Domain.Entity
{
    public class Offer : BaseEntity
    {
        public int MerchantId { get; set; } // which merchant created it
        public int CategoryId { get; set; } // which category it belongs

        // display for customners
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }

        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }

        // inventory
        public int TotalCoupons { get; set; }
        public int RemainingCoupons { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }   // this is for worker

        public OfferStatus Status { get; set; } = OfferStatus.Pending; // admin approves until its pending

        public string? RejectionReason { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByAdminId { get; set; } // saves which admin approved

        // this is caluclated in here
        public decimal DiscountPercentage =>
            OriginalPrice > 0
                ? Math.Round((OriginalPrice - DiscountedPrice) / OriginalPrice * 100, 2)
                : 0;

        // Navigation
        public Merchant Merchant { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public ApplicationUser? ApprovedByAdmin { get; set; }
        public ICollection<Coupon> Coupons { get; set; } = new List<Coupon>(); // list of all successful purchases
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>(); //when a reservation is made, RemainingCoupons should decrease


        // I keep the Offer table clean by only storing the RejectionReason,
        // which is the only piece of data the Merchant needs to see to fix their offer
    }
}
