// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Domain.Entity
{
    public class GlobalSettings
    {
        // we only have one active row in this table, using pattern singleton (kind of)
        public int Id { get; set; }

        // How long a reservation is held before being auto-cancelled, this is in minutes
        public int ReservationDurationMinutes { get; set; } = 30;

        // How long a merchant can edit their offer after creation, this is in hours
        public int MerchantEditWindowHours { get; set; } = 24;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? UpdatedByAdminId { get; set; } // saves which admin changed it, string for guid

        // Navigation
        public ApplicationUser? UpdatedByAdmin { get; set; } // connection to admin who changed this
    }
}
