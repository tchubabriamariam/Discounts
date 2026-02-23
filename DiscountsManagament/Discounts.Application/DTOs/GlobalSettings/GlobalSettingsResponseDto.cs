// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.DTOs.GlobalSettings
{
    public class GlobalSettingsResponseDto
    {
        // admin can see current settings
        public int Id { get; set; } // grey because singleton
        public int ReservationDurationMinutes { get; set; }
        public int MerchantEditWindowHours { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? UpdatedByAdminEmail { get; set; } // null for starting point
    }
}
