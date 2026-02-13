// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Domain.Enums
{
    public enum ReservationStatus
    {
        Active = 0,
        Completed = 1, // Converted to a purchase
        Expired = 2, // Cleaned up by worker
        Cancelled = 3
    }
}
