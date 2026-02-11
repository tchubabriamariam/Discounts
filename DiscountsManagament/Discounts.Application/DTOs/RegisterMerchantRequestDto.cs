namespace Discounts.Application.DTOs;

public class RegisterMerchantRequestDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? CompanyDescription { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
}