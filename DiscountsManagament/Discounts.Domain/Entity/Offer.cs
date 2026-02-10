using Discounts.Domain.Enums;
namespace Discounts.Domain.Entity;

public class Offer : BaseEntity{
    public int MerchantId { get; set; }
    public int CategoryId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }

    public decimal OriginalPrice { get; set; }
    public decimal DiscountedPrice { get; set; }

    public int TotalCoupons { get; set; }
    public int RemainingCoupons { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public OfferStatus Status { get; set; } = OfferStatus.Pending;

    public string? RejectionReason { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByAdminId { get; set; }

    // this is caluclated
    public decimal DiscountPercentage =>
        OriginalPrice > 0
            ? Math.Round((OriginalPrice - DiscountedPrice) / OriginalPrice * 100, 2)
            : 0;

    // Navigation
    public Merchant Merchant { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public ApplicationUser? ApprovedByAdmin { get; set; }
    public ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}

