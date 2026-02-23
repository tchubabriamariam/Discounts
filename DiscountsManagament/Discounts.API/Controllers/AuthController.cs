// Copyright (C) TBC Bank. All Rights Reserved.

//using Asp.Versioning;

using Discounts.Application.DTOs;
using Discounts.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers
{
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
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterCustomerRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterCustomerAsync(request, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpPost("register/merchant")]
        public async Task<IActionResult> RegisterMerchant([FromBody] RegisterMerchantRequestDto request,
            CancellationToken cancellationToken)
        {
            var result = await _authService.RegisterMerchantAsync(request, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
        {
            var result = await _authService.LoginAsync(request, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }
    }
}
