// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.DTOs.Categories
{
    public class UpdateCategoryRequestDto
    {
        // admin changes already existing category, same as creating but if needs change or new rules this will help
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
    }
}
