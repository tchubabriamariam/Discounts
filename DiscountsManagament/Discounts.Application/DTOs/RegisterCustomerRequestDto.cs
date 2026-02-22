// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.DTOs
{
    public class RegisterCustomerRequestDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;

        // this was problem, now customer has option to choose balance when registering
        public decimal InitialBalance { get; set; } = 0; // default to 0

    }
}
