// Copyright (C) TBC Bank.All Rights Reserved.

using System.Security.Claims;
using Discounts.Application.DTOs.Offers;
using Discounts.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Merchant")]
    public class MerchantOffersController : Controller
    {
        private readonly IOfferService _offerService;
        private readonly ICategoryService _categoryService;

        public MerchantOffersController(IOfferService offerService, ICategoryService categoryService)
        {
            _offerService = offerService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> MyOffers()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var offers = await _offerService.GetMerchantOffersAsync(userId!);
            return View(offers);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _categoryService.GetActiveCategoriesAsync();
            ViewBag.Categories = categories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOfferRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetActiveCategoriesAsync();
                ViewBag.Categories = categories;
                return View(request);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _offerService.CreateOfferAsync(userId!, request);
                TempData["SuccessMessage"] = "Offer created successfully! It is pending admin approval.";
                return RedirectToAction(nameof(MyOffers));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var categories = await _categoryService.GetActiveCategoriesAsync();
                ViewBag.Categories = categories;
                return View(request);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var offer = await _offerService.GetOfferDetailsAsync(id);

                var categories = await _categoryService.GetActiveCategoriesAsync();
                ViewBag.Categories = categories;

                var request = new UpdateOfferRequestDto
                {
                    Title = offer.Title,
                    Description = offer.Description,
                    ImageUrl = offer.ImageUrl,
                    OriginalPrice = offer.OriginalPrice,
                    DiscountedPrice = offer.DiscountedPrice,
                    TotalCoupons = offer.TotalCoupons,
                    StartDate = offer.StartDate,
                    EndDate = offer.EndDate
                };

                ViewBag.OfferId = id;
                return View(request);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(MyOffers));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateOfferRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var categories = await _categoryService.GetActiveCategoriesAsync();
                ViewBag.Categories = categories;
                ViewBag.OfferId = id;
                return View(request);
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _offerService.UpdateOfferAsync(userId!, id, request);
                TempData["SuccessMessage"] = "Offer updated successfully!";
                return RedirectToAction(nameof(MyOffers));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                var categories = await _categoryService.GetActiveCategoriesAsync();
                ViewBag.Categories = categories;
                ViewBag.OfferId = id;
                return View(request);
            }
        }
    }
}
