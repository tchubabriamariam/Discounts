// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.DTOs.Categories
{
    public class CreateCategoryRequestDto
    {
        // admin creates new category
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }

        // no isActive because when created it is automatically active because admin creates it
    }
}
