// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs.Admin;
using Discounts.Application.Exceptions;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Discounts.Application.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly ILogger<AdminService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminService(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            ILogger<AdminService> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IEnumerable<UserResponseDto>> GetAllUsersAsync(string? roleFilter,
            CancellationToken cancellationToken = default)
        {
            var users = await _userManager.Users.ToListAsync(cancellationToken).ConfigureAwait(false);

            var result = new List<UserResponseDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(roleFilter) && !roles.Contains(roleFilter)) continue;

                var userDto = await MapToUserResponseAsync(user, roles.ToList()).ConfigureAwait(false);
                result.Add(userDto);
            }

            return result.OrderBy(u => u.Email);
        }

        public async Task<UserResponseDto> GetUserByIdAsync(string userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);

            if (user is null) throw new NotFoundException("User", userId);

            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

            return await MapToUserResponseAsync(user, roles.ToList()).ConfigureAwait(false);
        }

        public async Task<UserResponseDto> UpdateUserAsync(string userId, UpdateUserRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);

            if (user is null) throw new NotFoundException("User", userId);

            if (user.Email != request.Email)
            {
                var emailExists = await _userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);

                if (emailExists is not null) throw new AlreadyExistsException("User", "Email", request.Email);

                user.Email = request.Email;
                user.UserName = request.Email;
                user.NormalizedEmail = request.Email.ToUpperInvariant();
                user.NormalizedUserName = request.Email.ToUpperInvariant();
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Balance = request.Balance;

            var result = await _userManager.UpdateAsync(user).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessRuleViolationException($"Failed to update user: {errors}");
            }

            _logger.LogInformation("User updated by admin: {Email}", user.Email);

            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

            return await MapToUserResponseAsync(user, roles.ToList()).ConfigureAwait(false);
        }

        public async Task<UserResponseDto> BlockUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);

            if (user is null) throw new NotFoundException("User", userId);

            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

            if (roles.Contains(Roles.Admin)) throw new BusinessRuleViolationException("Cannot block an admin user.");

            if (!user.IsActive) throw new BusinessRuleViolationException($"User {user.Email} is already blocked.");

            user.IsActive = false;

            await _userManager.UpdateAsync(user).ConfigureAwait(false);

            _logger.LogInformation("User blocked by admin: {Email}", user.Email);

            return await MapToUserResponseAsync(user, roles.ToList()).ConfigureAwait(false);
        }

        public async Task<UserResponseDto> UnblockUserAsync(string userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);

            if (user is null) throw new NotFoundException("User", userId);

            if (user.IsActive) throw new BusinessRuleViolationException($"User {user.Email} is already active.");

            user.IsActive = true;

            await _userManager.UpdateAsync(user).ConfigureAwait(false);

            _logger.LogInformation("User unblocked by admin: {Email}", user.Email);

            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

            return await MapToUserResponseAsync(user, roles.ToList()).ConfigureAwait(false);
        }

        public async Task<UserResponseDto> MakeAdminAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);

            if (user is null) throw new NotFoundException("User", userId);

            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

            if (roles.Contains(Roles.Admin))
                throw new BusinessRuleViolationException($"User {user.Email} is already an admin.");

            var result = await _userManager.AddToRoleAsync(user, Roles.Admin).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessRuleViolationException($"Failed to add admin role: {errors}");
            }

            _logger.LogInformation("User promoted to admin: {Email}", user.Email);

            roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

            return await MapToUserResponseAsync(user, roles.ToList()).ConfigureAwait(false);
        }

        public async Task<UserResponseDto> RemoveAdminAsync(string userId,
            CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);

            if (user is null) throw new NotFoundException("User", userId);

            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

            if (!roles.Contains(Roles.Admin))
                throw new BusinessRuleViolationException($"User {user.Email} is not an admin.");

            var result = await _userManager.RemoveFromRoleAsync(user, Roles.Admin).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BusinessRuleViolationException($"Failed to remove admin role: {errors}");
            }

            _logger.LogInformation("Admin role removed from user: {Email}", user.Email);

            roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

            return await MapToUserResponseAsync(user, roles.ToList()).ConfigureAwait(false);
        }

        public async Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByIdAsync(userId).ConfigureAwait(false);

            if (user is null) throw new NotFoundException("User", userId);

            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);

            if (roles.Contains(Roles.Admin))
                throw new BusinessRuleViolationException("Cannot delete an admin user. Remove admin role first.");

            user.IsDeleted = true;
            user.DeletedAt = DateTime.UtcNow;
            user.IsActive = false;

            await _userManager.UpdateAsync(user).ConfigureAwait(false);

            _logger.LogInformation("User soft deleted by admin: {Email}", user.Email);
        }

        private async Task<UserResponseDto> MapToUserResponseAsync(ApplicationUser user, List<string> roles)
        {
            if (roles.Contains(Roles.Merchant))
            {
                var merchant = await _unitOfWork.Merchants.GetByUserIdAsync(user.Id).ConfigureAwait(false);
                user.Merchant = merchant;
            }

            var userDto = user.Adapt<UserResponseDto>();
            userDto.Roles = roles;

            return userDto;
        }
    }
}
