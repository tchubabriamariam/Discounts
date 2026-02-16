// Copyright (C) TBC Bank.All Rights Reserved.

using Discounts.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers
{
    public class OffersController : Controller
    {
        private readonly IOfferService _offerService;

        public OffersController(IOfferService offerService)
        {
            _offerService = offerService;
        }

        public async Task<IActionResult> Index(int? categoryId, decimal? minPrice, decimal? maxPrice)
        {
            var offers = await _offerService.GetApprovedOffersAsync(categoryId, minPrice, maxPrice);
            return View(offers);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var offer = await _offerService.GetOfferDetailsAsync(id);
                return View(offer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Index", "Home");
            }
        }
    }
}
