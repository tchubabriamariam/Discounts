// Copyright (C) TBC Bank.All Rights Reserved.

using Discounts.Application.DTOs.Admin;
using Discounts.Application.Exceptions;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Implementations;
using Discounts.Domain.Entity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using MockQueryable.Moq;

namespace Discounts.Application.Tests.Admin
{
   public class AdminServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<AdminService>> _loggerMock;
        private readonly AdminService _service;

        public AdminServiceTests()
        {
            _userManagerMock = MockUserManager();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<AdminService>>();
            _service = new AdminService(_userManagerMock.Object, _unitOfWorkMock.Object, _loggerMock.Object);
        }

        #region GetAllUsersAsync Tests

        [Fact(DisplayName = "When no role filter is provided should return all users")]
        public async Task GetAllUsersAsync_WhenNoFilter_ShouldReturnAllUsers()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new() { Id = "1", Email = "admin@test.com" },
                new() { Id = "2", Email = "merchant@test.com" },
                new() { Id = "3", Email = "customer@test.com" }
            };

            var mockQueryable = users.AsQueryable().BuildMock();
            _userManagerMock.Setup(x => x.Users).Returns(mockQueryable);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { Roles.Customer });

            // Act
            var result = await _service.GetAllUsersAsync(null);

            // Assert
            result.Should().HaveCount(3);
        }

        [Fact(DisplayName = "When role filter is provided should return only users with that role")]
        public async Task GetAllUsersAsync_WhenRoleFilter_ShouldReturnFilteredUsers()
        {
            // Arrange
            var users = new List<ApplicationUser>
            {
                new() { Id = "1", Email = "admin@test.com" },
                new() { Id = "2", Email = "merchant@test.com" }
            };

            var mockQueryable = users.AsQueryable().BuildMock();
            _userManagerMock.Setup(x => x.Users).Returns(mockQueryable);
            _userManagerMock.SetupSequence(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { Roles.Admin })
                .ReturnsAsync(new List<string> { Roles.Merchant });

            // Act
            var result = await _service.GetAllUsersAsync(Roles.Admin);

            // Assert
            result.Should().HaveCount(1);
            result.First().Email.Should().Be("admin@test.com");
        }

        #endregion

        #region GetUserByIdAsync Tests

        [Fact(DisplayName = "When valid user id is provided should return user details")]
        public async Task GetUserByIdAsync_WhenValidId_ShouldReturnUser()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "user@test.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Customer });

            // Act
            var result = await _service.GetUserByIdAsync(userId);

            // Assert
            result.Should().NotBeNull();
            result.Email.Should().Be("user@test.com");
        }

        [Fact(DisplayName = "When user does not exist should throw not found exception")]
        public async Task GetUserByIdAsync_WhenUserNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var userId = "invalid-user";

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _service.GetUserByIdAsync(userId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        #endregion

        #region UpdateUserAsync Tests

        [Fact(DisplayName = "When valid update request is provided should update user details")]
        public async Task UpdateUserAsync_WhenValidRequest_ShouldUpdateUser()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "old@test.com", Balance = 100m };
            var request = new UpdateUserRequestDto
            {
                FirstName = "Updated",
                LastName = "Name",
                Email = "old@test.com",
                Balance = 200m
            };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Customer });

            // Act
            var result = await _service.UpdateUserAsync(userId, request);

            // Assert
            result.Should().NotBeNull();
            user.Balance.Should().Be(200m);
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
        }

        [Fact(DisplayName = "When user does not exist should throw not found exception")]
        public async Task UpdateUserAsync_WhenUserNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var userId = "invalid-user";
            var request = new UpdateUserRequestDto();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _service.UpdateUserAsync(userId, request);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "When new email already exists should throw already exists exception")]
        public async Task UpdateUserAsync_WhenEmailExists_ShouldThrowAlreadyExistsException()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "old@test.com" };
            var existingUser = new ApplicationUser { Email = "existing@test.com" };
            var request = new UpdateUserRequestDto { Email = "existing@test.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.FindByEmailAsync("existing@test.com")).ReturnsAsync(existingUser);

            // Act
            Func<Task> act = async () => await _service.UpdateUserAsync(userId, request);

            // Assert
            await act.Should().ThrowAsync<AlreadyExistsException>();
        }

        [Fact(DisplayName = "When update fails should throw business rule violation exception")]
        public async Task UpdateUserAsync_WhenUpdateFails_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "user@test.com" };
            var request = new UpdateUserRequestDto { Email = "user@test.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

            // Act
            Func<Task> act = async () => await _service.UpdateUserAsync(userId, request);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>();
        }

        #endregion

        #region BlockUserAsync Tests

        [Fact(DisplayName = "When active user is blocked should set is active to false")]
        public async Task BlockUserAsync_WhenActiveUser_ShouldBlockUser()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "user@test.com", IsActive = true };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Customer });
            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.BlockUserAsync(userId);

            // Assert
            result.Should().NotBeNull();
            user.IsActive.Should().BeFalse();
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
        }

        [Fact(DisplayName = "When user does not exist should throw not found exception")]
        public async Task BlockUserAsync_WhenUserNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var userId = "invalid-user";

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _service.BlockUserAsync(userId);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "When user is admin should throw business rule violation exception")]
        public async Task BlockUserAsync_WhenUserIsAdmin_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var userId = "admin-123";
            var user = new ApplicationUser { Id = userId, Email = "admin@test.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Admin });

            // Act
            Func<Task> act = async () => await _service.BlockUserAsync(userId);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*Cannot block an admin user*");
        }

        [Fact(DisplayName = "When user is already blocked should throw business rule violation exception")]
        public async Task BlockUserAsync_WhenAlreadyBlocked_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "user@test.com", IsActive = false };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Customer });

            // Act
            Func<Task> act = async () => await _service.BlockUserAsync(userId);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*already blocked*");
        }

        #endregion

        #region UnblockUserAsync Tests

        [Fact(DisplayName = "When blocked user is unblocked should set is active to true")]
        public async Task UnblockUserAsync_WhenBlockedUser_ShouldUnblockUser()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "user@test.com", IsActive = false };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Customer });

            // Act
            var result = await _service.UnblockUserAsync(userId);

            // Assert
            result.Should().NotBeNull();
            user.IsActive.Should().BeTrue();
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
        }

        [Fact(DisplayName = "When user is already active should throw business rule violation exception")]
        public async Task UnblockUserAsync_WhenAlreadyActive_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "user@test.com", IsActive = true };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);

            // Act
            Func<Task> act = async () => await _service.UnblockUserAsync(userId);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*already active*");
        }

        #endregion

        #region MakeAdminAsync Tests

        [Fact(DisplayName = "When user is promoted to admin should add admin role")]
        public async Task MakeAdminAsync_WhenValidUser_ShouldAddAdminRole()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "user@test.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.SetupSequence(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Customer })
                .ReturnsAsync(new List<string> { Roles.Customer, Roles.Admin });
            _userManagerMock.Setup(x => x.AddToRoleAsync(user, Roles.Admin))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.MakeAdminAsync(userId);

            // Assert
            result.Should().NotBeNull();
            _userManagerMock.Verify(x => x.AddToRoleAsync(user, Roles.Admin), Times.Once);
        }

        [Fact(DisplayName = "When user is already admin should throw business rule violation exception")]
        public async Task MakeAdminAsync_WhenAlreadyAdmin_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var userId = "admin-123";
            var user = new ApplicationUser { Id = userId, Email = "admin@test.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Admin });

            // Act
            Func<Task> act = async () => await _service.MakeAdminAsync(userId);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*already an admin*");
        }

        #endregion

        #region RemoveAdminAsync Tests

        [Fact(DisplayName = "When admin role is removed should remove admin from roles")]
        public async Task RemoveAdminAsync_WhenValidAdmin_ShouldRemoveAdminRole()
        {
            // Arrange
            var userId = "admin-123";
            var user = new ApplicationUser { Id = userId, Email = "admin@test.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.SetupSequence(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Admin })
                .ReturnsAsync(new List<string> { Roles.Customer });
            _userManagerMock.Setup(x => x.RemoveFromRoleAsync(user, Roles.Admin))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _service.RemoveAdminAsync(userId);

            // Assert
            result.Should().NotBeNull();
            _userManagerMock.Verify(x => x.RemoveFromRoleAsync(user, Roles.Admin), Times.Once);
        }

        [Fact(DisplayName = "When user is not admin should throw business rule violation exception")]
        public async Task RemoveAdminAsync_WhenNotAdmin_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "user@test.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Customer });

            // Act
            Func<Task> act = async () => await _service.RemoveAdminAsync(userId);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*not an admin*");
        }

        #endregion

        #region DeleteUserAsync Tests

        [Fact(DisplayName = "When user is deleted should soft delete and deactivate")]
        public async Task DeleteUserAsync_WhenValidUser_ShouldSoftDelete()
        {
            // Arrange
            var userId = "user-123";
            var user = new ApplicationUser { Id = userId, Email = "user@test.com", IsActive = true };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Customer });
            _userManagerMock.Setup(x => x.UpdateAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            await _service.DeleteUserAsync(userId);

            // Assert
            user.IsDeleted.Should().BeTrue();
            user.DeletedAt.Should().NotBeNull();
            user.IsActive.Should().BeFalse();
            _userManagerMock.Verify(x => x.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
        }

        [Fact(DisplayName = "When admin user is deleted should throw business rule violation exception")]
        public async Task DeleteUserAsync_WhenAdminUser_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var userId = "admin-123";
            var user = new ApplicationUser { Id = userId, Email = "admin@test.com" };

            _userManagerMock.Setup(x => x.FindByIdAsync(userId)).ReturnsAsync(user);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Admin });

            // Act
            Func<Task> act = async () => await _service.DeleteUserAsync(userId);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*Cannot delete an admin user*");
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
