// Copyright (C) TBC Bank. All Rights Reserved.

using Microsoft.AspNetCore.Identity;

namespace Discounts.Domain.Entity
{
    public class ApplicationUser : IdentityUser
    {
        // inheriting from identityuser for built-in jwt and role-based authorization
        public string FirstName { get; set; } = string.Empty; // avoiding null references
        public string LastName { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } // i use soft delete
        public DateTime? DeletedAt { get; set; }

        // Navigation
        public ICollection<Coupon> Coupons { get; set; } = new List<Coupon>(); // one to many
        public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>(); // one to many
        public Merchant? Merchant { get; set; } // one to one, if user registeres as merchant i link it to merchant table
    }
}
