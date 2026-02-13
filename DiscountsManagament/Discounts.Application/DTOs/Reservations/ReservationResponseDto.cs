// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.DTOs.Reservations
{
    public class ReservationResponseDto
    {
        public int Id { get; set; }
        public int OfferId { get; set; }
        public string OfferTitle { get; set; } = string.Empty;
        public string MerchantCompanyName { get; set; } = string.Empty;
        public decimal PricePerCoupon { get; set; }
        public decimal TotalPrice { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ReservedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int MinutesRemaining { get; set; }
        public bool IsExpired { get; set; }
    }
}
