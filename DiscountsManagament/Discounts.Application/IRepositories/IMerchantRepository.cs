// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;

namespace Discounts.Application.IRepositories
{
    public interface IMerchantRepository : IBaseRepository<Merchant>
    {
        Task<Merchant?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<Merchant?> GetWithOffersAsync(int merchantId, CancellationToken cancellationToken = default);
    }
}
