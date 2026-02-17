// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs.Categories;
using Discounts.Application.Exceptions;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Mapster;
using Microsoft.Extensions.Logging;

namespace Discounts.Application.Services.Implementations
{
    public class CategoryService : ICategoryService
    {
        private readonly ILogger<CategoryService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(
            IUnitOfWork unitOfWork,
            ILogger<CategoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var nameExists = await _unitOfWork.Categories.NameExistsAsync(request.Name, cancellationToken)
                .ConfigureAwait(false);

            if (nameExists) throw new AlreadyExistsException("Category", "Name", request.Name);

            var category = request.Adapt<Category>();
            category.IsActive = true;

            await _unitOfWork.Categories.AddAsync(category, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Category created: {CategoryName} (ID: {CategoryId})", category.Name, category.Id);

            return await MapToCategoryResponseAsync(category).ConfigureAwait(false);
        }

        public async Task<CategoryResponseDto> UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken)
                .ConfigureAwait(false);

            if (category is null) throw new NotFoundException("Category", categoryId);

            if (category.Name != request.Name)
            {
                var nameExists = await _unitOfWork.Categories.NameExistsAsync(request.Name, cancellationToken)
                    .ConfigureAwait(false);

                if (nameExists) throw new AlreadyExistsException("Category", "Name", request.Name);
            }

            request.Adapt(category);

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Category updated: {CategoryName} (ID: {CategoryId})", category.Name, category.Id);

            return await MapToCategoryResponseAsync(category).ConfigureAwait(false);
        }

        public async Task DeleteCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken)
                .ConfigureAwait(false);

            if (category is null) throw new NotFoundException("Category", categoryId);

            var offersInCategory = await _unitOfWork.Offers.GetByCategoryIdAsync(categoryId, cancellationToken)
                .ConfigureAwait(false);

            if (offersInCategory.Any())
                throw new BusinessRuleViolationException(
                    $"Cannot delete category '{category.Name}' because it has {offersInCategory.Count()} associated offers.");

            _unitOfWork.Categories.SoftDelete(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Category soft deleted: {CategoryName} (ID: {CategoryId})", category.Name,
                category.Id);
        }

        public async Task<CategoryResponseDto> ActivateCategoryAsync(int categoryId,
            CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken)
                .ConfigureAwait(false);

            if (category is null) throw new NotFoundException("Category", categoryId);

            if (category.IsActive)
                throw new BusinessRuleViolationException($"Category '{category.Name}' is already active.");

            category.IsActive = true;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Category activated: {CategoryName} (ID: {CategoryId})", category.Name, category.Id);

            return await MapToCategoryResponseAsync(category).ConfigureAwait(false);
        }

        public async Task<CategoryResponseDto> DeactivateCategoryAsync(int categoryId,
            CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken)
                .ConfigureAwait(false);

            if (category is null) throw new NotFoundException("Category", categoryId);

            if (!category.IsActive)
                throw new BusinessRuleViolationException($"Category '{category.Name}' is already inactive.");

            category.IsActive = false;

            _unitOfWork.Categories.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Category deactivated: {CategoryName} (ID: {CategoryId})", category.Name,
                category.Id);

            return await MapToCategoryResponseAsync(category).ConfigureAwait(false);
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(bool includeInactive,
            CancellationToken cancellationToken = default)
        {
            IEnumerable<Category> categories;

            if (includeInactive)
                categories = await _unitOfWork.Categories.GetAllAsync(cancellationToken).ConfigureAwait(false);
            else
                categories = await _unitOfWork.Categories.GetActiveAsync(cancellationToken).ConfigureAwait(false);

            var result = new List<CategoryResponseDto>();

            foreach (var category in categories)
                result.Add(await MapToCategoryResponseAsync(category).ConfigureAwait(false));

            return result.OrderBy(c => c.Name);
        }

        // ── Public ────────────────────────────────────────────────────────────────

        public async Task<IEnumerable<CategoryResponseDto>> GetActiveCategoriesAsync(
            CancellationToken cancellationToken = default)
        {
            var categories = await _unitOfWork.Categories.GetActiveAsync(cancellationToken).ConfigureAwait(false);

            var result = new List<CategoryResponseDto>();

            foreach (var category in categories)
                result.Add(await MapToCategoryResponseAsync(category).ConfigureAwait(false));

            return result.OrderBy(c => c.Name);
        }

        public async Task<CategoryResponseDto> GetCategoryByIdAsync(int categoryId,
            CancellationToken cancellationToken = default)
        {
            var category = await _unitOfWork.Categories.GetByIdAsync(categoryId, cancellationToken)
                .ConfigureAwait(false);

            if (category is null) throw new NotFoundException("Category", categoryId);

            return await MapToCategoryResponseAsync(category).ConfigureAwait(false);
        }

        // ── Helper ────────────────────────────────────────────────────────────────

        private async Task<CategoryResponseDto> MapToCategoryResponseAsync(Category category)
        {
            var offers = await _unitOfWork.Offers.GetByCategoryIdAsync(category.Id).ConfigureAwait(false);

            var response = category.Adapt<CategoryResponseDto>();
            response.OfferCount = offers.Count();

            return response;
        }
    }
}
