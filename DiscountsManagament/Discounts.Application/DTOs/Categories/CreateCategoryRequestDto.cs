// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.DTOs.Categories
{
    public class CreateCategoryRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
    }
}
