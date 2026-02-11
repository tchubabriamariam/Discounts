using Discounts.Application.IRepositories;
using Discounts.Persistance.Context;
using Microsoft.Extensions.Logging;

namespace Discounts.Infrustructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UnitOfWork> _logger;

    private readonly Lazy<IOfferRepository> _offerRepository;
    private readonly Lazy<ICouponRepository> _couponRepository;
    private readonly Lazy<IReservationRepository> _reservationRepository;
    private readonly Lazy<IMerchantRepository> _merchantRepository;
    private readonly Lazy<ICategoryRepository> _categoryRepository;
    private readonly Lazy<IGlobalSettingsRepository> _globalSettingsRepository;

    public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger)
    {
        _context = context;
        _logger  = logger;

        _offerRepository       = new Lazy<IOfferRepository>(() => new OfferRepository(_context));
        _couponRepository      = new Lazy<ICouponRepository>(() => new CouponRepository(_context));
        _reservationRepository = new Lazy<IReservationRepository>(() => new ReservationRepository(_context));
        _merchantRepository    = new Lazy<IMerchantRepository>(() => new MerchantRepository(_context));
        _categoryRepository    = new Lazy<ICategoryRepository>(() => new CategoryRepository(_context));
        _globalSettingsRepository = new Lazy<IGlobalSettingsRepository>(() => new GlobalSettingsRepository(_context));
    }

    public IOfferRepository Offers
    {
        get { return _offerRepository.Value; }
    }

    public ICouponRepository Coupons
    {
        get { return _couponRepository.Value; }
    }

    public IReservationRepository Reservations
    {
        get { return _reservationRepository.Value; }
    }

    public IMerchantRepository Merchants
    {
        get { return _merchantRepository.Value; }
    }
    public ICategoryRepository Categories 
    { 
        get { return _categoryRepository.Value; }
    }
    public IGlobalSettingsRepository GlobalSettings 
    { 
        get { return _globalSettingsRepository.Value; }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during changes saving in database");
            throw;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}






