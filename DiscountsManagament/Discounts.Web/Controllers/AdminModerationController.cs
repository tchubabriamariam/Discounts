// Copyright (C) TBC Bank.All Rights Reserved.

using System.Security.Claims;
using Discounts.Application.DTOs.Offers;
using Discounts.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminModerationController : Controller
    {
        private readonly IOfferService _offerService;

        public AdminModerationController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var pendingOffers = await _offerService.GetPendingOffersAsync();
            return View(pendingOffers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _offerService.ApproveOfferAsync(adminId!, id);
                TempData["SuccessMessage"] = "Offer approved successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, RejectOfferRequestDto request)
        {
            try
            {
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _offerService.RejectOfferAsync(adminId!, id, request);
                TempData["SuccessMessage"] = "Offer rejected.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
