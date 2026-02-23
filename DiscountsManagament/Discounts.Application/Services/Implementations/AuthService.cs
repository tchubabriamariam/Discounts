// Copyright (C) TBC Bank. All Rights Reserved.

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Discounts.Application.DTOs;
using Discounts.Application.IRepositories;
using Discounts.Application.Services.Interfaces;
using Discounts.Domain.Entity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Discounts.Application.Services.Implementations
{
    public class AuthService : IAuthService
    {
        // this contains business logic
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<AuthService> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterCustomerAsync(RegisterCustomerRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);

            if (existingUser is not null)
                throw new InvalidOperationException($"User with email {request.Email} already exists.");

            if (request.Password != request.ConfirmPassword)
                throw new InvalidOperationException("Passwords do not match.");

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Balance = request.InitialBalance,
                IsActive = true,
                CreatedAt = DateTime.UtcNow

            };

            var result = await _userManager.CreateAsync(user, request.Password).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            await _userManager.AddToRoleAsync(user, Roles.Customer).ConfigureAwait(false);

            _logger.LogInformation("Customer registered successfully: {Email}", user.Email);

            var token = await GenerateJwtTokenAsync(user).ConfigureAwait(false);

            return token;
        }

        public async Task<AuthResponseDto> RegisterMerchantAsync(RegisterMerchantRequestDto request,
            CancellationToken cancellationToken = default)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);

            if (existingUser is not null)
                throw new InvalidOperationException($"User with email {request.Email} already exists.");

            if (request.Password != request.ConfirmPassword)
                throw new InvalidOperationException("Passwords do not match.");

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

            var result = await _userManager.CreateAsync(user, request.Password).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            await _userManager.AddToRoleAsync(user, Roles.Merchant).ConfigureAwait(false);

            var merchant = new Merchant
            {
                UserId = user.Id,
                CompanyName = request.CompanyName,
                Description = request.CompanyDescription,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                ContactEmail = request.Email
            };

            await _unitOfWork.Merchants.AddAsync(merchant, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("Merchant registered successfully: {Email}", user.Email);

            var token = await GenerateJwtTokenAsync(user).ConfigureAwait(false);

            return token;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request,
            CancellationToken cancellationToken = default)
        {
            // here i say invalid email or passwrod because if someone fake tries to enter we should not give them too much info
            var user = await _userManager.FindByEmailAsync(request.Email).ConfigureAwait(false);

            if (user is null) throw new UnauthorizedAccessException("Invalid email or password");

            if (!user.IsActive) throw new UnauthorizedAccessException("Your account has been deactivated");

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password).ConfigureAwait(false);

            if (!passwordValid) throw new UnauthorizedAccessException("Invalid email or password");

            _logger.LogInformation("User logged in: {Email}", user.Email);

            var token = await GenerateJwtTokenAsync(user).ConfigureAwait(false);

            return token;
        }


        // these are helpers
        private async Task<AuthResponseDto> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user).ConfigureAwait(false);
            // var role = roles.FirstOrDefault() ?? string.Empty; // they have main role (first added role)

            int? merchantId = null;

            if (roles.Contains(Roles.Merchant))
            {
                // we need to see which offers merchants own, used when customers verify coupons
                var merchant = await _unitOfWork.Merchants.GetByUserIdAsync(user.Id).ConfigureAwait(false);
                merchantId = merchant?.Id;
            }

            // we build claims in here, tokens inside info
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email!),
                new(ClaimTypes.GivenName, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                // new(ClaimTypes.Role, role) // this is for future authorization by roles
            };
            foreach (var r in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, r));
            }

            if (merchantId.HasValue) claims.Add(new Claim("merchantId", merchantId.Value.ToString())); // put merchantid into token so dont need database to find the id

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // using the algorithm from lecture
            var expiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"]));

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expiry,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token); // turn into long string

            // this is better for decoding
            return new AuthResponseDto
            {
                Token = tokenString,
                Email = user.Email!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                // Role = role,
                Role = string.Join(", ", roles),
                MerchantId = merchantId,
                ExpiresAt = expiry
            };
        }
    }
}
