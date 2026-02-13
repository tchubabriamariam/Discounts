// Copyright (C) TBC Bank. All Rights Reserved.

using Microsoft.AspNetCore.Identity;

namespace Discounts.Domain.Entity
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Navigation
        public ICollection<Coupon> Coupons { get; set; } = new List<Coupon>();
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
        public Merchant? Merchant { get; set; }
    }
}
