// Copyright (C) TBC Bank.All Rights Reserved.

using System.Security.Claims;
using Discounts.Application.DTOs.Coupons;
using Discounts.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Merchant")]
    public class MerchantCouponsController : Controller
    {
        private readonly ICouponService _couponService;

        public MerchantCouponsController(ICouponService couponService)
        {
            _couponService = couponService;
        }

        [HttpGet]
        public IActionResult VerifyCoupon()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCoupon(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError(string.Empty, "Please enter a coupon code.");
                return View();
            }

            // Store code in ViewBag to show CouponDetails confirmation page
            // We don't verify ownership here - MarkCouponAsUsedAsync will do that
            ViewBag.Code = code.Trim().ToUpper();
            return View("CouponDetails");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsUsed(string code)
        {
            try
            {
                var merchantUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var coupon = await _couponService.MarkCouponAsUsedAsync(merchantUserId!,
                    new MarkCouponAsUsedRequestDto { Code = code });
                TempData["SuccessMessage"] = $"Coupon '{coupon.Code}' has been marked as used successfully! Offer: {coupon.OfferTitle}";
                return RedirectToAction(nameof(VerifyCoupon));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(VerifyCoupon));
            }
        }
    }
}
