// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.IRepositories;
using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using Discounts.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrustructure.Repositories
{
    public class ReservationRepository : BaseRepository<Reservation>, IReservationRepository
    {
        public ReservationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Reservation>> GetByUserIdAsync(string userId,
            CancellationToken cancellationToken = default) =>
            await _dbSet
                .Where(r => r.UserId == userId)
                .Include(r => r.Offer)
                .ThenInclude(o => o.Merchant)
                .Include(r => r.Offer)
                .ThenInclude(o => o.Category)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<IEnumerable<Reservation>> GetByOfferIdAsync(int offerId,
            CancellationToken cancellationToken = default) =>
            await _dbSet
                .Where(r => r.OfferId == offerId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<Reservation?> GetActiveReservationAsync(string userId, int offerId,
            CancellationToken cancellationToken = default) =>
            await _dbSet
                .FirstOrDefaultAsync(r =>
                        r.UserId == userId &&
                        r.OfferId == offerId &&
                        r.Status == ReservationStatus.Active,
                    cancellationToken).ConfigureAwait(false);

        public async Task<IEnumerable<Reservation>> GetExpiredReservationsAsync(
            CancellationToken cancellationToken = default) =>
            await _dbSet
                .Where(r => r.Status == ReservationStatus.Active && r.ExpiresAt <= DateTime.UtcNow)
                .Include(r => r.Offer)
                .ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
