// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs.Reservations;
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
    public class ReservationService : IReservationService
    {
        private readonly ILogger<ReservationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            ILogger<ReservationService> logger)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<ReservationResponseDto> CreateReservationAsync(string userId,
            CreateReservationRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);

            if (user is null) throw new NotFoundException("User", userId);

            if (!user.IsActive) throw new AccountInactiveException();

            var offer = await _unitOfWork.Offers.GetWithDetailsAsync(request.OfferId, cancellationToken)
                .ConfigureAwait(false);

            if (offer is null) throw new NotFoundException("Offer", request.OfferId);

            if (offer.Status != OfferStatus.Approved)
                throw new OfferNotAvailableException($"Offer status is {offer.Status}");

            if (offer.StartDate.Date > DateTime.UtcNow.Date)
                throw new OfferNotAvailableException($"Offer starts on {offer.StartDate:yyyy-MM-dd}");

            if (offer.EndDate.Date < DateTime.UtcNow.Date) throw new OfferNotAvailableException("Offer has expired");

            if (offer.RemainingCoupons < request.Quantity)
                throw new InsufficientCouponsException(offer.RemainingCoupons, request.Quantity);

            var existingReservation = await _unitOfWork.Reservations
                .GetActiveReservationAsync(userId, request.OfferId, cancellationToken).ConfigureAwait(false);

            if (existingReservation is not null) throw new DuplicateReservationException();

            var settings = await _unitOfWork.GlobalSettings.GetAsync(cancellationToken).ConfigureAwait(false);

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

            await _unitOfWork.Reservations.AddAsync(reservation, cancellationToken).ConfigureAwait(false);
            _unitOfWork.Offers.Update(offer);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Reservation {ReservationId} created by user {UserId} for offer {OfferId}, quantity: {Quantity}",
                reservation.Id, userId, request.OfferId, request.Quantity);

            reservation.Offer = offer;

            return reservation.Adapt<ReservationResponseDto>();
        }

        public async Task<IEnumerable<string>> PurchaseReservationAsync(string userId, int reservationId,
            PurchaseReservationRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);

            if (user is null) throw new NotFoundException("User", userId);

            if (!user.IsActive) throw new AccountInactiveException();

            var reservation = await _unitOfWork.Reservations.FirstOrDefaultAsync(
                r => r.Id == reservationId && r.UserId == userId,
                cancellationToken).ConfigureAwait(false);

            if (reservation is null) throw new NotFoundException("Reservation", reservationId);

            if (reservation.Status != ReservationStatus.Active)
                throw new InvalidOfferStatusException(reservation.Status.ToString(),
                    ReservationStatus.Active.ToString());

            if (reservation.ExpiresAt <= DateTime.UtcNow) throw new ReservationExpiredException();

            var offer = await _unitOfWork.Offers.GetWithDetailsAsync(reservation.OfferId, cancellationToken)
                .ConfigureAwait(false);

            if (offer is null) throw new NotFoundException("Offer", reservation.OfferId);

            var totalPrice = offer.DiscountedPrice * reservation.Quantity;

            if (user.Balance < totalPrice) throw new InsufficientBalanceException(totalPrice, user.Balance);

            user.Balance -= totalPrice;

            var coupons = new List<Coupon>();
            var couponCodes = new List<string>();

            for (var i = 0; i < reservation.Quantity; i++)
            {
                string couponCode;

                do
                    couponCode = GenerateUniqueCouponCode();
                while (await _unitOfWork.Coupons.CodeExistsAsync(couponCode, cancellationToken).ConfigureAwait(false));

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

            await _unitOfWork.Coupons.AddRangeAsync(coupons, cancellationToken).ConfigureAwait(false);
            _unitOfWork.Reservations.Update(reservation);
            await _userManager.UpdateAsync(user).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation(
                "Reservation {ReservationId} purchased by user {UserId}. Generated {Count} coupons. Total price: {TotalPrice}",
                reservationId, userId, coupons.Count, totalPrice);

            return couponCodes;
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetMyReservationsAsync(string userId,
            CancellationToken cancellationToken = default)
        {
            var reservations = await _unitOfWork.Reservations.GetByUserIdAsync(userId, cancellationToken)
                .ConfigureAwait(false);

            var result = reservations.Adapt<IEnumerable<ReservationResponseDto>>();

            return result.OrderByDescending(r => r.ReservedAt);
        }

        public async Task CancelReservationAsync(string userId, int reservationId,
            CancellationToken cancellationToken = default)
        {
            var reservation = await _unitOfWork.Reservations.FirstOrDefaultAsync(
                r => r.Id == reservationId && r.UserId == userId,
                cancellationToken).ConfigureAwait(false);

            if (reservation is null) throw new NotFoundException("Reservation", reservationId);

            if (reservation.Status != ReservationStatus.Active)
                throw new BusinessRuleViolationException(
                    $"Cannot cancel a {reservation.Status} reservation. Only Active reservations can be cancelled");

            var offer = await _unitOfWork.Offers.GetByIdAsync(reservation.OfferId, cancellationToken)
                .ConfigureAwait(false);

            if (offer is not null)
            {
                // someone else can use this coupons
                offer.RemainingCoupons += reservation.Quantity;
                _unitOfWork.Offers.Update(offer);
            }

            reservation.Status = ReservationStatus.Cancelled;

            _unitOfWork.Reservations.Update(reservation);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Reservation {ReservationId} cancelled by user {UserId}", reservationId, userId);
        }

        public async Task<ReservationResponseDto> GetReservationByIdAsync(string userId, int reservationId,
            CancellationToken cancellationToken = default)
        {
            var reservations = await _unitOfWork.Reservations.GetByUserIdAsync(userId, cancellationToken)
                .ConfigureAwait(false);

            var reservation = reservations.FirstOrDefault(r => r.Id == reservationId);

            if (reservation is null) throw new NotFoundException("Reservation", reservationId);

            return reservation.Adapt<ReservationResponseDto>();
        }

        private static string GenerateUniqueCouponCode()
        {
            // creates 12 character long alphanumeric code
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }
    }
}
