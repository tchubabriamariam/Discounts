// Copyright (C) TBC Bank.All Rights Reserved.

using Discounts.Application.DTOs.Categories;
using Discounts.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;

        public AdminCategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.GetAllCategoriesAsync(true);
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // saves from hackers fake webpages
        public async Task<IActionResult> Create(CreateCategoryRequestDto request)
        {
            // if fluentvalitaion found any errors i show form again with view
            if (!ModelState.IsValid) return View(request);

            try
            {
                await _categoryService.CreateCategoryAsync(request);
                TempData["SuccessMessage"] = "Category created successfully!"; // save massage which survives redirect
                return RedirectToAction(nameof(Index)); // goes to index()
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message); // service throws exception add to errors
                return View(request); // display view with errors
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var category = await _categoryService.GetCategoryByIdAsync(id);
                var request = new UpdateCategoryRequestDto
                {
                    Name = category.Name,
                    Description = category.Description,
                    IconUrl = category.IconUrl
                };
                ViewBag.CategoryId = id;
                ViewBag.CategoryInfo = category;
                return View(request);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateCategoryRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.CategoryId = id;
                return View(request);
            }

            try
            {
                await _categoryService.UpdateCategoryAsync(id, request);
                TempData["SuccessMessage"] = "Category updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.CategoryId = id;
                return View(request);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                await _categoryService.ActivateCategoryAsync(id);
                TempData["SuccessMessage"] = "Category activated.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                await _categoryService.DeactivateCategoryAsync(id);
                TempData["SuccessMessage"] = "Category deactivated.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _categoryService.DeleteCategoryAsync(id);
                TempData["SuccessMessage"] = "Category deleted.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
