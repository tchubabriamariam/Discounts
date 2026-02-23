// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Domain.Entity
{
    public class Merchant : BaseEntity
    {
        // this is a organization or company
        public string UserId { get; set; } = string.Empty; // explicit fk for performance, allows filtering by id without loading the full User entity, avoiding join
        public string CompanyName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? LogoUrl { get; set; }
        public string? ContactEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsVerified { get; set; } // this is for admin to confirm that this merchant is legal

        // Navigation
        public ApplicationUser User { get; set; } = null!; // to link with ApplicationUser table, one to one
        public ICollection<Offer> Offers { get; set; } = new List<Offer>(); // merchant creates offers, one to many
    }
}
