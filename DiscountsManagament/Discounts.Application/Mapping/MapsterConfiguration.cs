using Discounts.Application.DTOs.Offers;
using Discounts.Domain.Entity;
using Mapster;
using Microsoft.Extensions.DependencyInjection;

namespace Discounts.Application.Mapping;

public static class MapsterConfiguration
{
    public static void RegisterMaps(this IServiceCollection service)
    {
        TypeAdapterConfig<Offer, OfferResponseDto>
            .NewConfig()
            .Map(dest => dest.CategoryName, src => src.Category != null ? src.Category.Name : string.Empty)
            .Map(dest => dest.MerchantCompanyName, src => src.Merchant != null ? src.Merchant.CompanyName : string.Empty)
            .Map(dest => dest.DiscountPercentage, src => src.DiscountPercentage);
    }
}