// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;

namespace Discounts.Application.IRepositories
{
    public interface IReservationRepository : IBaseRepository<Reservation>
    {
        // get all reservations by specific customer
        Task<IEnumerable<Reservation>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);

        // get all reservations made for specific offer
        Task<IEnumerable<Reservation>> GetByOfferIdAsync(int offerId, CancellationToken cancellationToken = default);

        // check if a user has active reservation for an offer
        Task<Reservation?> GetActiveReservationAsync(string userId, int offerId,
            CancellationToken cancellationToken = default);

        // status is active but reservation is expired
        Task<IEnumerable<Reservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default); // used by worker
    }
}
