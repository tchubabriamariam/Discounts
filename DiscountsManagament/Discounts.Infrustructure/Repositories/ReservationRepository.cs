using Discounts.Application.IRepositories;
using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using Discounts.Infrustructure.Repositories;
using Discounts.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrustructure.Repositories;

public class ReservationRepository : BaseRepository<Reservation>, IReservationRepository
{
    public ReservationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Reservation>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.UserId == userId)
            .Include(r => r.Offer)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetByOfferIdAsync(int offerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.OfferId == offerId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Reservation?> GetActiveReservationAsync(string userId, int offerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(r =>
                    r.UserId == userId &&
                    r.OfferId == offerId &&
                    r.Status == ReservationStatus.Active,
                cancellationToken);
    }

    public async Task<IEnumerable<Reservation>> GetExpiredReservationsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.Status == ReservationStatus.Active && r.ExpiresAt <= DateTime.UtcNow)
            .Include(r => r.Offer)
            .ToListAsync(cancellationToken);
    }
}