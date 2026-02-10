using Discounts.Domain.Enums;

namespace Discounts.Domain.Entity;


public class Coupon : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int OfferId { get; set; }

    public string Code { get; set; } = string.Empty; // this is unqiue
    public decimal PricePaid { get; set; }

    public CouponStatus Status { get; set; } = CouponStatus.Active;

    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UsedAt { get; set; }
    public DateTime ExpiresAt { get; set; }

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public Offer Offer { get; set; } = null!;
}