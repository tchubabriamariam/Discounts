// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.IRepositories;
using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using Discounts.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrustructure.Repositories
{
    public class OfferRepository : BaseRepository<Offer>, IOfferRepository
    {
        public OfferRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Offer>> GetByMerchantIdAsync(int merchantId,
            CancellationToken cancellationToken = default) =>
            await _dbSet
                .Where(o => o.MerchantId == merchantId)
                .Include(o => o.Category)
                .Include(o => o.Merchant)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<IEnumerable<Offer>> GetByCategoryIdAsync(int categoryId,
            CancellationToken cancellationToken = default) =>
            await _dbSet
                .Where(o => o.CategoryId == categoryId)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<IEnumerable<Offer>> GetByStatusAsync(OfferStatus status,
            CancellationToken cancellationToken = default) =>
            await _dbSet
                .Where(o => o.Status == status)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<IEnumerable<Offer>> GetApprovedOffersAsync(CancellationToken cancellationToken = default) =>
            await _dbSet
                .Where(o => o.Status == OfferStatus.Approved && o.EndDate > DateTime.UtcNow)
                .Include(o => o.Merchant)
                .Include(o => o.Category)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<IEnumerable<Offer>> GetExpiredOffersAsync(CancellationToken cancellationToken = default) =>
            await _dbSet
                .Where(o => o.Status == OfferStatus.Approved && o.EndDate <= DateTime.UtcNow)
                .ToListAsync(cancellationToken).ConfigureAwait(false);

        public async Task<Offer?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default) =>
            await _dbSet
                .Include(o => o.Merchant)
                .Include(o => o.Category)
                .FirstOrDefaultAsync(o => o.Id == id, cancellationToken).ConfigureAwait(false);
    }
}
