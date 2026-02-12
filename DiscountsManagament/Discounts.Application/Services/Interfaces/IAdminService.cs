using Discounts.Application.DTOs.Admin;

namespace Discounts.Application.Services.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(string? roleFilter, CancellationToken cancellationToken = default);
    Task<UserResponseDto> GetUserByIdAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserResponseDto> UpdateUserAsync(string userId, UpdateUserRequestDto request, CancellationToken cancellationToken = default);
    Task<UserResponseDto> BlockUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserResponseDto> UnblockUserAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserResponseDto> MakeAdminAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserResponseDto> RemoveAdminAsync(string userId, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default);
}