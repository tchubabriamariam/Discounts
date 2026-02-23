// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.DTOs.GlobalSettings
{
    public class UpdateGlobalSettingsRequestDto
    {
        // admin sends this to server to update settings
        public int ReservationDurationMinutes { get; set; }
        public int MerchantEditWindowHours { get; set; }
    }
}
