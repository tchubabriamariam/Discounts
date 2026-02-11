using Microsoft.AspNetCore.Mvc; 
//using Asp.Versioning;
using Discounts.Application.DTOs;
using Discounts.Application.Services.Interfaces;

namespace Discounts.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register/customer")]
    public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterCustomerAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("register/merchant")]
    public async Task<IActionResult> RegisterMerchant([FromBody] RegisterMerchantRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterMerchantAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }
}