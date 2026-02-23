// Copyright (C) TBC Bank.All Rights Reserved.

using Discounts.Application.DTOs.Offers;
using Discounts.Application.IRepositories;
using Discounts.Application.Mapping;
using Discounts.Application.Services.Implementations;
using Discounts.Domain.Entity;
using Discounts.Domain.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Discounts.Domain.Entity;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;

namespace Discounts.Application.Tests.Offers
{
    public class OfferServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ILogger<OfferService>> _loggerMock;
        private readonly OfferService _service;

        public OfferServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userManagerMock = MockUserManager();
            _loggerMock = new Mock<ILogger<OfferService>>();
            _service = new OfferService(_unitOfWorkMock.Object, _userManagerMock.Object, _loggerMock.Object);
            MapsterConfiguration.RegisterMaps(new ServiceCollection());
        }

        #region CreateOfferAsync Tests

        [Fact(DisplayName = "When valid request is provided should create offer with pending status")]
        public async Task CreateOfferAsync_WhenValidRequest_ShouldCreateOfferWithPendingStatus()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant
            {
                Id = 1,
                UserId = merchantUserId,
                IsVerified = true
            };
            var category = new Category { Id = 1, Name = "Food", IsActive = true };
            var request = new CreateOfferRequestDto
            {
                Title = "50% Off Pizza",
                Description = "Great deal!",
                CategoryId = 1,
                OriginalPrice = 100m,
                DiscountedPrice = 50m,
                TotalCoupons = 100,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30)
            };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _unitOfWorkMock.Setup(x => x.Offers.AddAsync(It.IsAny<Offer>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.CreateOfferAsync(merchantUserId, request);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(OfferStatus.Pending);
            result.RemainingCoupons.Should().Be(100);
            _unitOfWorkMock.Verify(x => x.Offers.AddAsync(It.IsAny<Offer>(), It.IsAny<CancellationToken>()),
                Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When merchant does not exist should throw invalid operation exception")]
        public async Task CreateOfferAsync_WhenMerchantNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var merchantUserId = "invalid-merchant";
            var request = new CreateOfferRequestDto();

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Merchant?)null);

            // Act
            Func<Task> act = async () => await _service.CreateOfferAsync(merchantUserId, request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Merchant profile not found.");
        }

        [Fact(DisplayName = "When category does not exist should throw invalid operation exception")]
        public async Task CreateOfferAsync_WhenCategoryNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId, IsVerified = true };
            var request = new CreateOfferRequestDto { CategoryId = 999 };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _service.CreateOfferAsync(merchantUserId, request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Category not found");
        }

        [Fact(DisplayName =
            "When discounted price is greater than or equal to original price should throw invalid operation exception")]
        public async Task
            CreateOfferAsync_WhenDiscountedPriceGreaterThanOrEqualToOriginal_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId, IsVerified = true };
            var category = new Category { Id = 1 };
            var request = new CreateOfferRequestDto
            {
                CategoryId = 1,
                OriginalPrice = 50m,
                DiscountedPrice = 100m,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(1)
            };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // Act
            Func<Task> act = async () => await _service.CreateOfferAsync(merchantUserId, request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Discounted price must be less than original price");
        }

        [Fact(DisplayName = "When end date is before or equal to start date should throw invalid operation exception")]
        public async Task CreateOfferAsync_WhenEndDateBeforeOrEqualToStartDate_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId, IsVerified = true };
            var category = new Category { Id = 1 };
            var request = new CreateOfferRequestDto
            {
                CategoryId = 1,
                OriginalPrice = 100m,
                DiscountedPrice = 50m,
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(5)
            };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // Act
            Func<Task> act = async () => await _service.CreateOfferAsync(merchantUserId, request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("End date must be after start date");
        }

        #endregion

        #region UpdateOfferAsync Tests

        [Fact(DisplayName = "When valid request is provided should update offer")]
        public async Task UpdateOfferAsync_WhenValidRequest_ShouldUpdateOffer()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId };
            var category = new Category { Id = 2, Name = "Updated Category" };
            var offer = new Offer
            {
                Id = 1,
                MerchantId = 1,
                CreatedAt = DateTime.UtcNow.AddHours(-1),
                Merchant = merchant,
                Category = new Category { Id = 1 }
            };
            var settings = new Domain.Entity.GlobalSettings { MerchantEditWindowHours = 24 };
            var request = new UpdateOfferRequestDto
            {
                Title = "Updated Title",
                CategoryId = 2,
                OriginalPrice = 200m,
                DiscountedPrice = 100m,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(10)
            };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);
            _unitOfWorkMock.Setup(x => x.GlobalSettings.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(settings);
            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(2, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.UpdateOfferAsync(merchantUserId, 1, request);

            // Assert
            result.Should().NotBeNull();
            _unitOfWorkMock.Verify(x => x.Offers.Update(It.IsAny<Offer>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When merchant does not exist should throw invalid operation exception")]
        public async Task UpdateOfferAsync_WhenMerchantNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var merchantUserId = "invalid-merchant";
            var request = new UpdateOfferRequestDto();

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Merchant?)null);

            // Act
            Func<Task> act = async () => await _service.UpdateOfferAsync(merchantUserId, 1, request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Merchant profile not found");
        }

        [Fact(DisplayName = "When offer does not exist should throw invalid operation exception")]
        public async Task UpdateOfferAsync_WhenOfferNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Offer?)null);

            // Act
            Func<Task> act = async () => await _service.UpdateOfferAsync(merchantUserId, 999, new UpdateOfferRequestDto());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Offer not found");
        }

        [Fact(DisplayName = "When merchant does not own the offer should throw unauthorized access exception")]
        public async Task UpdateOfferAsync_WhenMerchantDoesNotOwnOffer_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId };
            var offer = new Offer
            {
                Id = 1, MerchantId = 2, Merchant = new Merchant { Id = 2 }, Category = new Category()
            };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);

            // Act
            Func<Task> act = async () => await _service.UpdateOfferAsync(merchantUserId, 1, new UpdateOfferRequestDto());

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("You do not own this offer");
        }

        [Fact(DisplayName = "When edit window has expired should throw invalid operation exception")]
        public async Task UpdateOfferAsync_WhenEditWindowExpired_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId };
            var offer = new Offer
            {
                Id = 1,
                MerchantId = 1,
                CreatedAt = DateTime.UtcNow.AddHours(-25),
                Merchant = merchant,
                Category = new Category()
            };
            var settings = new Domain.Entity.GlobalSettings { MerchantEditWindowHours = 24 };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);
            _unitOfWorkMock.Setup(x => x.GlobalSettings.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(settings);

            // Act
            Func<Task> act = async () => await _service.UpdateOfferAsync(merchantUserId, 1, new UpdateOfferRequestDto());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Edit window has expired*");
        }

        #endregion

        #region ApproveOfferAsync Tests

        [Fact(DisplayName = "When pending offer is approved should set status to approved")]
        public async Task ApproveOfferAsync_WhenValidPendingOffer_ShouldApproveOffer()
        {
            // Arrange
            var adminUserId = "admin-123";
            var offerId = 1;
            var offer = new Offer
            {
                Id = offerId,
                Title = "Test Offer",
                Status = OfferStatus.Pending,
                Merchant = new Merchant { Id = 1, CompanyName = "Test" },
                Category = new Category { Id = 1, Name = "Food" }
            };

            _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(offerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.ApproveOfferAsync(adminUserId, offerId);

            // Assert
            result.Status.Should().Be(OfferStatus.Approved);
            offer.ApprovedByAdminId.Should().Be(adminUserId);
            offer.ApprovedAt.Should().NotBeNull();
            _unitOfWorkMock.Verify(x => x.Offers.Update(It.IsAny<Offer>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When offer does not exist should throw invalid operation exception")]
        public async Task ApproveOfferAsync_WhenOfferNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Offer?)null);

            // Act
            Func<Task> act = async () => await _service.ApproveOfferAsync("admin-1", 999);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Offer not found");
        }

        [Fact(DisplayName = "When offer is not pending should throw invalid operation exception")]
        public async Task ApproveOfferAsync_WhenNonPendingOffer_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var adminUserId = "admin-123";
            var offerId = 1;
            var offer = new Offer
            {
                Id = offerId, Status = OfferStatus.Approved, Merchant = new Merchant(), Category = new Category()
            };

            _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(offerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);

            // Act
            Func<Task> act = async () => await _service.ApproveOfferAsync(adminUserId, offerId);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Only pending offers can be approved");
        }

        #endregion

        #region RejectOfferAsync Tests

        [Fact(DisplayName = "When pending offer is rejected should set status to rejected with reason")]
        public async Task RejectOfferAsync_WhenValidPendingOffer_ShouldRejectOfferWithReason()
        {
            // Arrange
            var adminUserId = "admin-123";
            var offerId = 1;
            var reason = "Inappropriate content";
            var request = new RejectOfferRequestDto { Reason = reason };
            var offer = new Offer
            {
                Id = offerId, Status = OfferStatus.Pending, Merchant = new Merchant(), Category = new Category()
            };

            _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(offerId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _service.RejectOfferAsync(adminUserId, offerId, request);

            // Assert
            result.Status.Should().Be(OfferStatus.Rejected);
            result.RejectionReason.Should().Be(reason);
            _unitOfWorkMock.Verify(x => x.Offers.Update(It.IsAny<Offer>()), Times.Once);
        }

        [Fact(DisplayName = "When offer does not exist should throw invalid operation exception")]
        public async Task RejectOfferAsync_WhenOfferNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Offer?)null);

            // Act
            Func<Task> act = async () => await _service.RejectOfferAsync("admin-1", 999, new RejectOfferRequestDto());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Offer not found");
        }

        [Fact(DisplayName = "When offer is not pending should throw invalid operation exception")]
        public async Task RejectOfferAsync_WhenNonPendingOffer_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var offer = new Offer
            {
                Id = 1, Status = OfferStatus.Approved, Merchant = new Merchant(), Category = new Category()
            };

            _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);

            // Act
            Func<Task> act = async () => await _service.RejectOfferAsync("admin-1", 1, new RejectOfferRequestDto());

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Only pending offers can be rejected");
        }

        #endregion

        #region GetApprovedOffersAsync Tests

        [Fact(DisplayName = "When no filters are provided should return all approved offers")]
        public async Task GetApprovedOffersAsync_WhenNoFilters_ShouldReturnAllApprovedOffers()
        {
            // Arrange
            var offers = new List<Offer>
            {
                new()
                {
                    Id = 1,
                    CategoryId = 1,
                    DiscountedPrice = 50m,
                    Merchant = new Merchant(),
                    Category = new Category()
                },
                new()
                {
                    Id = 2,
                    CategoryId = 2,
                    DiscountedPrice = 75m,
                    Merchant = new Merchant(),
                    Category = new Category()
                }
            };

            _unitOfWorkMock.Setup(x => x.Offers.GetApprovedOffersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(offers);

            // Act
            var result = await _service.GetApprovedOffersAsync(null, null, null);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact(DisplayName = "When category filter is applied should return only offers from that category")]
        public async Task GetApprovedOffersAsync_WhenCategoryFilter_ShouldReturnFilteredOffers()
        {
            // Arrange
            var categoryId = 1;
            var offers = new List<Offer>
            {
                new()
                {
                    Id = 1,
                    CategoryId = 1,
                    DiscountedPrice = 50m,
                    Merchant = new Merchant { CompanyName = "A" },
                    Category = new Category { Name = "Food" }
                },
                new()
                {
                    Id = 2,
                    CategoryId = 2,
                    DiscountedPrice = 75m,
                    Merchant = new Merchant { CompanyName = "B" },
                    Category = new Category { Name = "Travel" }
                }
            };

            _unitOfWorkMock.Setup(x => x.Offers.GetApprovedOffersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(offers);

            // Act
            var result = await _service.GetApprovedOffersAsync(categoryId, null, null);

            // Assert
            result.Should().HaveCount(1);
            result.First().CategoryName.Should().Be("Food");
        }

        [Fact(DisplayName = "When price range filter is applied should return only offers within that range")]
        public async Task GetApprovedOffersAsync_WhenPriceRange_ShouldReturnFilteredOffers()
        {
            // Arrange
            var minPrice = 40m;
            var maxPrice = 60m;
            var offers = new List<Offer>
            {
                new()
                {
                    Id = 1,
                    CategoryId = 1,
                    DiscountedPrice = 30m,
                    Merchant = new Merchant(),
                    Category = new Category()
                },
                new()
                {
                    Id = 2,
                    CategoryId = 1,
                    DiscountedPrice = 50m,
                    Merchant = new Merchant(),
                    Category = new Category()
                },
                new()
                {
                    Id = 3,
                    CategoryId = 1,
                    DiscountedPrice = 70m,
                    Merchant = new Merchant(),
                    Category = new Category()
                }
            };

            _unitOfWorkMock.Setup(x => x.Offers.GetApprovedOffersAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(offers);

            // Act
            var result = await _service.GetApprovedOffersAsync(null, minPrice, maxPrice);

            // Assert
            result.Should().HaveCount(1);
            result.First().DiscountedPrice.Should().Be(50m);
        }

        #endregion

        #region GetMerchantOffersAsync Tests

        [Fact(DisplayName = "When valid merchant exists should return all merchant offers")]
        public async Task GetMerchantOffersAsync_WhenValidMerchant_ShouldReturnOffers()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId };
            var offers = new List<Offer>
            {
                new() { Id = 1, MerchantId = 1, Merchant = merchant, Category = new Category() },
                new() { Id = 2, MerchantId = 1, Merchant = merchant, Category = new Category() }
            };

            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            _unitOfWorkMock.Setup(x => x.Offers.GetByMerchantIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offers);

            // Act
            var result = await _service.GetMerchantOffersAsync(merchantUserId);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact(DisplayName = "When merchant does not exist should throw invalid operation exception")]
        public async Task GetMerchantOffersAsync_WhenMerchantNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Merchant?)null);

            // Act
            Func<Task> act = async () => await _service.GetMerchantOffersAsync("invalid-merchant");

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Merchant profile not found.");
        }

        #endregion

        #region GetOfferDetailsAsync Tests

        [Fact(DisplayName = "When valid offer id is provided should return offer details")]
        public async Task GetOfferDetailsAsync_WhenValidOfferId_ShouldReturnOfferDetails()
        {
            // Arrange
            var offer = new Offer
            {
                Id = 1,
                Title = "Test Offer",
                Merchant = new Merchant { CompanyName = "Test" },
                Category = new Category { Name = "Food" }
            };

            _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offer);

            // Act
            var result = await _service.GetOfferDetailsAsync(1);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("Test Offer");
        }

        [Fact(DisplayName = "When offer does not exist should throw invalid operation exception")]
        public async Task GetOfferDetailsAsync_WhenOfferNotFound_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.Offers.GetWithDetailsAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Offer?)null);

            // Act
            Func<Task> act = async () => await _service.GetOfferDetailsAsync(999);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Offer not found.");
        }

        #endregion

        #region GetPendingOffersAsync Tests

        [Fact(DisplayName = "When called should return only pending offers")]
        public async Task GetPendingOffersAsync_WhenCalled_ShouldReturnOnlyPendingOffers()
        {
            // Arrange
            var offers = new List<Offer>
            {
                new()
                {
                    Id = 1, Status = OfferStatus.Pending, Merchant = new Merchant(), Category = new Category()
                },
                new() { Id = 2, Status = OfferStatus.Pending, Merchant = new Merchant(), Category = new Category() }
            };

            _unitOfWorkMock.Setup(x => x.Offers.GetByStatusAsync(OfferStatus.Pending, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offers);

            // Act
            var result = await _service.GetPendingOffersAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(o => o.Status == OfferStatus.Pending).Should().BeTrue();
        }

        #endregion

        #region GetSalesHistoryAsync Tests

        [Fact(DisplayName = "When valid merchant exists should return sales history with customer details")]
        public async Task GetSalesHistoryAsync_WhenValidMerchant_ShouldReturnSalesHistory()
        {
            // Arrange
            var merchantUserId = "merchant-123";
            var merchant = new Merchant { Id = 1, UserId = merchantUserId };
            var user = new ApplicationUser
            {
                Id = "customer-1", FirstName = "John", LastName = "Doe", Email = "john@test.com"
            };
            var offer = new Offer { Id = 1, Title = "Test Offer", MerchantId = 1 };
            var coupons = new List<Coupon>
            {
                new()
                {
                    Id = 1,
                    OfferId = 1,
                    UserId = "customer-1",
                    Code = "ABC123",
                    PricePaid = 50m,
                    Offer = offer,
                    User = user
                }
            };

            var mockQueryable = coupons.AsQueryable().BuildMock();

            _unitOfWorkMock.Setup(x => x.Coupons.Query())
                .Returns(mockQueryable);
            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(merchantUserId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);
            // Act
            var result = await _service.GetSalesHistoryAsync(merchantUserId);

            // Assert
            result.Should().HaveCount(1);
            result.First().CustomerFullName.Should().Be("John Doe");
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
