// Copyright (C) TBC Bank.All Rights Reserved.

using Discounts.Application.DTOs.Reservations;
using Discounts.Application.Exceptions;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Implementations;
using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Discounts.Application.Tests.Reservations
{
    public class ReservationServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<ILogger<ReservationService>> _loggerMock;
    private readonly ReservationService _sut;

    public ReservationServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userManagerMock = MockUserManager();
        _loggerMock = new Mock<ILogger<ReservationService>>();
        _sut = new ReservationService(_unitOfWorkMock.Object, _userManagerMock.Object, _loggerMock.Object);
    }

    #region CreateReservationAsync Tests

    [Fact(DisplayName = "When valid request is provided should create reservation and decrease remaining coupons")]
    public async Task CreateReservationAsync_WhenValidRequest_ShouldCreateReservationAndDecreaseRemainingCoupons()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = true };
        var offer = new Offer
        {
            Id = 1,
            Status = OfferStatus.Approved,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30),
            RemainingCoupons = 100,
            Merchant = new Merchant { CompanyName = "Test" },
            Category = new Category { Name = "Food" }
        };
        var settings = new GlobalSettings { ReservationDurationMinutes = 30 };
        var request = new CreateReservationRequestDto { OfferId = 1, Quantity = 5 };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(offer);
        _unitOfWorkMock.Setup(x => x.Reservations.GetActiveReservationAsync(userId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);
        _unitOfWorkMock.Setup(x => x.GlobalSettings.GetAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(settings);
        _unitOfWorkMock.Setup(x => x.Reservations.AddAsync(It.IsAny<Reservation>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateReservationAsync(userId, request);

        // Assert
        result.Should().NotBeNull();
        result.Quantity.Should().Be(5);
        offer.RemainingCoupons.Should().Be(95);
        _unitOfWorkMock.Verify(x => x.Offers.Update(It.IsAny<Offer>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When user does not exist should throw not found exception")]
    public async Task CreateReservationAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "invalid-user";
        var request = new CreateReservationRequestDto();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

        // Act
        Func<Task> act = async () => await _sut.CreateReservationAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = "When user is inactive should throw account inactive exception")]
    public async Task CreateReservationAsync_WhenInactiveUser_ShouldThrowAccountInactiveException()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = false };
        var request = new CreateReservationRequestDto();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _sut.CreateReservationAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<AccountInactiveException>();
    }

    [Fact(DisplayName = "When offer does not exist should throw not found exception")]
    public async Task CreateReservationAsync_WhenOfferNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = true };
        var request = new CreateReservationRequestDto { OfferId = 999 };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Offer?)null);

        // Act
        Func<Task> act = async () => await _sut.CreateReservationAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = "When offer is not approved should throw offer not available exception")]
    public async Task CreateReservationAsync_WhenOfferNotApproved_ShouldThrowOfferNotAvailableException()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = true };
        var offer = new Offer
        {
            Id = 1,
            Status = OfferStatus.Pending,
            Merchant = new Merchant(),
            Category = new Category()
        };
        var request = new CreateReservationRequestDto { OfferId = 1 };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(offer);

        // Act
        Func<Task> act = async () => await _sut.CreateReservationAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<OfferNotAvailableException>();
    }

    [Fact(DisplayName = "When offer has not started yet should throw offer not available exception")]
    public async Task CreateReservationAsync_WhenOfferNotStarted_ShouldThrowOfferNotAvailableException()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = true };
        var offer = new Offer
        {
            Id = 1,
            Status = OfferStatus.Approved,
            StartDate = DateTime.UtcNow.AddDays(5),
            EndDate = DateTime.UtcNow.AddDays(30),
            Merchant = new Merchant(),
            Category = new Category()
        };
        var request = new CreateReservationRequestDto { OfferId = 1 };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(offer);

        // Act
        Func<Task> act = async () => await _sut.CreateReservationAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<OfferNotAvailableException>();
    }

    [Fact(DisplayName = "When offer has expired should throw offer not available exception")]
    public async Task CreateReservationAsync_WhenOfferExpired_ShouldThrowOfferNotAvailableException()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = true };
        var offer = new Offer
        {
            Id = 1,
            Status = OfferStatus.Approved,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddDays(-1),
            Merchant = new Merchant(),
            Category = new Category()
        };
        var request = new CreateReservationRequestDto { OfferId = 1 };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(offer);

        // Act
        Func<Task> act = async () => await _sut.CreateReservationAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<OfferNotAvailableException>();
    }

    [Fact(DisplayName = "When insufficient coupons are available should throw insufficient coupons exception")]
    public async Task CreateReservationAsync_WhenInsufficientCoupons_ShouldThrowInsufficientCouponsException()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = true };
        var offer = new Offer
        {
            Id = 1,
            Status = OfferStatus.Approved,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30),
            RemainingCoupons = 3,
            Merchant = new Merchant(),
            Category = new Category()
        };
        var request = new CreateReservationRequestDto { OfferId = 1, Quantity = 5 };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(offer);

        // Act
        Func<Task> act = async () => await _sut.CreateReservationAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<InsufficientCouponsException>();
    }

    [Fact(DisplayName = "When user already has active reservation for this offer should throw duplicate reservation exception")]
    public async Task CreateReservationAsync_WhenDuplicateReservation_ShouldThrowDuplicateReservationException()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = true };
        var offer = new Offer
        {
            Id = 1,
            Status = OfferStatus.Approved,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(30),
            RemainingCoupons = 100,
            Merchant = new Merchant(),
            Category = new Category()
        };
        var existingReservation = new Reservation { UserId = userId, OfferId = 1 };
        var request = new CreateReservationRequestDto { OfferId = 1, Quantity = 1 };

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(offer);
        _unitOfWorkMock.Setup(x => x.Reservations.GetActiveReservationAsync(userId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingReservation);

        // Act
        Func<Task> act = async () => await _sut.CreateReservationAsync(userId, request);

        // Assert
        await act.Should().ThrowAsync<DuplicateReservationException>();
    }

    #endregion

    #region PurchaseReservationAsync Tests

    [Fact(DisplayName = "When valid reservation is purchased should generate coupons and deduct balance")]
    public async Task PurchaseReservationAsync_WhenValidReservation_ShouldGenerateCouponsAndDeductBalance()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = true, Balance = 200m };
        var reservation = new Reservation
        {
            Id = 1,
            UserId = userId,
            OfferId = 1,
            Quantity = 2,
            Status = ReservationStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        var offer = new Offer
        {
            Id = 1,
            DiscountedPrice = 50m,
            EndDate = DateTime.UtcNow.AddDays(30),
            Merchant = new Merchant(),
            Category = new Category()
        };
        var request = new PurchaseReservationRequestDto();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.Reservations.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Reservation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(offer);
        _unitOfWorkMock.Setup(x => x.Coupons.CodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _unitOfWorkMock.Setup(x => x.Coupons.AddRangeAsync(It.IsAny<List<Coupon>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.PurchaseReservationAsync(userId, 1, request);

        // Assert
        result.Should().HaveCount(2);
        result.All(code => code.Length == 12).Should().BeTrue();
        user.Balance.Should().Be(100m);
        reservation.Status.Should().Be(ReservationStatus.Completed);
        _unitOfWorkMock.Verify(x => x.Coupons.AddRangeAsync(It.Is<List<Coupon>>(c => c.Count == 2), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When user does not exist should throw not found exception")]
    public async Task PurchaseReservationAsync_WhenUserNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "invalid-user";
        var request = new PurchaseReservationRequestDto();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

        // Act
        Func<Task> act = async () => await _sut.PurchaseReservationAsync(userId, 1, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = "When user is inactive should throw account inactive exception")]
    public async Task PurchaseReservationAsync_WhenInactiveUser_ShouldThrowAccountInactiveException()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = false };
        var request = new PurchaseReservationRequestDto();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

        // Act
        Func<Task> act = async () => await _sut.PurchaseReservationAsync(userId, 1, request);

        // Assert
        await act.Should().ThrowAsync<AccountInactiveException>();
    }

    [Fact(DisplayName = "When reservation does not exist should throw not found exception")]
    public async Task PurchaseReservationAsync_WhenReservationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = true };
        var request = new PurchaseReservationRequestDto();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.Reservations.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Reservation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        // Act
        Func<Task> act = async () => await _sut.PurchaseReservationAsync(userId, 999, request);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = "When reservation is not active should throw invalid offer status exception")]
    public async Task PurchaseReservationAsync_WhenReservationNotActive_ShouldThrowInvalidOfferStatusException()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = true };
        var reservation = new Reservation
        {
            Id = 1,
            UserId = userId,
            Status = ReservationStatus.Completed
        };
        var request = new PurchaseReservationRequestDto();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.Reservations.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Reservation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        Func<Task> act = async () => await _sut.PurchaseReservationAsync(userId, 1, request);

        // Assert
        await act.Should().ThrowAsync<InvalidOfferStatusException>();
    }

    [Fact(DisplayName = "When reservation has expired should throw reservation expired exception")]
    public async Task PurchaseReservationAsync_WhenReservationExpired_ShouldThrowReservationExpiredException()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = true };
        var reservation = new Reservation
        {
            Id = 1,
            UserId = userId,
            Status = ReservationStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1)
        };
        var request = new PurchaseReservationRequestDto();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.Reservations.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Reservation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        Func<Task> act = async () => await _sut.PurchaseReservationAsync(userId, 1, request);

        // Assert
        await act.Should().ThrowAsync<ReservationExpiredException>();
    }

    [Fact(DisplayName = "When user has insufficient balance should throw insufficient balance exception")]
    public async Task PurchaseReservationAsync_WhenInsufficientBalance_ShouldThrowInsufficientBalanceException()
    {
        // Arrange
        var userId = "customer-123";
        var user = new ApplicationUser { Id = userId, IsActive = true, Balance = 50m };
        var reservation = new Reservation
        {
            Id = 1,
            UserId = userId,
            OfferId = 1,
            Quantity = 2,
            Status = ReservationStatus.Active,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };
        var offer = new Offer { Id = 1, DiscountedPrice = 60m, Merchant = new Merchant(), Category = new Category() };
        var request = new PurchaseReservationRequestDto();

        _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
        _unitOfWorkMock.Setup(x => x.Reservations.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Reservation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(offer);

        // Act
        Func<Task> act = async () => await _sut.PurchaseReservationAsync(userId, 1, request);

        // Assert
        await act.Should().ThrowAsync<InsufficientBalanceException>();
    }

    #endregion

    #region CancelReservationAsync Tests

    [Fact(DisplayName = "When valid active reservation is cancelled should restore coupons to offer")]
    public async Task CancelReservationAsync_WhenValidActiveReservation_ShouldCancelAndRestoreCoupons()
    {
        // Arrange
        var userId = "customer-123";
        var reservation = new Reservation
        {
            Id = 1,
            UserId = userId,
            OfferId = 1,
            Quantity = 5,
            Status = ReservationStatus.Active
        };
        var offer = new Offer { Id = 1, RemainingCoupons = 95 };

        _unitOfWorkMock.Setup(x => x.Reservations.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Reservation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);
        _unitOfWorkMock.Setup(x => x.Offers.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(offer);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _sut.CancelReservationAsync(userId, 1);

        // Assert
        reservation.Status.Should().Be(ReservationStatus.Cancelled);
        offer.RemainingCoupons.Should().Be(100);
        _unitOfWorkMock.Verify(x => x.Reservations.Update(It.IsAny<Reservation>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "When reservation does not exist should throw not found exception")]
    public async Task CancelReservationAsync_WhenReservationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "customer-123";

        _unitOfWorkMock.Setup(x => x.Reservations.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Reservation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Reservation?)null);

        // Act
        Func<Task> act = async () => await _sut.CancelReservationAsync(userId, 999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact(DisplayName = "When reservation is not active should throw business rule violation exception")]
    public async Task CancelReservationAsync_WhenReservationNotActive_ShouldThrowBusinessRuleViolationException()
    {
        // Arrange
        var userId = "customer-123";
        var reservation = new Reservation
        {
            Id = 1,
            UserId = userId,
            Status = ReservationStatus.Completed
        };

        _unitOfWorkMock.Setup(x => x.Reservations.FirstOrDefaultAsync(It.IsAny<System.Linq.Expressions.Expression<Func<Reservation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservation);

        // Act
        Func<Task> act = async () => await _sut.CancelReservationAsync(userId, 1);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    #endregion

    #region GetMyReservationsAsync Tests

    [Fact(DisplayName = "When user has reservations should return all reservations ordered by date")]
    public async Task GetMyReservationsAsync_WhenUserHasReservations_ShouldReturnReservations()
    {
        // Arrange
        var userId = "customer-123";
        var reservations = new List<Reservation>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                Offer = new Offer
                {
                    Title = "Offer 1",
                    Merchant = new Merchant { CompanyName = "Merchant 1" },
                    Category = new Category { Name = "Food" }
                }
            },
            new()
            {
                Id = 2,
                UserId = userId,
                Offer = new Offer
                {
                    Title = "Offer 2",
                    Merchant = new Merchant { CompanyName = "Merchant 2" },
                    Category = new Category { Name = "Travel" }
                }
            }
        };

        _unitOfWorkMock.Setup(x => x.Reservations.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);

        // Act
        var result = await _sut.GetMyReservationsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
    }

    #endregion

    #region GetReservationByIdAsync Tests

    [Fact(DisplayName = "When valid reservation id is provided should return reservation details")]
    public async Task GetReservationByIdAsync_WhenValidId_ShouldReturnReservation()
    {
        // Arrange
        var userId = "customer-123";
        var reservations = new List<Reservation>
        {
            new()
            {
                Id = 1,
                UserId = userId,
                Offer = new Offer
                {
                    Merchant = new Merchant { CompanyName = "Test" },
                    Category = new Category { Name = "Food" }
                }
            }
        };

        _unitOfWorkMock.Setup(x => x.Reservations.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);

        // Act
        var result = await _sut.GetReservationByIdAsync(userId, 1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
    }

    [Fact(DisplayName = "When reservation does not exist should throw not found exception")]
    public async Task GetReservationByIdAsync_WhenReservationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = "customer-123";
        var reservations = new List<Reservation>();

        _unitOfWorkMock.Setup(x => x.Reservations.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(reservations);

        // Act
        Func<Task> act = async () => await _sut.GetReservationByIdAsync(userId, 999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    private static Mock<UserManager<ApplicationUser>> MockUserManager()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        return new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);
    }
}
}
