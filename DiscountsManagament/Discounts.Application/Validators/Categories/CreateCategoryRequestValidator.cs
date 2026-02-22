// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs.Categories;
using FluentValidation;

namespace Discounts.Application.Validators.Categories
{
    public class CreateCategoryRequestValidator : AbstractValidator<CreateCategoryRequestDto>
    {
        public CreateCategoryRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Category name is required")
                .MaximumLength(100).WithMessage("Category name can't be more then 100 characters");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description can't be more then 500 characters")
                .When(x => x.Description is not null);

            RuleFor(x => x.IconUrl)
                .MaximumLength(500).WithMessage("Icon URL can't be more then 500 characters")
                .When(x => x.IconUrl is not null);
        }
    }
}
