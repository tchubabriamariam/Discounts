using Discounts.Application.DTOs.GlobalSettings;
using Discounts.Application.Exceptions;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Discounts.Application.Services.Implementations;

public class GlobalSettingsService : IGlobalSettingsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<GlobalSettingsService> _logger;

    public GlobalSettingsService(
        IUnitOfWork unitOfWork,
        UserManager<ApplicationUser> userManager,
        ILogger<GlobalSettingsService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<GlobalSettingsResponseDto> GetGlobalSettingsAsync(CancellationToken cancellationToken = default)
    {
        var settings = await _unitOfWork.GlobalSettings.GetAsync(cancellationToken);

        if (settings is null)
        {
            throw new NotFoundException("Global settings not found.");
        }

        ApplicationUser? updatedByAdmin = null;

        if (!string.IsNullOrEmpty(settings.UpdatedByAdminId))
        {
            updatedByAdmin = await _userManager.FindByIdAsync(settings.UpdatedByAdminId);
        }

        var response = settings.Adapt<GlobalSettingsResponseDto>();
        response.UpdatedByAdminEmail = updatedByAdmin?.Email;

        return response;
    }

    public async Task<GlobalSettingsResponseDto> UpdateGlobalSettingsAsync(string adminUserId, UpdateGlobalSettingsRequestDto request, CancellationToken cancellationToken = default)
    {
        var admin = await _userManager.FindByIdAsync(adminUserId);

        if (admin is null)
        {
            throw new NotFoundException("Admin user", adminUserId);
        }

        var roles = await _userManager.GetRolesAsync(admin);

        if (!roles.Contains(Roles.Admin))
        {
            throw new ForbiddenException("Only administrators can update global settings.");
        }

        if (request.ReservationDurationMinutes < 5)
        {
            throw new BusinessRuleViolationException("Reservation duration must be at least 5 minutes.");
        }

        if (request.ReservationDurationMinutes > 1440)
        {
            throw new BusinessRuleViolationException("Reservation duration cannot exceed 24 hours (1440 minutes).");
        }

        if (request.MerchantEditWindowHours < 1)
        {
            throw new BusinessRuleViolationException("Merchant edit window must be at least 1 hour.");
        }

        if (request.MerchantEditWindowHours > 168)
        {
            throw new BusinessRuleViolationException("Merchant edit window cannot exceed 7 days (168 hours).");
        }

        var settings = await _unitOfWork.GlobalSettings.GetAsync(cancellationToken);

        if (settings is null)
        {
            throw new NotFoundException("Global settings not found.");
        }

        var oldReservationDuration = settings.ReservationDurationMinutes;
        var oldEditWindow = settings.MerchantEditWindowHours;

        request.Adapt(settings);
        settings.UpdatedByAdminId = adminUserId;
        settings.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.GlobalSettings.UpdateAsync(settings, cancellationToken);

        _logger.LogInformation(
            "Global settings updated by admin {AdminEmail}: ReservationDuration {OldDuration}min → {NewDuration}min, EditWindow {OldWindow}hrs → {NewWindow}hrs",
            admin.Email,
            oldReservationDuration,
            settings.ReservationDurationMinutes,
            oldEditWindow,
            settings.MerchantEditWindowHours);

        var response = settings.Adapt<GlobalSettingsResponseDto>();
        response.UpdatedByAdminEmail = admin.Email;

        return response;
    }
}




























