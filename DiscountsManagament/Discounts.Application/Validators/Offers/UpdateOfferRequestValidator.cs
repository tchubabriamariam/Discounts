// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs.Offers;
using FluentValidation;

namespace Discounts.Application.Validators.Offers
{
    public class UpdateOfferRequestValidator : AbstractValidator<UpdateOfferRequestDto>
    {
        public UpdateOfferRequestValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required")
                .MaximumLength(300).WithMessage("Title can't be more then 300 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Description is required")
                .MaximumLength(2000).WithMessage("Description can't be more then 2000 characters");

            RuleFor(x => x.CategoryId)
                .GreaterThan(0).WithMessage("Category is required");

            RuleFor(x => x.OriginalPrice)
                .GreaterThan(0).WithMessage("Original price must be greater than 0");

            RuleFor(x => x.DiscountedPrice)
                .GreaterThan(0).WithMessage("Discounted price must be greater than 0")
                .LessThan(x => x.OriginalPrice).WithMessage("Discounted price must be less than original price");

            RuleFor(x => x.TotalCoupons)
                .GreaterThan(0).WithMessage("Total coupons must be greater than 0");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThan(x => x.StartDate).WithMessage("End date must be after start date");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500).WithMessage("Image URL can't be more then 500 characters")
                .When(x => x.ImageUrl is not null);
        }
    }
}
