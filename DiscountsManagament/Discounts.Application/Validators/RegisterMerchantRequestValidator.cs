// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs;
using FluentValidation;

namespace Discounts.Application.Validators
{
    public class RegisterMerchantRequestValidator : AbstractValidator<RegisterMerchantRequestDto>
    {
        public RegisterMerchantRequestValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("first name is required")
                .MaximumLength(100).WithMessage("first name cannot be more then 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("last name is required")
                .MaximumLength(100).WithMessage("last name cannot be more then 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("email is required")
                .EmailAddress().WithMessage("invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("password is required")
                .MinimumLength(8).WithMessage("password must be at least 8 characters")
                .Matches("[A-Z]").WithMessage("password must contain at least one uppercase letter")
                .Matches("[0-9]").WithMessage("password must contain at least one digit")
                .Matches("[^a-zA-Z0-9]").WithMessage("password must contain at least one special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("confirm password is required")
                .Equal(x => x.Password).WithMessage("passwords do not match");

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("company name is required")
                .MaximumLength(200).WithMessage("company name cannot be more then 200 characters");

            RuleFor(x => x.CompanyDescription)
                .MaximumLength(1000).WithMessage("description cannot be more then 1000 characters")
                .When(x => x.CompanyDescription is not null);

            RuleFor(x => x.PhoneNumber)
                .MaximumLength(50).WithMessage("phone number cannot be more then 50 characters")
                .When(x => x.PhoneNumber is not null);

            RuleFor(x => x.Address)
                .MaximumLength(500).WithMessage("address cannot be more then 500 characters")
                .When(x => x.Address is not null);
        }
    }
}
