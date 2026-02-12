using Discounts.Application.IRepositories;
using Discounts.Application.Services.Implementations;
using Discounts.Application.Services.Interfaces;
using Discounts.Infrustructure.Repositories;


namespace Discounts.API.Infrustructure.exctentions;

public static class ServiceExtensions
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IOfferRepository, OfferRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IMerchantRepository, MerchantRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IGlobalSettingsRepository, GlobalSettingsRepository>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IOfferService, OfferService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<ICouponService, CouponService>();
    }
}