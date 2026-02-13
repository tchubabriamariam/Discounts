// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs.Reservations;

namespace Discounts.Application.Services.Interfaces
{
    public interface IReservationService
    {
        Task<ReservationResponseDto> CreateReservationAsync(string userId, CreateReservationRequestDto request,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<string>> PurchaseReservationAsync(string userId, int reservationId,
            PurchaseReservationRequestDto request, CancellationToken cancellationToken = default);

        Task<IEnumerable<ReservationResponseDto>> GetMyReservationsAsync(string userId,
            CancellationToken cancellationToken = default);

        Task CancelReservationAsync(string userId, int reservationId, CancellationToken cancellationToken = default);

        Task<ReservationResponseDto> GetReservationByIdAsync(string userId, int reservationId,
            CancellationToken cancellationToken = default);
    }
}
