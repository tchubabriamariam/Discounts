// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs;
using FluentValidation;

namespace Discounts.Application.Validators
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("email is required")
                .EmailAddress().WithMessage("invalid email format");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("password is required");
        }
    }
}
