using System.Diagnostics;
using Discounts.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Discounts.Web.Models;

namespace Discounts.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IOfferService _offerService;
    private readonly ICategoryService _categoryService;

    public HomeController(
        ILogger<HomeController> logger,
        IOfferService offerService,
        ICategoryService categoryService)
    {
        _logger = logger;
        _offerService = offerService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(int? categoryId, decimal? minPrice, decimal? maxPrice)
    {
        var offers = await _offerService.GetApprovedOffersAsync(categoryId, minPrice, maxPrice);
        var categories = await _categoryService.GetActiveCategoriesAsync();

        ViewBag.Categories = categories;
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;

        return View(offers);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
