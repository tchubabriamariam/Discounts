using Discounts.Application.DTOs.Reservations;

namespace Discounts.Application.Services.Interfaces;

public interface IReservationService
{
    /// <summary>
    /// Creates a temporary reservation for the specified offer
    /// </summary>
    Task<ReservationResponseDto> CreateReservationAsync(string userId, CreateReservationRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Purchases/confirms a reserved offer, converting it to coupons
    /// </summary>
    Task<IEnumerable<string>> PurchaseReservationAsync(string userId, int reservationId, PurchaseReservationRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all reservations for the current user
    /// </summary>
    Task<IEnumerable<ReservationResponseDto>> GetMyReservationsAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels an active reservation manually
    /// </summary>
    Task CancelReservationAsync(string userId, int reservationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific reservation by ID
    /// </summary>
    Task<ReservationResponseDto> GetReservationByIdAsync(string userId, int reservationId, CancellationToken cancellationToken = default);
}