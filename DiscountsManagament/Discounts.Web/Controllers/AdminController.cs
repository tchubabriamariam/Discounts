// Copyright (C) TBC Bank.All Rights Reserved.

using Discounts.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly IOfferService _offerService;
        private readonly ICategoryService _categoryService;

        public AdminController(
            IAdminService adminService,
            IOfferService offerService,
            ICategoryService categoryService)
        {
            _adminService = adminService;
            _offerService = offerService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            var users = await _adminService.GetAllUsersAsync(null);
            var pendingOffers = await _offerService.GetPendingOffersAsync();
            var categories = await _categoryService.GetAllCategoriesAsync(true);

            ViewBag.TotalUsers = users.Count();
            ViewBag.TotalCustomers = users.Count(u => u.Roles.Contains("Customer"));
            ViewBag.TotalMerchants = users.Count(u => u.Roles.Contains("Merchant"));
            ViewBag.PendingOffers = pendingOffers.Count();
            ViewBag.TotalCategories = categories.Count();

            return View();
        }
        public async Task<IActionResult> PendingMerchants(CancellationToken cancellationToken)
        {
            var merchants = await _adminService.GetPendingMerchantsAsync(cancellationToken);
            return View(merchants);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyMerchant(int merchantId, CancellationToken cancellationToken)
        {
            await _adminService.VerifyMerchantAsync(merchantId, cancellationToken);
            TempData["SuccessMessage"] = "Merchant verified successfully!";
            return RedirectToAction("PendingMerchants");
        }
    }
}
