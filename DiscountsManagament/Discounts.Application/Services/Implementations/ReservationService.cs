using Discounts.Application.Exceptions;
using Discounts.Application.DTOs.Reservations;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Discounts.Application.Services.Implementations;

public class ReservationService : IReservationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<ReservationService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ReservationResponseDto> CreateReservationAsync(string userId, CreateReservationRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        if (!user.IsActive)
        {
            throw new AccountInactiveException();
        }

        var offer = await _unitOfWork.Offers.GetWithDetailsAsync(request.OfferId, cancellationToken);
        if (offer is null)
        {
            throw new NotFoundException("Offer", request.OfferId);
        }

        if (offer.Status != OfferStatus.Approved)
        {
            throw new OfferNotAvailableException($"Offer status is {offer.Status}");
        }

        if (offer.StartDate.Date > DateTime.UtcNow.Date)
        {
            throw new OfferNotAvailableException($"Offer starts on {offer.StartDate:yyyy-MM-dd}");
        }

        if (offer.EndDate.Date < DateTime.UtcNow.Date)
        {
            throw new OfferNotAvailableException("Offer has expired");
        }

        if (offer.RemainingCoupons < request.Quantity)
        {
            throw new InsufficientCouponsException(offer.RemainingCoupons, request.Quantity);
        }

        // Check if user already has an active reservation for this offer
        var existingReservation = await _unitOfWork.Reservations.GetActiveReservationAsync(userId, request.OfferId, cancellationToken);
        if (existingReservation is not null)
        {
            throw new DuplicateReservationException();
        }

        var settings = await _unitOfWork.GlobalSettings.GetAsync(cancellationToken);

        // Create the reservation
        var reservation = new Reservation
        {
            UserId = userId,
            OfferId = request.OfferId,
            Quantity = request.Quantity,
            Status = ReservationStatus.Active,
            ReservedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddMinutes(settings.ReservationDurationMinutes)
        };

        offer.RemainingCoupons -= request.Quantity;

        await _unitOfWork.Reservations.AddAsync(reservation, cancellationToken);
        _unitOfWork.Offers.Update(offer);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reservation {ReservationId} created by user {UserId} for offer {OfferId}, quantity: {Quantity}", 
            reservation.Id, userId, request.OfferId, request.Quantity);

        // Manually map since we already have the offer loaded
        var response = new ReservationResponseDto
        {
            Id = reservation.Id,
            OfferId = reservation.OfferId,
            OfferTitle = offer.Title,
            MerchantCompanyName = offer.Merchant?.CompanyName ?? string.Empty,
            PricePerCoupon = offer.DiscountedPrice,
            TotalPrice = offer.DiscountedPrice * request.Quantity,
            Quantity = reservation.Quantity,
            Status = reservation.Status.ToString(),
            ReservedAt = reservation.ReservedAt,
            ExpiresAt = reservation.ExpiresAt,
            MinutesRemaining = (int)(reservation.ExpiresAt - DateTime.UtcNow).TotalMinutes,
            IsExpired = reservation.ExpiresAt <= DateTime.UtcNow
        };

        return response;
    }

    public async Task<IEnumerable<string>> PurchaseReservationAsync(string userId, int reservationId, PurchaseReservationRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new NotFoundException("User", userId);
        }

        if (!user.IsActive)
        {
            throw new AccountInactiveException();
        }

        var reservation = await _unitOfWork.Reservations.FirstOrDefaultAsync(
            r => r.Id == reservationId && r.UserId == userId,
            cancellationToken);

        if (reservation is null)
        {
            throw new NotFoundException("Reservation", reservationId);
        }

        if (reservation.Status != ReservationStatus.Active)
        {
            throw new InvalidOfferStatusException(reservation.Status.ToString(), ReservationStatus.Active.ToString());
        }

        if (reservation.ExpiresAt <= DateTime.UtcNow)
        {
            throw new ReservationExpiredException();
        }

        var offer = await _unitOfWork.Offers.GetWithDetailsAsync(reservation.OfferId, cancellationToken);
        if (offer is null)
        {
            throw new NotFoundException("Offer", reservation.OfferId);
        }

        var totalPrice = offer.DiscountedPrice * reservation.Quantity;

        // check user balance
        if (user.Balance < totalPrice)
        {
            throw new InsufficientBalanceException(totalPrice, user.Balance);
        }

        // deduct balance
        user.Balance -= totalPrice;

        // generate coupons
        var coupons = new List<Coupon>();
        var couponCodes = new List<string>();

        for (int i = 0; i < reservation.Quantity; i++)
        {
            string couponCode;
            do
            {
                couponCode = GenerateUniqueCouponCode();
            }
            while (await _unitOfWork.Coupons.CodeExistsAsync(couponCode, cancellationToken));

            var coupon = new Coupon
            {
                UserId = userId,
                OfferId = reservation.OfferId,
                Code = couponCode,
                PricePaid = offer.DiscountedPrice,
                Status = CouponStatus.Active,
                PurchasedAt = DateTime.UtcNow,
                ExpiresAt = offer.EndDate
            };

            coupons.Add(coupon);
            couponCodes.Add(couponCode);
        }

        reservation.Status = ReservationStatus.Completed;

        // save everything
        await _unitOfWork.Coupons.AddRangeAsync(coupons, cancellationToken);
        _unitOfWork.Reservations.Update(reservation);
        await _userManager.UpdateAsync(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reservation {ReservationId} purchased by user {UserId}. Generated {Count} coupons. Total price: {TotalPrice}", 
            reservationId, userId, coupons.Count, totalPrice);

        return couponCodes;
    }

    public async Task<IEnumerable<ReservationResponseDto>> GetMyReservationsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var reservations = await _unitOfWork.Reservations.GetByUserIdAsync(userId, cancellationToken);

        return reservations
            .Adapt<IEnumerable<ReservationResponseDto>>()
            .OrderByDescending(r => r.ReservedAt);
    }

    public async Task CancelReservationAsync(string userId, int reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await _unitOfWork.Reservations.FirstOrDefaultAsync(
            r => r.Id == reservationId && r.UserId == userId,
            cancellationToken);

        if (reservation is null)
        {
            throw new NotFoundException("Reservation", reservationId);
        }

        if (reservation.Status != ReservationStatus.Active)
        {
            throw new BusinessRuleViolationException($"Cannot cancel a {reservation.Status} reservation. Only Active reservations can be cancelled.");
        }

        var offer = await _unitOfWork.Offers.GetByIdAsync(reservation.OfferId, cancellationToken);
        if (offer is not null)
        {
            // Return coupons to available pol
            offer.RemainingCoupons += reservation.Quantity;
            _unitOfWork.Offers.Update(offer);
        }

        reservation.Status = ReservationStatus.Cancelled;

        _unitOfWork.Reservations.Update(reservation);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reservation {ReservationId} cancelled by user {UserId}", reservationId, userId);
    }

    public async Task<ReservationResponseDto> GetReservationByIdAsync(string userId, int reservationId, CancellationToken cancellationToken = default)
    {
        var reservations = await _unitOfWork.Reservations.GetByUserIdAsync(userId, cancellationToken);
        
        var reservation = reservations.FirstOrDefault(r => r.Id == reservationId);

        if (reservation is null)
        {
            throw new NotFoundException("Reservation", reservationId);
        }

        return reservation.Adapt<ReservationResponseDto>();
    }


    private string GenerateUniqueCouponCode()
    {
        // unique 12-character alphanumeric code
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 12)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}