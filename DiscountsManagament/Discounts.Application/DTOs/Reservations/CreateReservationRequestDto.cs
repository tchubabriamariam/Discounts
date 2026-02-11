namespace Discounts.Application.DTOs.Reservations;

public class CreateReservationRequestDto
{
    public int OfferId { get; set; }
    public int Quantity { get; set; } = 1;
}