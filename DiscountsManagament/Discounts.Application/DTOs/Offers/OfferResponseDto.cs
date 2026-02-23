// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Enums;

namespace Discounts.Application.DTOs.Offers
{
    public class OfferResponseDto
    {
        // used by customers to see offers, this is ouput dto
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string MerchantCompanyName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal DiscountedPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public int TotalCoupons { get; set; }
        public int RemainingCoupons { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public OfferStatus Status { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
