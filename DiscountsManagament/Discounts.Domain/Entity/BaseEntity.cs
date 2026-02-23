// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Domain.Entity
{
    public abstract class BaseEntity
    {
        // used by category,coupon, merchant, offer, reservation
        public int Id { get; set; } //pk
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } // soft delete
        public DateTime? DeletedAt { get; set; } // when this soft delete happened
    }
}
