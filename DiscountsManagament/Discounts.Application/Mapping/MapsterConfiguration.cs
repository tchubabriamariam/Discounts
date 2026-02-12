using Discounts.Application.DTOs.Categories;
using Discounts.Application.DTOs.Coupons;
using Discounts.Application.DTOs.Offers;
using Discounts.Application.DTOs.Reservations;
using Discounts.Domain.Entity;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Discounts.Application.Mapping;

public static class MapsterConfiguration
{
    public static void RegisterMaps(this IServiceCollection service)
    {
        // offers
        TypeAdapterConfig<Offer, OfferResponseDto>
            .NewConfig()
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : string.Empty)
            .Map(dest => dest.MerchantCompanyName, src => src.Merchant != null ? src.Merchant.CompanyName : string.Empty)
            .Map(dest => dest.DiscountPercentage, src => src.DiscountPercentage);
        
        // reservations
        TypeAdapterConfig<Reservation, ReservationResponseDto>
            .NewConfig()
            .Map(dest => dest.OfferTitle, src => src.Offer != null ? src.Offer.Title : string.Empty)
            .Map(dest => dest.MerchantCompanyName, src => src.Offer != null && src.Offer.Merchant != null ? src.Offer.Merchant.CompanyName : string.Empty)
            .Map(dest => dest.PricePerCoupon, src => src.Offer != null ? src.Offer.DiscountedPrice : 0)
            .Map(dest => dest.TotalPrice, src => src.Offer != null ? src.Offer.DiscountedPrice * src.Quantity : 0)
            .Map(dest => dest.Status, src => src.Status.ToString())
            .Map(dest => dest.MinutesRemaining, src => src.ExpiresAt > DateTime.UtcNow 
                ? (int)(src.ExpiresAt - DateTime.UtcNow).TotalMinutes 
                : 0)
            .Map(dest => dest.IsExpired, src => src.ExpiresAt <= DateTime.UtcNow);

        // coupons
        TypeAdapterConfig<Coupon, SalesHistoryResponseDto>
            .NewConfig()
            .Map(dest => dest.CustomerFullName, src => src.User != null ? $"{src.User.FirstName} {src.User.LastName}" : "Unknown")
            .Map(dest => dest.CustomerEmail, src => src.User != null ? src.User.Email : string.Empty)
            .Map(dest => dest.OfferTitle, src => src.Offer != null ? src.Offer.Title : string.Empty);
        
        
        TypeAdapterConfig<Coupon, CouponResponseDto>
            .NewConfig()
            .Map(dest => dest.OfferTitle, src => src.Offer != null ? src.Offer.Title : string.Empty)
            .Map(dest => dest.MerchantCompanyName, src => src.Offer != null && src.Offer.Merchant != null ? src.Offer.Merchant.CompanyName : string.Empty)
            .Map(dest => dest.CategoryName, src => src.Offer != null && src.Offer.Category != null ? src.Offer.Category.Name : string.Empty)
            .Map(dest => dest.IsExpired, src => src.ExpiresAt <= DateTime.UtcNow || src.Status == Domain.Enums.CouponStatus.Expired)
            .Map(dest => dest.DaysUntilExpiry, src => src.ExpiresAt > DateTime.UtcNow 
                ? (int)(src.ExpiresAt.Date - DateTime.UtcNow.Date).TotalDays 
                : 0);
        // category
        TypeAdapterConfig<Category, CategoryResponseDto>
            .NewConfig();
    }
}