// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs.Admin;
using FluentValidation;

namespace Discounts.Application.Validators.Admin
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequestDto>
    {
        public UpdateUserRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name can't be more then 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required.")
                .MaximumLength(100).WithMessage("Last name can't be more then 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.Balance)
                .GreaterThanOrEqualTo(0).WithMessage("Balance can't be negative");
        }
    }
}
