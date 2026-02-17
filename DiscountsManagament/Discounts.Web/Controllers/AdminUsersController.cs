// Copyright (C) TBC Bank.All Rights Reserved.

using Discounts.Application.DTOs.Admin;
using Discounts.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly IAdminService _adminService;

        public AdminUsersController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? role)
        {
            var users = await _adminService.GetAllUsersAsync(role);
            ViewBag.RoleFilter = role;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            try
            {
                var user = await _adminService.GetUserByIdAsync(id);
                var request = new UpdateUserRequestDto
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Balance = user.Balance
                };
                ViewBag.UserId = id;
                ViewBag.UserInfo = user;
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
        public async Task<IActionResult> Edit(string id, UpdateUserRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.UserId = id;
                return View(request);
            }

            try
            {
                await _adminService.UpdateUserAsync(id, request);
                TempData["SuccessMessage"] = "User updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                ViewBag.UserId = id;
                return View(request);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Block(string id)
        {
            try
            {
                await _adminService.BlockUserAsync(id);
                TempData["SuccessMessage"] = "User blocked successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unblock(string id)
        {
            try
            {
                await _adminService.UnblockUserAsync(id);
                TempData["SuccessMessage"] = "User unblocked successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeAdmin(string id)
        {
            try
            {
                await _adminService.MakeAdminAsync(id);
                TempData["SuccessMessage"] = "User promoted to admin successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdmin(string id)
        {
            try
            {
                await _adminService.RemoveAdminAsync(id);
                TempData["SuccessMessage"] = "Admin role removed successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _adminService.DeleteUserAsync(id);
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
