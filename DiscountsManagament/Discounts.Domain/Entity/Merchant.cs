namespace Discounts.Domain.Entity;

public class Merchant : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? ContactEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public bool IsVerified { get; set; } = false;

    // Navigation
    public ApplicationUser User { get; set; } = null!;
    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
}