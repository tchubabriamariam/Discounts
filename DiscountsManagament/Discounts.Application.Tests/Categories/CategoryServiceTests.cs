// Copyright (C) TBC Bank.All Rights Reserved.

using Discounts.Application.DTOs.Categories;
using Discounts.Application.Exceptions;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Implementations;
using Discounts.Domain.Entity;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Discounts.Application.Tests.Categories
{
    public class CategoryServiceTests
    {
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly Mock<ILogger<CategoryService>> _loggerMock;
        private readonly CategoryService _service;

        public CategoryServiceTests()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _loggerMock = new Mock<ILogger<CategoryService>>();
            _service = new CategoryService(_unitOfWorkMock.Object, _loggerMock.Object);
        }

        #region CreateCategoryAsync Tests

        [Fact(DisplayName = "When valid request is provided should create category with active status")]
        public async Task CreateCategoryAsync_WhenValidRequest_ShouldCreateCategoryWithActiveStatus()
        {
            // Arrange
            var request = new CreateCategoryRequestDto
            {
                Name = "Food",
                Description = "Food and dining",
                IconUrl = "https://example.com/icon.png"
            };

            _unitOfWorkMock.Setup(x => x.Categories.NameExistsAsync("Food", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _unitOfWorkMock.Setup(x => x.Categories.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _unitOfWorkMock.Setup(x => x.Offers.GetByCategoryIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Offer>());

            // Act
            var result = await _service.CreateCategoryAsync(request);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Food");
            result.IsActive.Should().BeTrue();
            _unitOfWorkMock.Verify(x => x.Categories.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When category name already exists should throw already exists exception")]
        public async Task CreateCategoryAsync_WhenNameExists_ShouldThrowAlreadyExistsException()
        {
            // Arrange
            var request = new CreateCategoryRequestDto { Name = "Food" };

            _unitOfWorkMock.Setup(x => x.Categories.NameExistsAsync("Food", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _service.CreateCategoryAsync(request);

            // Assert
            await act.Should().ThrowAsync<AlreadyExistsException>()
                .WithMessage("*Name*Food*");
        }

        #endregion

        #region UpdateCategoryAsync Tests

        [Fact(DisplayName = "When valid request is provided should update category")]
        public async Task UpdateCategoryAsync_WhenValidRequest_ShouldUpdateCategory()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = 1, Name = "Food", IsActive = true };
            var request = new UpdateCategoryRequestDto
            {
                Name = "Updated Food",
                Description = "Updated description"
            };

            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _unitOfWorkMock.Setup(x => x.Categories.NameExistsAsync("Updated Food", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _unitOfWorkMock.Setup(x => x.Offers.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Offer>());

            // Act
            var result = await _service.UpdateCategoryAsync(categoryId, request);

            // Assert
            result.Should().NotBeNull();
            _unitOfWorkMock.Verify(x => x.Categories.Update(It.IsAny<Category>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When category does not exist should throw not found exception")]
        public async Task UpdateCategoryAsync_WhenCategoryNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            var request = new UpdateCategoryRequestDto();

            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _service.UpdateCategoryAsync(999, request);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "When new name already exists should throw already exists exception")]
        public async Task UpdateCategoryAsync_WhenNewNameExists_ShouldThrowAlreadyExistsException()
        {
            // Arrange
            var category = new Category { Id = 1, Name = "Food" };
            var request = new UpdateCategoryRequestDto { Name = "Travel" };

            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _unitOfWorkMock.Setup(x => x.Categories.NameExistsAsync("Travel", category.Id,It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = async () => await _service.UpdateCategoryAsync(1, request);

            // Assert
            await act.Should().ThrowAsync<AlreadyExistsException>();
        }

        #endregion

        #region DeleteCategoryAsync Tests

        [Fact(DisplayName = "When category has no offers should soft delete category")]
        public async Task DeleteCategoryAsync_WhenNoOffers_ShouldSoftDeleteCategory()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = 1, Name = "Food" };

            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _unitOfWorkMock.Setup(x => x.Offers.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Offer>());
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _service.DeleteCategoryAsync(categoryId);

            // Assert
            _unitOfWorkMock.Verify(x => x.Categories.SoftDelete(It.IsAny<Category>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When category does not exist should throw not found exception")]
        public async Task DeleteCategoryAsync_WhenCategoryNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _service.DeleteCategoryAsync(999);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "When category has associated offers should throw business rule violation exception")]
        public async Task DeleteCategoryAsync_WhenCategoryHasOffers_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = 1, Name = "Food" };
            var offers = new List<Offer>
            {
                new() { Id = 1, CategoryId = 1 },
                new() { Id = 2, CategoryId = 1 }
            };

            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _unitOfWorkMock.Setup(x => x.Offers.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(offers);

            // Act
            Func<Task> act = async () => await _service.DeleteCategoryAsync(categoryId);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*associated offers*");
        }

        #endregion

        #region ActivateCategoryAsync Tests

        [Fact(DisplayName = "When inactive category is activated should set status to active")]
        public async Task ActivateCategoryAsync_WhenInactiveCategory_ShouldActivateCategory()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = 1, Name = "Food", IsActive = false };

            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _unitOfWorkMock.Setup(x => x.Offers.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Offer>());

            // Act
            var result = await _service.ActivateCategoryAsync(categoryId);

            // Assert
            result.IsActive.Should().BeTrue();
            _unitOfWorkMock.Verify(x => x.Categories.Update(It.IsAny<Category>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When category does not exist should throw not found exception")]
        public async Task ActivateCategoryAsync_WhenCategoryNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _service.ActivateCategoryAsync(999);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "When category is already active should throw business rule violation exception")]
        public async Task ActivateCategoryAsync_WhenAlreadyActive_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = 1, Name = "Food", IsActive = true };

            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // Act
            Func<Task> act = async () => await _service.ActivateCategoryAsync(categoryId);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*already active*");
        }

        #endregion

        #region DeactivateCategoryAsync Tests

        [Fact(DisplayName = "When active category is deactivated should set status to inactive")]
        public async Task DeactivateCategoryAsync_WhenActiveCategory_ShouldDeactivateCategory()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = 1, Name = "Food", IsActive = true };

            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);
            _unitOfWorkMock.Setup(x => x.Offers.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Offer>());

            // Act
            var result = await _service.DeactivateCategoryAsync(categoryId);

            // Assert
            result.IsActive.Should().BeFalse();
            _unitOfWorkMock.Verify(x => x.Categories.Update(It.IsAny<Category>()), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact(DisplayName = "When category does not exist should throw not found exception")]
        public async Task DeactivateCategoryAsync_WhenCategoryNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _service.DeactivateCategoryAsync(999);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        [Fact(DisplayName = "When category is already inactive should throw business rule violation exception")]
        public async Task DeactivateCategoryAsync_WhenAlreadyInactive_ShouldThrowBusinessRuleViolationException()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = 1, Name = "Food", IsActive = false };

            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);

            // Act
            Func<Task> act = async () => await _service.DeactivateCategoryAsync(categoryId);

            // Assert
            await act.Should().ThrowAsync<BusinessRuleViolationException>()
                .WithMessage("*already inactive*");
        }

        #endregion

        #region GetAllCategoriesAsync Tests

        [Fact(DisplayName = "When include inactive is true should return all categories")]
        public async Task GetAllCategoriesAsync_WhenIncludeInactive_ShouldReturnAllCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new() { Id = 1, Name = "Food", IsActive = true },
                new() { Id = 2, Name = "Travel", IsActive = false }
            };

            _unitOfWorkMock.Setup(x => x.Categories.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);
            _unitOfWorkMock.Setup(x => x.Offers.GetByCategoryIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Offer>());

            // Act
            var result = await _service.GetAllCategoriesAsync(true);

            // Assert
            result.Should().HaveCount(2);
        }

        [Fact(DisplayName = "When include inactive is false should return only active categories")]
        public async Task GetAllCategoriesAsync_WhenExcludeInactive_ShouldReturnOnlyActiveCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new() { Id = 1, Name = "Food", IsActive = true }
            };

            _unitOfWorkMock.Setup(x => x.Categories.GetActiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);
            _unitOfWorkMock.Setup(x => x.Offers.GetByCategoryIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Offer>());

            // Act
            var result = await _service.GetAllCategoriesAsync(false);

            // Assert
            result.Should().HaveCount(1);
            result.All(c => c.IsActive).Should().BeTrue();
        }

        #endregion

        #region GetActiveCategoriesAsync Tests

        [Fact(DisplayName = "When called should return only active categories")]
        public async Task GetActiveCategoriesAsync_WhenCalled_ShouldReturnOnlyActiveCategories()
        {
            // Arrange
            var categories = new List<Category>
            {
                new() { Id = 1, Name = "Food", IsActive = true },
                new() { Id = 2, Name = "Travel", IsActive = true }
            };

            _unitOfWorkMock.Setup(x => x.Categories.GetActiveAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(categories);
            _unitOfWorkMock.Setup(x => x.Offers.GetByCategoryIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Offer>());

            // Act
            var result = await _service.GetActiveCategoriesAsync();

            // Assert
            result.Should().HaveCount(2);
            result.All(c => c.IsActive).Should().BeTrue();
        }

        #endregion

        #region GetCategoryByIdAsync Tests

        [Fact(DisplayName = "When valid category id is provided should return category details")]
        public async Task GetCategoryByIdAsync_WhenValidId_ShouldReturnCategory()
        {
            // Arrange
            var categoryId = 1;
            var category = new Category { Id = 1, Name = "Food", IsActive = true };

            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(category);
            _unitOfWorkMock.Setup(x => x.Offers.GetByCategoryIdAsync(categoryId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Offer>());

            // Act
            var result = await _service.GetCategoryByIdAsync(categoryId);

            // Assert
            result.Should().NotBeNull();
            result.Name.Should().Be("Food");
        }

        [Fact(DisplayName = "When category does not exist should throw not found exception")]
        public async Task GetCategoryByIdAsync_WhenCategoryNotFound_ShouldThrowNotFoundException()
        {
            // Arrange
            _unitOfWorkMock.Setup(x => x.Categories.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Category?)null);

            // Act
            Func<Task> act = async () => await _service.GetCategoryByIdAsync(999);

            // Assert
            await act.Should().ThrowAsync<NotFoundException>();
        }

        #endregion
    }
}
