// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs.Reservations;
using FluentValidation;

namespace Discounts.Application.Validators.Reservations
{
    public class CreateReservationRequestValidator : AbstractValidator<CreateReservationRequestDto>
    {
        public CreateReservationRequestValidator()
        {
            RuleFor(x => x.OfferId)
                .GreaterThan(0).WithMessage("Offer ID must be greater than 0.");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be at least 1.")
                .LessThanOrEqualTo(10).WithMessage("Cannot reserve more than 10 coupons at once.");
        }
    }
}
