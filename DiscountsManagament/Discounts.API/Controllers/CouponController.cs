using System.Security.Claims;
using Discounts.Application.DTOs.Coupons;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers;


[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/coupons")]
public class CouponController : ControllerBase
{
    private readonly ICouponService _couponService;

    public CouponController(ICouponService couponService)
    {
        _couponService = couponService;
    }


    //customer
    [HttpGet("my-coupons")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> GetMyCoupons(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _couponService.GetMyCouponsAsync(userId, cancellationToken);
        
        return Ok(result);
    }

    [HttpGet("by-code/{code}")]
    [Authorize(Roles = Roles.Customer)]
    public async Task<IActionResult> GetCouponByCode(string code, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _couponService.GetCouponByCodeAsync(userId, code, cancellationToken);
        
        return Ok(result);
    }


    // merchant
    [HttpPost("mark-as-used")]
    [Authorize(Roles = Roles.Merchant)]
    public async Task<IActionResult> MarkCouponAsUsed(
        [FromBody] MarkCouponAsUsedRequestDto request, 
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _couponService.MarkCouponAsUsedAsync(userId, request, cancellationToken);
        
        return Ok(new 
        { 
            message = "Coupon marked as used successfully.",
            coupon = result 
        });
    }
}




























