using Discounts.Application.DTOs.Categories;

namespace Discounts.Application.Services.Interfaces;

public interface ICategoryService
{
    // admin
    Task<CategoryResponseDto> CreateCategoryAsync(CreateCategoryRequestDto request, CancellationToken cancellationToken = default);
    Task<CategoryResponseDto> UpdateCategoryAsync(int categoryId, UpdateCategoryRequestDto request, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<CategoryResponseDto> ActivateCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<CategoryResponseDto> DeactivateCategoryAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<CategoryResponseDto>> GetAllCategoriesAsync(bool includeInactive, CancellationToken cancellationToken = default);

    // merchand and customer too, everyone has access to this
    Task<IEnumerable<CategoryResponseDto>> GetActiveCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryResponseDto> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default);
}