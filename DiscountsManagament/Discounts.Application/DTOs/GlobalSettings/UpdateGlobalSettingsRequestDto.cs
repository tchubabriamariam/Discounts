// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.DTOs.GlobalSettings
{
    public class UpdateGlobalSettingsRequestDto
    {
        public int ReservationDurationMinutes { get; set; }
        public int MerchantEditWindowHours { get; set; }
    }
}
