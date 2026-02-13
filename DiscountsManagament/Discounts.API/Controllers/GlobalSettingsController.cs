using System.Security.Claims;
using Discounts.Application.DTOs.GlobalSettings;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/settings")]
public class GlobalSettingsController : ControllerBase
{
    private readonly IGlobalSettingsService _globalSettingsService;

    public GlobalSettingsController(IGlobalSettingsService globalSettingsService)
    {
        _globalSettingsService = globalSettingsService;
    }

    [HttpGet]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Merchant}")]
    public async Task<IActionResult> GetGlobalSettings(CancellationToken cancellationToken)
    {
        var result = await _globalSettingsService.GetGlobalSettingsAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateGlobalSettings(
        [FromBody] UpdateGlobalSettingsRequestDto request,
        CancellationToken cancellationToken)
    {
        var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _globalSettingsService.UpdateGlobalSettingsAsync(adminUserId, request, cancellationToken);
        
        return Ok(new
        {
            message = "Global settings updated successfully.",
            settings = result
        });
    }
}




























