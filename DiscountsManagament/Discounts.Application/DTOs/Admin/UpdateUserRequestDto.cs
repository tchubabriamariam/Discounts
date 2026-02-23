// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.DTOs.Admin
{
    public class UpdateUserRequestDto
    {
        // this is when admin modifies user
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public decimal Balance { get; set; }
    }
}
