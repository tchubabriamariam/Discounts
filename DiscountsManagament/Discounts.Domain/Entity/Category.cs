// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Domain.Entity
{
    public class Category : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public bool IsActive { get; set; } = true; // admin can deactivate

        // Navigation
        public ICollection<Offer> Offers { get; set; } = new List<Offer>(); // one category can have many offers, one to many
    }
}
