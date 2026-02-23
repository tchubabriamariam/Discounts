// Copyright (C) TBC Bank.All Rights Reserved.

using Discounts.Application.DTOs.Admin;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Discounts.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : Controller
    {
        private readonly IAdminService _adminService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;


        public AdminUsersController(IAdminService adminService, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            _adminService = adminService;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
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
        public async Task<IActionResult> AddRole(string id, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) throw new Exception("User not found.");

                var validRoles = new[] { "Admin", "Merchant", "Customer" };
                if (!validRoles.Contains(role)) throw new Exception("Invalid role.");

                if (await _userManager.IsInRoleAsync(user, role))
                    throw new Exception($"User already has the {role} role.");

                await _userManager.AddToRoleAsync(user, role);
                if (role == Roles.Merchant)
                {
                    var existingMerchant = await _unitOfWork.Merchants.GetByUserIdAsync(id);

                    if (existingMerchant is null)
                    {
                        var merchant = new Merchant
                        {
                            UserId = id,
                            CompanyName = $"{user.FirstName} {user.LastName}",
                            ContactEmail = user.Email!,
                            IsVerified = false
                        };
                        await _unitOfWork.Merchants.AddAsync(merchant);
                        await _unitOfWork.SaveChangesAsync();
                    }
                }

                TempData["SuccessMessage"] = $"Role '{role}' added successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string id, string role)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null) throw new Exception("User not found.");

                if (!await _userManager.IsInRoleAsync(user, role))
                    throw new Exception($"User does not have the {role} role.");

                await _userManager.RemoveFromRoleAsync(user, role);
                TempData["SuccessMessage"] = $"Role '{role}' removed successfully.";
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
