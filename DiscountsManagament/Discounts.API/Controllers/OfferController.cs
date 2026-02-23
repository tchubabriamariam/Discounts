// Copyright (C) TBC Bank. All Rights Reserved.

using System.Security.Claims;
using Discounts.Application.DTOs.Offers;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/offers")]
    public class OfferController : ControllerBase
    {
        private readonly IOfferService _offerService;

        public OfferController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        // customer

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetApprovedOffers(
            [FromQuery] int? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            CancellationToken cancellationToken)
        {
            var result = await _offerService.GetApprovedOffersAsync(categoryId, minPrice, maxPrice, cancellationToken)
                .ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetOfferDetails(int id, CancellationToken cancellationToken)
        {
            var result = await _offerService.GetOfferDetailsAsync(id, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        // merchant

        [HttpPost]
        [Authorize(Roles = Roles.Merchant)]
        public async Task<IActionResult> CreateOffer([FromBody] CreateOfferRequestDto request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _offerService.CreateOfferAsync(userId, request, cancellationToken).ConfigureAwait(false);
            return CreatedAtAction(nameof(GetOfferDetails), new { id = result.Id }, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Merchant)]
        public async Task<IActionResult> UpdateOffer(int id, [FromBody] UpdateOfferRequestDto request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _offerService.UpdateOfferAsync(userId, id, request, cancellationToken)
                .ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("my-offers")]
        [Authorize(Roles = Roles.Merchant)]
        public async Task<IActionResult> GetMyOffers(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _offerService.GetMerchantOffersAsync(userId, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpGet("sales-history")]
        [Authorize(Roles = Roles.Merchant)]
        public async Task<IActionResult> GetSalesHistory(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _offerService.GetSalesHistoryAsync(userId, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        // admin
        [HttpGet("pending")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> GetPendingOffers(CancellationToken cancellationToken)
        {
            var result = await _offerService.GetPendingOffersAsync(cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpPost("{id:int}/approve")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> ApproveOffer(int id, CancellationToken cancellationToken)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _offerService.ApproveOfferAsync(adminId, id, cancellationToken).ConfigureAwait(false);
            return Ok(result);
        }

        [HttpPost("{id:int}/reject")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> RejectOffer(int id, [FromBody] RejectOfferRequestDto request,
            CancellationToken cancellationToken)
        {
            var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _offerService.RejectOfferAsync(adminId, id, request, cancellationToken)
                .ConfigureAwait(false);
            return Ok(result);
        }
    }
}
