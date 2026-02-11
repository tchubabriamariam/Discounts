using System.Security.Claims;
using Discounts.Application.DTOs.Reservations;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.API.Controllers;

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

    /// <summary>
    /// Creates a new reservation for an offer (temporary hold)
    /// </summary>
    /// <param name="request">Reservation details including offer ID and quantity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created reservation details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateReservation(
        [FromBody] CreateReservationRequestDto request, 
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _reservationService.CreateReservationAsync(userId, request, cancellationToken);
        
        return CreatedAtAction(
            nameof(GetReservation), 
            new { id = result.Id }, 
            result);
    }

    /// <summary>
    /// Purchases/confirms a reservation, converting it to actual coupons
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <param name="request">Purchase details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of generated coupon codes</returns>
    [HttpPost("{id:int}/purchase")]
    [ProducesResponseType(typeof(IEnumerable<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PurchaseReservation(
        int id, 
        [FromBody] PurchaseReservationRequestDto request, 
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var couponCodes = await _reservationService.PurchaseReservationAsync(userId, id, request, cancellationToken);
        
        return Ok(new 
        { 
            message = "Purchase successful! Your coupons have been generated.",
            couponCodes = couponCodes,
            count = couponCodes.Count()
        });
    }

    /// <summary>
    /// Gets all reservations for the current user
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of user's reservations</returns>
    [HttpGet("my-reservations")]
    [ProducesResponseType(typeof(IEnumerable<ReservationResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyReservations(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _reservationService.GetMyReservationsAsync(userId, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Gets a specific reservation by ID
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Reservation details</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReservationResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReservation(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await _reservationService.GetReservationByIdAsync(userId, id, cancellationToken);
        
        return Ok(result);
    }

    /// <summary>
    /// Cancels an active reservation manually
    /// </summary>
    /// <param name="id">Reservation ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelReservation(int id, CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await _reservationService.CancelReservationAsync(userId, id, cancellationToken);
        
        return NoContent();
    }
}