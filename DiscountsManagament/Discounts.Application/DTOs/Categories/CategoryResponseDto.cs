// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.DTOs.Categories
{
    public class CategoryResponseDto
    {
        // when user wants to see a list of categories
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconUrl { get; set; }
        public bool IsActive { get; set; } // admin controlls this
        public int OfferCount { get; set; } // mapper counts how many offers are linked to this category
        public DateTime CreatedAt { get; set; }
    }
}
