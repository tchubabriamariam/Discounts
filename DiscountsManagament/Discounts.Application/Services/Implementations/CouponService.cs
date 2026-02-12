using Discounts.Application.DTOs.Coupons;
using Discounts.Application.Exceptions;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Discounts.Application.Services.Implementations;

public class CouponService : ICouponService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<CouponService> _logger;

    public CouponService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<CouponService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IEnumerable<CouponResponseDto>> GetMyCouponsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        var coupons = await _unitOfWork.Coupons.GetByUserIdAsync(userId, cancellationToken);

        var result = coupons.Adapt<IEnumerable<CouponResponseDto>>();

        return result.OrderByDescending(c => c.PurchasedAt);
    }

    public async Task<CouponResponseDto> GetCouponByCodeAsync(string userId, string code, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        
        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        var coupon = await _unitOfWork.Coupons.GetByCodeAsync(code, cancellationToken);

        if (coupon is null)
        {
            throw new NotFoundException("Coupon", code);
        }

        if (coupon.UserId != userId)
        {
            throw new ForbiddenException("You do not own this coupon.");
        }

        return coupon.Adapt<CouponResponseDto>();
    }

    public async Task<CouponResponseDto> MarkCouponAsUsedAsync(string merchantUserId, MarkCouponAsUsedRequestDto request, CancellationToken cancellationToken = default)
    {
        var merchant = await _unitOfWork.Merchants.GetByUserIdAsync(merchantUserId, cancellationToken);

        if (merchant is null)
        {
            throw new NotFoundException("Merchant profile not found.");
        }

        var coupon = await _unitOfWork.Coupons.GetByCodeAsync(request.Code, cancellationToken);

        if (coupon is null)
        {
            throw new NotFoundException("Coupon", request.Code);
        }

        if (coupon.Offer.MerchantId != merchant.Id)
        {
            throw new ForbiddenException("This coupon does not belong to your offers.");
        }

        if (coupon.Status == CouponStatus.Used)
        {
            throw new BusinessRuleViolationException("This coupon has already been used.");
        }

        if (coupon.Status == CouponStatus.Expired)
        {
            throw new BusinessRuleViolationException("This coupon has expired.");
        }

        if (coupon.ExpiresAt < DateTime.UtcNow)
        {
            throw new BusinessRuleViolationException("This coupon has expired.");
        }

        coupon.Status = CouponStatus.Used;
        coupon.UsedAt = DateTime.UtcNow;

        _unitOfWork.Coupons.Update(coupon);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Coupon {CouponCode} marked as used by merchant {MerchantId}", request.Code, merchant.Id);

        return coupon.Adapt<CouponResponseDto>();
    }
}




























