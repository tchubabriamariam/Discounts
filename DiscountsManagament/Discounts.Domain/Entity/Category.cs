namespace Discounts.Domain.Entity;

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
}