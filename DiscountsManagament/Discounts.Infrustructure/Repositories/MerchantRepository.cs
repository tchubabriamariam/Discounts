// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.IRepositories;
using Discounts.Domain.Entity;
using Discounts.Persistance.Context;
using Microsoft.EntityFrameworkCore;

namespace Discounts.Infrustructure.Repositories
{
    public class MerchantRepository : BaseRepository<Merchant>, IMerchantRepository
    {
        public MerchantRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Merchant?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default) =>
            await _dbSet
                .FirstOrDefaultAsync(m => m.UserId == userId, cancellationToken).ConfigureAwait(false);

        public async Task<Merchant?>
            GetWithOffersAsync(int merchantId, CancellationToken cancellationToken = default) =>
            await _dbSet
                .Include(m => m.Offers)
                .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken).ConfigureAwait(false);
    }
}
