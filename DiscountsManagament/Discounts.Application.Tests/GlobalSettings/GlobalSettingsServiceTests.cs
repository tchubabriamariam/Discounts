// Copyright (C) TBC Bank.All Rights Reserved.

using Discounts.Application.DTOs.GlobalSettings;
using Discounts.Application.Exceptions;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Implementations;
using Discounts.Domain.Entity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;

namespace Discounts.Application.Tests.GlobalSettings
{
    public class GlobalSettingsServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<ILogger<GlobalSettingsService>> _loggerMock;
        private readonly GlobalSettingsService _service;

        public GlobalSettingsServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _userManagerMock = MockUserManager();
            _loggerMock = new Mock<ILogger<GlobalSettingsService>>();
            _service = new GlobalSettingsService(_unitOfWorkMock.Object, _userManagerMock.Object, _loggerMock.Object);
        }

        #region GetGlobalSettingsAsync Tests

        [Fact(DisplayName = "When settings exist should return settings with admin email")]
        public async Task GetGlobalSettingsAsync_WhenSettingsExist_ShouldReturnSettings()
        {
            // Arrange
            var adminId = "admin-123";
            var admin = new ApplicationUser { Id = adminId, Email = "admin@test.com" };
            var settings = new Domain.Entity.GlobalSettings
            {
                Id = 1,
                ReservationDurationMinutes = 30,
                MerchantEditWindowHours = 24,
                UpdatedByAdminId = adminId,
                UpdatedAt = DateTime.UtcNow
            };

            _unitOfWorkMock.Setup(x => x.GlobalSettings.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(settings);
            _userManagerMock.Setup(x => x.FindByIdAsync(adminId)).ReturnsAsync(admin);

            // Act
            var result = await _service.GetGlobalSettingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.ReservationDurationMinutes.Should().Be(30);
            result.MerchantEditWindowHours.Should().Be(24);
            result.UpdatedByAdminEmail.Should().Be("admin@test.com");
        }

        [Fact(DisplayName = "When settings do not exist should throw not found exception")]
        public async Task GetGlobalSettingsAsync_WhenSettingsNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.GlobalSettings.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entity.GlobalSettings?)null);

            // Act
            Func<Task> act = async () => await _service.GetGlobalSettingsAsync();

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Global settings not found*");
        }

        [Fact(DisplayName = "When settings exist but admin id is null should return settings without admin email")]
        public async Task GetGlobalSettingsAsync_WhenNoAdminId_ShouldReturnSettingsWithoutEmail()
        {
            // Arrange
            var settings = new Domain.Entity.GlobalSettings
            {
                Id = 1,
                ReservationDurationMinutes = 30,
                MerchantEditWindowHours = 24,
                UpdatedByAdminId = null
            };

            _unitOfWorkMock.Setup(x => x.GlobalSettings.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(settings);

            // Act
            var result = await _service.GetGlobalSettingsAsync();

            // Assert
            result.Should().NotBeNull();
            result.UpdatedByAdminEmail.Should().BeNull();
        }

        #endregion

        #region UpdateGlobalSettingsAsync Tests

        [Fact(DisplayName = "When valid request is provided should update settings")]
        public async Task UpdateGlobalSettingsAsync_WhenValidRequest_ShouldUpdateSettings()
        {
            // Arrange
            var adminUserId = "admin-123";
            var admin = new ApplicationUser { Id = adminUserId, Email = "admin@test.com" };
            var settings = new Domain.Entity.GlobalSettings
            {
                Id = 1,
                ReservationDurationMinutes = 30,
                MerchantEditWindowHours = 24
            };
            var request = new UpdateGlobalSettingsRequestDto
            {
                ReservationDurationMinutes = 45,
                MerchantEditWindowHours = 48
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(adminUserId)).ReturnsAsync(admin);
            _userManagerMock.Setup(x => x.GetRolesAsync(admin))
                .ReturnsAsync(new List<string> { Roles.Admin });
            _unitOfWorkMock.Setup(x => x.GlobalSettings.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(settings);
            _unitOfWorkMock.Setup(x => x.GlobalSettings.UpdateAsync(It.IsAny<Domain.Entity.GlobalSettings>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _service.UpdateGlobalSettingsAsync(adminUserId, request);

            // Assert
            result.Should().NotBeNull();
            settings.ReservationDurationMinutes.Should().Be(45);
            settings.MerchantEditWindowHours.Should().Be(48);
            settings.UpdatedByAdminId.Should().Be(adminUserId);
            settings.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            _unitOfWorkMock.Verify(x => x.GlobalSettings.UpdateAsync(It.IsAny<Domain.Entity.GlobalSettings>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When admin does not exist should throw not found exception")]
        public async Task UpdateGlobalSettingsAsync_WhenAdminNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var adminUserId = "invalid-admin";
            var request = new UpdateGlobalSettingsRequestDto();

            _userManagerMock.Setup(x => x.FindByIdAsync(adminUserId)).ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _service.UpdateGlobalSettingsAsync(adminUserId, request);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "When user is not admin should throw forbidden exception")]
        public async Task UpdateGlobalSettingsAsync_WhenNotAdmin_ShouldThrowForbiddenException()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "user@test.com" };
            var request = new UpdateGlobalSettingsRequestDto();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Customer });

            // Act
            Func<Task> act = async () => await _service.UpdateGlobalSettingsAsync(userId, request);

            // Assert
            await act.Should().ThrowAsync<ForbiddenException>()
                .WithMessage("*Only administrators can update*");
        }

        [Fact(DisplayName = "When reservation duration is less than five minutes should throw business rule violation exception")]
        public async Task UpdateGlobalSettingsAsync_WhenReservationDurationTooLow_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var adminUserId = "admin-123";
            var admin = new ApplicationUser { Id = adminUserId, Email = "admin@test.com" };
            var request = new UpdateGlobalSettingsRequestDto
            {
                ReservationDurationMinutes = 3,
                MerchantEditWindowHours = 24
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(adminUserId)).ReturnsAsync(admin);
            _userManagerMock.Setup(x => x.GetRolesAsync(admin))
                .ReturnsAsync(new List<string> { Roles.Admin });

            // Act
            Func<Task> act = async () => await _service.UpdateGlobalSettingsAsync(adminUserId, request);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*at least 5 minutes*");
        }

        [Fact(DisplayName = "When reservation duration exceeds twenty four hours should throw business rule violation exception")]
        public async Task UpdateGlobalSettingsAsync_WhenReservationDurationTooHigh_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var adminUserId = "admin-123";
            var admin = new ApplicationUser { Id = adminUserId, Email = "admin@test.com" };
            var request = new UpdateGlobalSettingsRequestDto
            {
                ReservationDurationMinutes = 1500,
                MerchantEditWindowHours = 24
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(adminUserId)).ReturnsAsync(admin);
            _userManagerMock.Setup(x => x.GetRolesAsync(admin))
                .ReturnsAsync(new List<string> { Roles.Admin });

            // Act
            Func<Task> act = async () => await _service.UpdateGlobalSettingsAsync(adminUserId, request);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*more then 24 hours*");
        }

        [Fact(DisplayName = "When edit window is less than one hour should throw business rule violation exception")]
        public async Task UpdateGlobalSettingsAsync_WhenEditWindowTooLow_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var adminUserId = "admin-123";
            var admin = new ApplicationUser { Id = adminUserId, Email = "admin@test.com" };
            var request = new UpdateGlobalSettingsRequestDto
            {
                ReservationDurationMinutes = 30,
                MerchantEditWindowHours = 0
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(adminUserId)).ReturnsAsync(admin);
            _userManagerMock.Setup(x => x.GetRolesAsync(admin))
                .ReturnsAsync(new List<string> { Roles.Admin });

            // Act
            Func<Task> act = async () => await _service.UpdateGlobalSettingsAsync(adminUserId, request);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*at least 1 hour*");
        }

        [Fact(DisplayName = "When edit window exceeds seven days should throw business rule violation exception")]
        public async Task UpdateGlobalSettingsAsync_WhenEditWindowTooHigh_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var adminUserId = "admin-123";
            var admin = new ApplicationUser { Id = adminUserId, Email = "admin@test.com" };
            var request = new UpdateGlobalSettingsRequestDto
            {
                ReservationDurationMinutes = 30,
                MerchantEditWindowHours = 200
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(adminUserId)).ReturnsAsync(admin);
            _userManagerMock.Setup(x => x.GetRolesAsync(admin))
                .ReturnsAsync(new List<string> { Roles.Admin });

            // Act
            Func<Task> act = async () => await _service.UpdateGlobalSettingsAsync(adminUserId, request);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*more then 7 days*");
        }

        [Fact(DisplayName = "When settings do not exist should throw not found exception")]
        public async Task UpdateGlobalSettingsAsync_WhenSettingsNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var adminUserId = "admin-123";
            var admin = new ApplicationUser { Id = adminUserId, Email = "admin@test.com" };
            var request = new UpdateGlobalSettingsRequestDto
            {
                ReservationDurationMinutes = 30,
                MerchantEditWindowHours = 24
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(adminUserId)).ReturnsAsync(admin);
            _userManagerMock.Setup(x => x.GetRolesAsync(admin))
                .ReturnsAsync(new List<string> { Roles.Admin });
            _unitOfWorkMock.Setup(x => x.GlobalSettings.GetAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((Domain.Entity.GlobalSettings?)null);

            // Act
            Func<Task> act = async () => await _service.UpdateGlobalSettingsAsync(adminUserId, request);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage("*Global settings not found*");
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
