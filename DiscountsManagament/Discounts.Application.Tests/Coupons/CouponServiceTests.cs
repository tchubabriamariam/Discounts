// Copyright (C) TBC Bank.All Rights Reserved.

using Discounts.Application.DTOs.Coupons;
using Discounts.Application.Exceptions;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Implementations;
using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Discounts.Application.Tests.Coupons
{
    public class CouponServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ILogger<CouponService>> _loggerMock;
        private readonly CouponService _service;

        public CouponServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userManagerMock = MockUserManager();
            _loggerMock = new Mock<ILogger<CouponService>>();
            _service = new CouponService(_unitOfWorkMock.Object, _userManagerMock.Object, _loggerMock.Object);
        }

        #region GetMyCouponsAsync Tests

        [Fact(DisplayName = "When user has coupons should return all coupons ordered by purchase date")]
        public async Task GetMyCouponsAsync_WhenUserHasCoupons_ShouldReturnCouponsOrderedByDate()
        {
            // Arrange
            var userId = "customer-123";
            var user = new ApplicationUser { Id = userId };
            var coupons = new List<Coupon>
            {
                new()
                {
                    Id = 1,
                    UserId = userId,
                    Code = "ABC123",
                    PurchasedAt = DateTime.UtcNow.AddDays(-2),
                    Offer = new Offer { Title = "Offer 1", Merchant = new Merchant(), Category = new Category() }
                },
                new()
                {
                    Id = 2,
                    UserId = userId,
                    Code = "XYZ789",
                    PurchasedAt = DateTime.UtcNow.AddDays(-1),
                    Offer = new Offer { Title = "Offer 2", Merchant = new Merchant(), Category = new Category() }
                }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.Coupons.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupons);

            // Act
            var result = await _service.GetMyCouponsAsync(userId);

            // Assert
            result.Should().HaveCount(2);
            result.First().Code.Should().Be("XYZ789");
        }

        [Fact(DisplayName = "When user does not exist should throw not found exception")]
        public async Task GetMyCouponsAsync_WhenUserNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var userId = "invalid-user";

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _service.GetMyCouponsAsync(userId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "When user has no coupons should return empty list")]
        public async Task GetMyCouponsAsync_WhenNoCoupons_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = "customer-123";
            var user = new ApplicationUser { Id = userId };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.Coupons.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Coupon>());

            // Act
            var result = await _service.GetMyCouponsAsync(userId);

            // Assert
            result.Should().BeEmpty();
        }

        #endregion

        #region GetCouponByCodeAsync Tests

        [Fact(DisplayName = "When valid coupon code is provided by owner should return coupon details")]
        public async Task GetCouponByCodeAsync_WhenValidCodeByOwner_ShouldReturnCoupon()
        {
            // Arrange
            var userId = "customer-123";
            var code = "ABC123";
            var user = new ApplicationUser { Id = userId };
            var coupon = new Coupon
            {
                Id = 1,
                UserId = userId,
                Code = code,
                Offer = new Offer { Merchant = new Merchant(), Category = new Category() }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.Coupons.GetByCodeAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon);

            // Act
            var result = await _service.GetCouponByCodeAsync(userId, code);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be(code);
        }

        [Fact(DisplayName = "When user does not exist should throw not found exception")]
        public async Task GetCouponByCodeAsync_WhenUserNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var userId = "invalid-user";
            var code = "ABC123";

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _service.GetCouponByCodeAsync(userId, code);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "When coupon does not exist should throw not found exception")]
        public async Task GetCouponByCodeAsync_WhenCouponNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var userId = "customer-123";
            var code = "INVALID";
            var user = new ApplicationUser { Id = userId };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.Coupons.GetByCodeAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Coupon?)null);

            // Act
            Func<Task> act = async () => await _service.GetCouponByCodeAsync(userId, code);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "When user does not own the coupon should throw forbidden exception")]
        public async Task GetCouponByCodeAsync_WhenUserDoesNotOwnCoupon_ShouldThrowForbiddenException()
        {
            // Arrange
            var userId = "customer-123";
            var code = "ABC123";
            var user = new ApplicationUser { Id = userId };
            var coupon = new Coupon
            {
                Id = 1,
                UserId = "different-user",
                Code = code,
                Offer = new Offer { Merchant = new Merchant(), Category = new Category() }
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _unitOfWorkMock.Setup(x => x.Coupons.GetByCodeAsync(code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon);

            // Act
            Func<Task> act = async () => await _service.GetCouponByCodeAsync(userId, code);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("*do not own this coupon*");
        }

        #endregion

        #region MarkCouponAsUsedAsync Tests

        [Fact(DisplayName = "When valid coupon is marked as used should update status and timestamp")]
        public async Task MarkCouponAsUsedAsync_WhenValidCoupon_ShouldMarkAsUsed()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId };
            var request = new MarkCouponAsUsedRequestDto { Code = "ABC123" };
            var coupon = new Coupon
            {
                Id = 1,
                Code = "ABC123",
                Status = CouponStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(10),
                Offer = new Offer
                {
                    MerchantId = 1,
                    Merchant = merchant,
                    Category = new Category()
                }
            };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Coupons.GetByCodeAsync(request.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.MarkCouponAsUsedAsync(merchantUserId, request);

            // Assert
            result.Should().NotBeNull();
            coupon.Status.Should().Be(CouponStatus.Used);
            coupon.UsedAt.Should().NotBeNull();
            _unitOfWorkMock.Verify(x => x.Coupons.Update(It.IsAny<Coupon>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When merchant does not exist should throw not found exception")]
        public async Task MarkCouponAsUsedAsync_WhenMerchantNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var merchantUserId = "invalid-merchant";
            var request = new MarkCouponAsUsedRequestDto { Code = "ABC123" };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Merchant?)null);

            // Act
            Func<Task> act = async () => await _service.MarkCouponAsUsedAsync(merchantUserId, request);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "When coupon does not exist should throw not found exception")]
        public async Task MarkCouponAsUsedAsync_WhenCouponNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId };
            var request = new MarkCouponAsUsedRequestDto { Code = "INVALID" };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Coupons.GetByCodeAsync(request.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Coupon?)null);

            // Act
            Func<Task> act = async () => await _service.MarkCouponAsUsedAsync(merchantUserId, request);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "When coupon does not belong to merchant should throw forbidden exception")]
        public async Task MarkCouponAsUsedAsync_WhenCouponDoesNotBelongToMerchant_ShouldThrowForbiddenException()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId };
            var request = new MarkCouponAsUsedRequestDto { Code = "ABC123" };
            var coupon = new Coupon
            {
                Id = 1,
                Code = "ABC123",
                Offer = new Offer
                {
                    MerchantId = 2,
                    Merchant = new Merchant { Id = 2 },
                    Category = new Category()
                }
            };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Coupons.GetByCodeAsync(request.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon);

            // Act
            Func<Task> act = async () => await _service.MarkCouponAsUsedAsync(merchantUserId, request);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("*does not belong to your offers*");
        }

        [Fact(DisplayName = "When coupon is already used should throw business rule violation exception")]
        public async Task MarkCouponAsUsedAsync_WhenCouponAlreadyUsed_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId };
            var request = new MarkCouponAsUsedRequestDto { Code = "ABC123" };
            var coupon = new Coupon
            {
                Id = 1,
                Code = "ABC123",
                Status = CouponStatus.Used,
                Offer = new Offer
                {
                    MerchantId = 1,
                    Merchant = merchant,
                    Category = new Category()
                }
            };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Coupons.GetByCodeAsync(request.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon);

            // Act
            Func<Task> act = async () => await _service.MarkCouponAsUsedAsync(merchantUserId, request);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*already been used*");
        }

        [Fact(DisplayName = "When coupon status is expired should throw business rule violation exception")]
        public async Task MarkCouponAsUsedAsync_WhenCouponStatusExpired_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId };
            var request = new MarkCouponAsUsedRequestDto { Code = "ABC123" };
            var coupon = new Coupon
            {
                Id = 1,
                Code = "ABC123",
                Status = CouponStatus.Expired,
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                Offer = new Offer
                {
                    MerchantId = 1,
                    Merchant = merchant,
                    Category = new Category()
                }
            };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Coupons.GetByCodeAsync(request.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon);

            // Act
            Func<Task> act = async () => await _service.MarkCouponAsUsedAsync(merchantUserId, request);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*expired*");
        }

        [Fact(DisplayName = "When coupon expiration date has passed should throw business rule violation exception")]
        public async Task MarkCouponAsUsedAsync_WhenCouponExpired_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId };
            var request = new MarkCouponAsUsedRequestDto { Code = "ABC123" };
            var coupon = new Coupon
            {
                Id = 1,
                Code = "ABC123",
                Status = CouponStatus.Active,
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                Offer = new Offer
                {
                    MerchantId = 1,
                    Merchant = merchant,
                    Category = new Category()
                }
            };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Coupons.GetByCodeAsync(request.Code, It.IsAny<CancellationToken>()))
                .ReturnsAsync(coupon);

            // Act
            Func<Task> act = async () => await _service.MarkCouponAsUsedAsync(merchantUserId, request);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*expired*");
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
