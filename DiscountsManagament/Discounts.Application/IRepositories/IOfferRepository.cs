// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Domain.Entity;
using Discounts.Domain.Enums;

namespace Discounts.Application.IRepositories
{
    public interface IOfferRepository : IBaseRepository<Offer>
    {
        // return all offer belonging to specifis merchant
        Task<IEnumerable<Offer>> GetByMerchantIdAsync(int merchantId, CancellationToken cancellationToken = default);

        // filter offers by category
        Task<IEnumerable<Offer>> GetByCategoryIdAsync(int categoryId, CancellationToken cancellationToken = default);

        // filter by their lifecycle
        Task<IEnumerable<Offer>> GetByStatusAsync(OfferStatus status, CancellationToken cancellationToken = default);

        // get offers that are aproved and not expired
        Task<IEnumerable<Offer>> GetApprovedOffersAsync(CancellationToken cancellationToken = default);

        // status is aproved but offer is expired
        Task<IEnumerable<Offer>> GetExpiredOffersAsync(CancellationToken cancellationToken = default);

        // uses include to get merchant and category info
        Task<Offer?> GetWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    }
}
