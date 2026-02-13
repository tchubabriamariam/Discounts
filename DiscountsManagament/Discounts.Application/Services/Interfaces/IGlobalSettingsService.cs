using Discounts.Application.DTOs.GlobalSettings;

namespace Discounts.Application.Services.Interfaces;

public interface IGlobalSettingsService
{
    Task<GlobalSettingsResponseDto> GetGlobalSettingsAsync(CancellationToken cancellationToken = default);
    Task<GlobalSettingsResponseDto> UpdateGlobalSettingsAsync(string adminUserId, UpdateGlobalSettingsRequestDto request, CancellationToken cancellationToken = default);
}