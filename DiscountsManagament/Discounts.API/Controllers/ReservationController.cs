// Copyright (C) TBC Bank. All Rights Reserved.

using System.Security.Claims;
using Discounts.Application.DTOs.Reservations;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/reservations")]
    [Authorize(Roles = Roles.Customer)]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateReservation(
            [FromBody] CreateReservationRequestDto request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _reservationService.CreateReservationAsync(userId, request, cancellationToken)
                .ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetReservation),
                new { id = result.Id },
                result);
        }

        [HttpPost("{id:int}/purchase")]
        public async Task<IActionResult> PurchaseReservation(
            int id,
            [FromBody] PurchaseReservationRequestDto request,
            CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var couponCodes = await _reservationService.PurchaseReservationAsync(userId, id, request, cancellationToken)
                .ConfigureAwait(false);

            return Ok(new
            {
                message = "Purchase successful! Your coupons have been generated.",
                couponCodes,
                count = couponCodes.Count()
            });
        }

        [HttpGet("my-reservations")]
        public async Task<IActionResult> GetMyReservations(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _reservationService.GetMyReservationsAsync(userId, cancellationToken)
                .ConfigureAwait(false);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetReservation(int id, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            var result = await _reservationService.GetReservationByIdAsync(userId, id, cancellationToken)
                .ConfigureAwait(false);

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> CancelReservation(int id, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            await _reservationService.CancelReservationAsync(userId, id, cancellationToken).ConfigureAwait(false);

            return NoContent();
        }
    }
}
