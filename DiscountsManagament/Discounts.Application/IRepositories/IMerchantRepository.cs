// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;

namespace Discounts.Application.IRepositories
{
    public interface IMerchantRepository : IBaseRepository<Merchant>
    {
        Task<Merchant?> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default); // get merchant record
        Task<Merchant?> GetWithOffersAsync(int merchantId, CancellationToken cancellationToken = default); // what offers this merchant has created
    }
}
