// Copyright (C) TBC Bank.All Rights Reserved.

using Discounts.Application.DTOs;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Implementations;
using Discounts.Domain.Entity;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace Discounts.Application.Tests.Auth
{
    public class AuthServiceTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly Mock<ILogger<AuthService>> _loggerMock;
        private readonly AuthService _service;

        public AuthServiceTests()
        {
            _userManagerMock = MockUserManager();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _configurationMock = new Mock<IConfiguration>();
            _loggerMock = new Mock<ILogger<AuthService>>();

            // Setting up jwt configuration
            _configurationMock.Setup(x => x["Jwt:Key"]).Returns("SuperSecretKeyForJwtTokenGenerationThatIsLongEnough123456");
            _configurationMock.Setup(x => x["Jwt:Issuer"]).Returns("DiscountsApp");
            _configurationMock.Setup(x => x["Jwt:Audience"]).Returns("DiscountsAppUsers");
            _configurationMock.Setup(x => x["Jwt:ExpiryMinutes"]).Returns("60");

            _service = new AuthService(_userManagerMock.Object, _unitOfWorkMock.Object, _configurationMock.Object, _loggerMock.Object);
        }

        #region RegisterCustomerAsync Tests

        [Fact(DisplayName = "When valid customer registration request is provided should create customer with customer role")]
        public async Task RegisterCustomerAsync_WhenValidRequest_ShouldCreateCustomerWithCustomerRole()
        {
            // Arrange
            var request = new RegisterCustomerRequestDto
            {
                Email = "customer@test.com",
                Password = "Test@123",
                ConfirmPassword = "Test@123",
                FirstName = "John",
                LastName = "Doe"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.Customer))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { Roles.Customer });

            // Act
            var result = await _service.RegisterCustomerAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.Email.Should().Be(request.Email);
            result.Role.Should().Be(Roles.Customer);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.Customer), Times.Once);
        }

        [Fact(DisplayName = "When email already exists should throw invalid operation exception")]
        public async Task RegisterCustomerAsync_WhenEmailExists_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new RegisterCustomerRequestDto { Email = "existing@test.com" };
            var existingUser = new ApplicationUser { Email = "existing@test.com" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(existingUser);

            // Act
            Func<Task> act = async () => await _service.RegisterCustomerAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");
        }

        [Fact(DisplayName = "When passwords do not match should throw invalid operation exception")]
        public async Task RegisterCustomerAsync_WhenPasswordsDoNotMatch_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new RegisterCustomerRequestDto
            {
                Email = "test@test.com",
                Password = "Test@123",
                ConfirmPassword = "Different@123"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _service.RegisterCustomerAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Passwords do not match*");
        }

        [Fact(DisplayName = "When user creation fails should throw invalid operation exception with error details")]
        public async Task RegisterCustomerAsync_WhenUserCreationFails_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new RegisterCustomerRequestDto
            {
                Email = "test@test.com",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password too weak" }));

            // Act
            Func<Task> act = async () => await _service.RegisterCustomerAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Failed to create user*");
        }

        #endregion

        #region RegisterMerchantAsync Tests

        [Fact(DisplayName = "When valid merchant registration request is provided should create merchant with merchant role and profile")]
        public async Task RegisterMerchantAsync_WhenValidRequest_ShouldCreateMerchantWithMerchantRole()
        {
            // Arrange
            var request = new RegisterMerchantRequestDto
            {
                Email = "merchant@test.com",
                Password = "Test@123",
                ConfirmPassword = "Test@123",
                FirstName = "Jane",
                LastName = "Smith",
                CompanyName = "Test Company",
                CompanyDescription = "A test company",
                PhoneNumber = "555-1234",
                Address = "123 Test St"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser?)null);
            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.Merchant))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
                .ReturnsAsync(new List<string> { Roles.Merchant });
            _unitOfWorkMock.Setup(x => x.Merchants.AddAsync(It.IsAny<Merchant>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Merchant { Id = 1 });

            // Act
            var result = await _service.RegisterMerchantAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.Email.Should().Be(request.Email);
            result.Role.Should().Be(Roles.Merchant);
            result.MerchantId.Should().Be(1);
            _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), request.Password), Times.Once);
            _userManagerMock.Verify(x => x.AddToRoleAsync(It.IsAny<ApplicationUser>(), Roles.Merchant), Times.Once);
            _unitOfWorkMock.Verify(x => x.Merchants.AddAsync(It.IsAny<Merchant>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When email already exists should throw invalid operation exception")]
        public async Task RegisterMerchantAsync_WhenEmailExists_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new RegisterMerchantRequestDto { Email = "existing@test.com" };
            var existingUser = new ApplicationUser { Email = "existing@test.com" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(existingUser);

            // Act
            Func<Task> act = async () => await _service.RegisterMerchantAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*already exists*");
        }

        [Fact(DisplayName = "When passwords do not match should throw invalid operation exception")]
        public async Task RegisterMerchantAsync_WhenPasswordsDoNotMatch_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var request = new RegisterMerchantRequestDto
            {
                Email = "test@test.com",
                Password = "Test@123",
                ConfirmPassword = "Different@123"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _service.RegisterMerchantAsync(request);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Passwords do not match*");
        }

        #endregion

        #region LoginAsync Tests

        [Fact(DisplayName = "When valid credentials are provided should return auth token with user details")]
        public async Task LoginAsync_WhenValidCredentials_ShouldReturnAuthToken()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "user@test.com",
                Password = "Test@123"
            };
            var user = new ApplicationUser
            {
                Id = "user-123",
                Email = "user@test.com",
                FirstName = "John",
                LastName = "Doe",
                IsActive = true
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Customer });

            // Act
            var result = await _service.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Token.Should().NotBeNullOrEmpty();
            result.Email.Should().Be(request.Email);
            result.Role.Should().Be(Roles.Customer);
        }

        [Fact(DisplayName = "When user does not exist should throw unauthorized access exception")]
        public async Task LoginAsync_WhenUserNotFound_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "nonexistent@test.com",
                Password = "Test@123"
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            Func<Task> act = async () => await _service.LoginAsync(request);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*Invalid email or password*");
        }

        [Fact(DisplayName = "When user is inactive should throw unauthorized access exception")]
        public async Task LoginAsync_WhenUserIsInactive_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "user@test.com",
                Password = "Test@123"
            };
            var user = new ApplicationUser
            {
                Email = "user@test.com",
                IsActive = false
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);

            // Act
            Func<Task> act = async () => await _service.LoginAsync(request);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*account has been deactivated*");
        }

        [Fact(DisplayName = "When password is incorrect should throw unauthorized access exception")]
        public async Task LoginAsync_WhenPasswordIncorrect_ShouldThrowUnauthorizedAccessException()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "user@test.com",
                Password = "WrongPassword"
            };
            var user = new ApplicationUser
            {
                Email = "user@test.com",
                IsActive = true
            };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(false);

            // Act
            Func<Task> act = async () => await _service.LoginAsync(request);

            // Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("*Invalid email or password*");
        }

        [Fact(DisplayName = "When merchant logs in should include merchant id in token")]
        public async Task LoginAsync_WhenMerchantLogsIn_ShouldIncludeMerchantIdInToken()
        {
            // Arrange
            var request = new LoginRequestDto
            {
                Email = "merchant@test.com",
                Password = "Test@123"
            };
            var user = new ApplicationUser
            {
                Id = "merchant-123",
                Email = "merchant@test.com",
                FirstName = "Jane",
                LastName = "Smith",
                IsActive = true
            };
            var merchant = new Merchant { Id = 5, UserId = "merchant-123" };

            _userManagerMock.Setup(x => x.FindByEmailAsync(request.Email))
                .ReturnsAsync(user);
            _userManagerMock.Setup(x => x.CheckPasswordAsync(user, request.Password))
                .ReturnsAsync(true);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { Roles.Merchant });
            _unitOfWorkMock.Setup(x => x.Merchants.GetByUserIdAsync(user.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(merchant);

            // Act
            var result = await _service.LoginAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.MerchantId.Should().Be(5);
            result.Role.Should().Be(Roles.Merchant);
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
