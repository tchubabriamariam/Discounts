// Copyright (C) TBC Bank.All Rights Reserved.

using System.Security.Claims;
using Discounts.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Merchant")]
    public class MerchantController : Controller
    {
        private readonly IOfferService _offerService;

        public MerchantController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        public async Task<IActionResult> Dashboard()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var offers = await _offerService.GetMerchantOffersAsync(userId!);
            return View(offers);
        }

        public async Task<IActionResult> SalesHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var sales = await _offerService.GetSalesHistoryAsync(userId!);
            return View(sales);
        }
    }
}
