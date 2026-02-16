// Copyright (C) TBC Bank.All Rights Reserved.

using System.Security.Claims;
using Discounts.Application.DTOs.Reservations;
using Discounts.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Customer")]
    public class ReservationsController : Controller
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet]
        public async Task<IActionResult> MyReservations()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservations = await _reservationService.GetMyReservationsAsync(userId!);
            return View(reservations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReservationRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid reservation data.";
                return RedirectToAction("Details", "Offers", new { id = request.OfferId });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _reservationService.CreateReservationAsync(userId!, request);
                TempData["SuccessMessage"] = "Reservation created successfully! Complete your purchase within 30 minutes.";
                return RedirectToAction(nameof(MyReservations));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Details", "Offers", new { id = request.OfferId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purchase(int id, PurchaseReservationRequestDto request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var couponCodes = await _reservationService.PurchaseReservationAsync(userId!, id, request);

                TempData["SuccessMessage"] = $"Purchase successful! Your coupon codes: {string.Join(", ", couponCodes)}";
                return RedirectToAction("MyCoupons", "Coupons");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(MyReservations));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _reservationService.CancelReservationAsync(userId!, id);
                TempData["SuccessMessage"] = "Reservation cancelled successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(MyReservations));
        }
    }
}
