// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs.Offers;
using Discounts.Application.Exceptions;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Discounts.Application.Services.Implementations
{
    public class OfferService : IOfferService
    {
        private readonly ILogger<OfferService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public OfferService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<OfferService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<OfferResponseDto> CreateOfferAsync(string merchantUserId, CreateOfferRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var merchant = await _unitOfWork.Merchants.GetByUserIdAsync(merchantUserId, cancellationToken)
                .ConfigureAwait(false);

            if (merchant is null) throw new InvalidOperationException("Merchant profile not found.");

            if (!merchant.IsVerified)
            {
                throw new ForbiddenException(
                    "Your merchant account is pending verification. " +
                    "Please wait for admin approval before creating offers.");
            }
            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken)
                .ConfigureAwait(false);

            if (category is null) throw new InvalidOperationException("Category not found.");

            if (request.DiscountedPrice >= request.OriginalPrice)
                throw new InvalidOperationException("Discounted price must be less than original price.");

            if (request.EndDate <= request.StartDate)
                throw new InvalidOperationException("End date must be after start date.");

            var offer = request.Adapt<Offer>();
            offer.MerchantId = merchant.Id;
            offer.RemainingCoupons = request.TotalCoupons;
            offer.Status = OfferStatus.Pending;

            offer.Merchant = merchant;
            offer.Category = category;

            await _unitOfWork.Offers.AddAsync(offer, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Offer created by merchant {MerchantId}: {OfferTitle}", merchant.Id, offer.Title);

            return offer.Adapt<OfferResponseDto>();
        }

        public async Task<OfferResponseDto> UpdateOfferAsync(string merchantUserId, int offerId,
            UpdateOfferRequestDto request, CancellationToken cancellationToken = default)
        {
            var merchant = await _unitOfWork.Merchants.GetByUserIdAsync(merchantUserId, cancellationToken)
                .ConfigureAwait(false);

            if (merchant is null) throw new InvalidOperationException("Merchant profile not found.");

            var offer = await _unitOfWork.Offers.GetWithDetailsAsync(offerId, cancellationToken).ConfigureAwait(false);

            if (offer is null) throw new InvalidOperationException("Offer not found.");

            if (offer.MerchantId != merchant.Id) throw new UnauthorizedAccessException("You do not own this offer.");

            var settings = await _unitOfWork.GlobalSettings.GetAsync(cancellationToken).ConfigureAwait(false);
            var editDeadline = offer.CreatedAt.AddHours(settings.MerchantEditWindowHours);

            if (DateTime.UtcNow > editDeadline)
                throw new InvalidOperationException(
                    $"Edit window has expired. Offers can only be edited within {settings.MerchantEditWindowHours} hours of creation.");

            var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId, cancellationToken)
                .ConfigureAwait(false);

            if (category is null) throw new InvalidOperationException("Category not found.");

            if (request.DiscountedPrice >= request.OriginalPrice)
                throw new InvalidOperationException("Discounted price must be less than original price.");

            if (request.EndDate <= request.StartDate)
                throw new InvalidOperationException("End date must be after start date.");

            request.Adapt(offer);
            offer.Category = category;

            _unitOfWork.Offers.Update(offer);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Offer {OfferId} updated by merchant {MerchantId}", offerId, merchant.Id);

            return offer.Adapt<OfferResponseDto>();
        }

        public async Task<IEnumerable<OfferResponseDto>> GetMerchantOffersAsync(string merchantUserId,
            CancellationToken cancellationToken = default)
        {
            var merchant = await _unitOfWork.Merchants.GetByUserIdAsync(merchantUserId, cancellationToken)
                .ConfigureAwait(false);

            if (merchant is null) throw new InvalidOperationException("Merchant profile not found.");

            var offers = await _unitOfWork.Offers.GetByMerchantIdAsync(merchant.Id, cancellationToken)
                .ConfigureAwait(false);

            return offers.Adapt<IEnumerable<OfferResponseDto>>();
        }

        public async Task<IEnumerable<SalesHistoryResponseDto>> GetSalesHistoryAsync(string merchantUserId,
            CancellationToken cancellationToken = default)
        {
            var merchant = await _unitOfWork.Merchants.GetByUserIdAsync(merchantUserId, cancellationToken)
                .ConfigureAwait(false);

            if (merchant is null) throw new InvalidOperationException("Merchant profile not found.");

            var offers = await _unitOfWork.Offers.GetByMerchantIdAsync(merchant.Id, cancellationToken)
                .ConfigureAwait(false);
            var result = new List<SalesHistoryResponseDto>();

            foreach (var offer in offers)
            {
                var coupons = await _unitOfWork.Coupons.GetByOfferIdAsync(offer.Id, cancellationToken)
                    .ConfigureAwait(false);

                foreach (var coupon in coupons)
                {
                    var user = await _userManager.FindByIdAsync(coupon.UserId).ConfigureAwait(false);

                    var salesEntry = coupon.Adapt<SalesHistoryResponseDto>();
                    salesEntry.CustomerFullName = user is null ? "Unknown" : $"{user.FirstName} {user.LastName}";
                    salesEntry.CustomerEmail = user?.Email ?? string.Empty;
                    salesEntry.OfferTitle = offer.Title;

                    result.Add(salesEntry);
                }
            }

            return result;
        }

        // admin part
        public async Task<IEnumerable<OfferResponseDto>> GetPendingOffersAsync(
            CancellationToken cancellationToken = default)
        {
            var offers = await _unitOfWork.Offers.GetByStatusAsync(OfferStatus.Pending, cancellationToken)
                .ConfigureAwait(false);

            return offers.Adapt<IEnumerable<OfferResponseDto>>();
        }

        public async Task<OfferResponseDto> ApproveOfferAsync(string adminUserId, int offerId,
            CancellationToken cancellationToken = default)
        {
            var offer = await _unitOfWork.Offers.GetWithDetailsAsync(offerId, cancellationToken).ConfigureAwait(false);

            if (offer is null) throw new InvalidOperationException("Offer not found.");

            if (offer.Status != OfferStatus.Pending)
                throw new InvalidOperationException("Only pending offers can be approved.");

            offer.Status = OfferStatus.Approved;
            offer.ApprovedAt = DateTime.UtcNow;
            offer.ApprovedByAdminId = adminUserId;

            _unitOfWork.Offers.Update(offer);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Offer {OfferId} approved by admin {AdminId}", offerId, adminUserId);

            return offer.Adapt<OfferResponseDto>();
        }

        public async Task<OfferResponseDto> RejectOfferAsync(string adminUserId, int offerId,
            RejectOfferRequestDto request, CancellationToken cancellationToken = default)
        {
            var offer = await _unitOfWork.Offers.GetWithDetailsAsync(offerId, cancellationToken).ConfigureAwait(false);

            if (offer is null) throw new InvalidOperationException("Offer not found.");

            if (offer.Status != OfferStatus.Pending)
                throw new InvalidOperationException("Only pending offers can be rejected.");

            offer.Status = OfferStatus.Rejected;
            offer.RejectionReason = request.Reason;

            _unitOfWork.Offers.Update(offer);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Offer {OfferId} rejected by admin {AdminId}", offerId, adminUserId);

            return offer.Adapt<OfferResponseDto>();
        }

        // customer part

        public async Task<IEnumerable<OfferResponseDto>> GetApprovedOffersAsync(int? categoryId, decimal? minPrice,
            decimal? maxPrice, CancellationToken cancellationToken = default)
        {
            var offers = await _unitOfWork.Offers.GetApprovedOffersAsync(cancellationToken).ConfigureAwait(false);

            if (categoryId.HasValue) offers = offers.Where(o => o.CategoryId == categoryId.Value);

            if (minPrice.HasValue) offers = offers.Where(o => o.DiscountedPrice >= minPrice.Value);

            if (maxPrice.HasValue) offers = offers.Where(o => o.DiscountedPrice <= maxPrice.Value);

            return offers.Adapt<IEnumerable<OfferResponseDto>>();
        }

        public async Task<OfferResponseDto> GetOfferDetailsAsync(int offerId,
            CancellationToken cancellationToken = default)
        {
            var offer = await _unitOfWork.Offers.GetWithDetailsAsync(offerId, cancellationToken).ConfigureAwait(false);

            if (offer is null) throw new InvalidOperationException("Offer not found.");

            return offer.Adapt<OfferResponseDto>();
        }
    }
}
