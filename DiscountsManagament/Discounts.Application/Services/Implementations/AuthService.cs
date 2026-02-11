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

namespace Discounts.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager   = userManager;
        _unitOfWork    = unitOfWork;
        _configuration = configuration;
        _logger        = logger;
    }

    public async Task<AuthResponseDto> RegisterCustomerAsync(RegisterCustomerRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException($"User with email {request.Email} already exists.");
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new InvalidOperationException("Passwords do not match.");
        }

        var user = new ApplicationUser
        {
            UserName  = request.Email,
            Email     = request.Email,
            FirstName = request.FirstName,
            LastName  = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        await _userManager.AddToRoleAsync(user, Roles.Customer);

        _logger.LogInformation("Customer registered successfully: {Email}", user.Email);

        var token = await GenerateJwtTokenAsync(user);

        return token;
    }

    public async Task<AuthResponseDto> RegisterMerchantAsync(RegisterMerchantRequestDto request, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException($"User with email {request.Email} already exists.");
        }

        if (request.Password != request.ConfirmPassword)
        {
            throw new InvalidOperationException("Passwords do not match.");
        }

        var user = new ApplicationUser
        {
            UserName  = request.Email,
            Email     = request.Email,
            FirstName = request.FirstName,
            LastName  = request.LastName
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create user: {errors}");
        }

        await _userManager.AddToRoleAsync(user, Roles.Merchant);

        var merchant = new Merchant
        {
            UserId      = user.Id,
            CompanyName = request.CompanyName,
            Description = request.CompanyDescription,
            PhoneNumber = request.PhoneNumber,
            Address     = request.Address,
            ContactEmail = request.Email
        };

        await _unitOfWork.Merchants.AddAsync(merchant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Merchant registered successfully: {Email}", user.Email);

        var token = await GenerateJwtTokenAsync(user);

        return token;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Your account has been deactivated.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        _logger.LogInformation("User logged in: {Email}", user.Email);

        var token = await GenerateJwtTokenAsync(user);

        return token;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private async Task<AuthResponseDto> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var role  = roles.FirstOrDefault() ?? string.Empty;

        int? merchantId = null;

        if (role == Roles.Merchant)
        {
            var merchant = await _unitOfWork.Merchants.GetByUserIdAsync(user.Id);
            merchantId   = merchant?.Id;
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.Role, role)
        };

        if (merchantId.HasValue)
        {
            claims.Add(new Claim("merchantId", merchantId.Value.ToString()));
        }

        var key     = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var creds   = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiry  = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryMinutes"]));

        var token = new JwtSecurityToken(
            issuer:             _configuration["Jwt:Issuer"],
            audience:           _configuration["Jwt:Audience"],
            claims:             claims,
            expires:            expiry,
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthResponseDto
        {
            Token     = tokenString,
            Email     = user.Email!,
            FirstName = user.FirstName,
            LastName  = user.LastName,
            Role      = role,
            MerchantId = merchantId,
            ExpiresAt = expiry
        };
    }
}






