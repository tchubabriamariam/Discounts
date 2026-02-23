// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.IRepositories
{
    public interface IUnitOfWork : IDisposable
    {
        // grouping so service only injects one thing
        IOfferRepository Offers { get; }
        ICouponRepository Coupons { get; }
        IReservationRepository Reservations { get; }
        IMerchantRepository Merchants { get; }
        ICategoryRepository Categories { get; }
        IGlobalSettingsRepository GlobalSettings { get; }

        // database transaction, atomicity all happens together or nothing at all
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
