// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;

namespace Discounts.Application.IRepositories
{
    public interface IReservationRepository : IBaseRepository<Reservation>
    {
        Task<IEnumerable<Reservation>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Reservation>> GetByOfferIdAsync(int offerId, CancellationToken cancellationToken = default);

        Task<Reservation?> GetActiveReservationAsync(string userId, int offerId,
            CancellationToken cancellationToken = default);

        Task<IEnumerable<Reservation>>
            GetExpiredReservationsAsync(CancellationToken cancellationToken = default); // used by worker
    }
}
