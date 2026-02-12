using Discounts.Application.DTOs.Admin;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers;

// this whole controller is for admin only
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Roles = Roles.Admin)]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers(
        [FromQuery] string? roleFilter,
        CancellationToken cancellationToken)
    {
        var result = await _adminService.GetAllUsersAsync(roleFilter, cancellationToken);
        return Ok(result);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserById(string id, CancellationToken cancellationToken)
    {
        var result = await _adminService.GetUserByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(
        string id,
        [FromBody] UpdateUserRequestDto request,
        CancellationToken cancellationToken)
    {
        var result = await _adminService.UpdateUserAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("users/{id}/block")]
    public async Task<IActionResult> BlockUser(string id, CancellationToken cancellationToken)
    {
        var result = await _adminService.BlockUserAsync(id, cancellationToken);
        return Ok(new { message = "User blocked successfully.", user = result });
    }

    [HttpPost("users/{id}/unblock")]
    public async Task<IActionResult> UnblockUser(string id, CancellationToken cancellationToken)
    {
        var result = await _adminService.UnblockUserAsync(id, cancellationToken);
        return Ok(new { message = "User unblocked successfully.", user = result });
    }

    [HttpPost("users/{id}/make-admin")]
    public async Task<IActionResult> MakeAdmin(string id, CancellationToken cancellationToken)
    {
        var result = await _adminService.MakeAdminAsync(id, cancellationToken);
        return Ok(new { message = "User promoted to admin successfully.", user = result });
    }

    [HttpPost("users/{id}/remove-admin")]
    public async Task<IActionResult> RemoveAdmin(string id, CancellationToken cancellationToken)
    {
        var result = await _adminService.RemoveAdminAsync(id, cancellationToken);
        return Ok(new { message = "Admin role removed successfully.", user = result });
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(string id, CancellationToken cancellationToken)
    {
        await _adminService.DeleteUserAsync(id, cancellationToken);
        return NoContent();
    }
}