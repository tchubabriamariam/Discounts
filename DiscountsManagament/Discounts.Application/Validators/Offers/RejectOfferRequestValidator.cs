// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs.Offers;
using FluentValidation;

namespace Discounts.Application.Validators.Offers
{
    public class RejectOfferRequestValidator : AbstractValidator<RejectOfferRequestDto>
    {
        public RejectOfferRequestValidator() =>
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Rejection reason is required.")
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
    }
}
