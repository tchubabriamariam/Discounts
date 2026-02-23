// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.DTOs.Offers
{
    public class SalesHistoryResponseDto
    {
        // for merchant's report, see who bought what and when
        public string CustomerFullName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CouponCode { get; set; } = string.Empty;
        public decimal PricePaid { get; set; }
        public DateTime PurchasedAt { get; set; }
        public string OfferTitle { get; set; } = string.Empty;
    }
}
