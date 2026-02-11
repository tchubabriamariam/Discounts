using Discounts.Application.DTOs.Offers;

namespace Discounts.Application.Services.Interfaces;

public interface IOfferService
{
    // Merchant
    Task<OfferResponseDto> CreateOfferAsync(string merchantUserId, CreateOfferRequestDto request, CancellationToken cancellationToken = default);
    Task<OfferResponseDto> UpdateOfferAsync(string merchantUserId, int offerId, UpdateOfferRequestDto request, CancellationToken cancellationToken = default);
    Task<IEnumerable<OfferResponseDto>> GetMerchantOffersAsync(string merchantUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SalesHistoryResponseDto>> GetSalesHistoryAsync(string merchantUserId, CancellationToken cancellationToken = default);

    // Admin
    Task<IEnumerable<OfferResponseDto>> GetPendingOffersAsync(CancellationToken cancellationToken = default);
    Task<OfferResponseDto> ApproveOfferAsync(string adminUserId, int offerId, CancellationToken cancellationToken = default);
    Task<OfferResponseDto> RejectOfferAsync(string adminUserId, int offerId, RejectOfferRequestDto request, CancellationToken cancellationToken = default);

    // Customer
    Task<IEnumerable<OfferResponseDto>> GetApprovedOffersAsync(int? categoryId, decimal? minPrice, decimal? maxPrice, CancellationToken cancellationToken = default);
    Task<OfferResponseDto> GetOfferDetailsAsync(int offerId, CancellationToken cancellationToken = default);
}