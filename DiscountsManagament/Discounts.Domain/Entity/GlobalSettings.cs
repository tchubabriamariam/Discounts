// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Domain.Entity
{
    public class GlobalSettings
    {
        public int Id { get; set; }

        // How long a reservation is held before being auto-cancelled, this is in minutes
        public int ReservationDurationMinutes { get; set; } = 30;

        // How long a merchant can edit their offer after creation, this is in hours
        public int MerchantEditWindowHours { get; set; } = 24;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedByAdminId { get; set; }

        // Navigation
        public ApplicationUser? UpdatedByAdmin { get; set; }
    }
}
