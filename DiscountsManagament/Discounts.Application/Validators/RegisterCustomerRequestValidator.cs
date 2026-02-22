// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs;
using FluentValidation;

namespace Discounts.Application.Validators
{
    public class RegisterCustomerRequestValidator : AbstractValidator<RegisterCustomerRequestDto>
    {
        public RegisterCustomerRequestValidator()
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

            // this is real life password checkleer
            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("password is required")
                .MinimumLength(8).WithMessage("password must be at least 8 characters")
                .Matches("[A-Z]").WithMessage("password must contain at least one uppercase letter")
                .Matches("[0-9]").WithMessage("password must contain at least one digit")
                .Matches("[^a-zA-Z0-9]").WithMessage("password must contain at least one special character");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("confirm password is required")
                .Equal(x => x.Password).WithMessage("passwords do not match");

            // this is added for customer to choose their own balance
            RuleFor(x => x.InitialBalance)
                .GreaterThanOrEqualTo(0).WithMessage("Initial balance cannot be negative")
                .LessThanOrEqualTo(10000).WithMessage("Initial balance cannot exceed ₾10,000"); // i assume we use lari
        }
    }
}
