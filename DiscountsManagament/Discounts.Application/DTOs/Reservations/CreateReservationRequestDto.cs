// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.DTOs.Reservations
{
    public class CreateReservationRequestDto
    {
        // when customer creates a reservation
        public int OfferId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
