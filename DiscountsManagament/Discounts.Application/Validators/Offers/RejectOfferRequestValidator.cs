// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs.Offers;
using FluentValidation;

namespace Discounts.Application.Validators.Offers
{
    public class RejectOfferRequestValidator : AbstractValidator<RejectOfferRequestDto>
    {
        public RejectOfferRequestValidator() =>
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Rejection reason is required")
                .MaximumLength(500).WithMessage("Reason can't be more then 500 characters");
    }
}
