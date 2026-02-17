// Copyright (C) TBC Bank.All Rights Reserved.

using System.Security.Claims;
using Discounts.Application.DTOs.GlobalSettings;
using Discounts.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminSettingsController : Controller
    {
        private readonly IGlobalSettingsService _globalSettingsService;

        public AdminSettingsController(IGlobalSettingsService globalSettingsService)
        {
            _globalSettingsService = globalSettingsService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var settings = await _globalSettingsService.GetGlobalSettingsAsync();
            var request = new UpdateGlobalSettingsRequestDto
            {
                ReservationDurationMinutes = settings.ReservationDurationMinutes,
                MerchantEditWindowHours = settings.MerchantEditWindowHours
            };
            ViewBag.Settings = settings;
            return View(request);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(UpdateGlobalSettingsRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var settings = await _globalSettingsService.GetGlobalSettingsAsync();
                ViewBag.Settings = settings;
                return View(request);
            }

            try
            {
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _globalSettingsService.UpdateGlobalSettingsAsync(adminId!, request);
                TempData["SuccessMessage"] = "Global settings updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var settings = await _globalSettingsService.GetGlobalSettingsAsync();
                ViewBag.Settings = settings;
                return View(request);
            }
        }
    }
}
