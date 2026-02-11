namespace Discounts.Application.IRepositories;

public interface IUnitOfWork : IDisposable
{
    IOfferRepository Offers { get; }
    ICouponRepository Coupons { get; }
    IReservationRepository Reservations { get; }
    IMerchantRepository Merchants { get; }
    ICategoryRepository Categories { get; }
    IGlobalSettingsRepository GlobalSettings { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}