// Copyright (C) TBC Bank. All Rights Reserved.

using Discounts.Application.DTOs.GlobalSettings;
using FluentValidation;

namespace Discounts.Application.Validators.GlobalSettings
{
    public class UpdateGlobalSettingsRequestValidator : AbstractValidator<UpdateGlobalSettingsRequestDto>
    {
        public UpdateGlobalSettingsRequestValidator()
        {
            RuleFor(x => x.ReservationDurationMinutes)
                .GreaterThanOrEqualTo(5).WithMessage("Reservation duration must be at least 5 minutes")
                .LessThanOrEqualTo(1440).WithMessage("Reservation duration can't be more then 24 hours (1440 minutes)");

            RuleFor(x => x.MerchantEditWindowHours)
                .GreaterThanOrEqualTo(1).WithMessage("Merchant edit window must be at least 1 hour")
                .LessThanOrEqualTo(168).WithMessage("Merchant edit window can't be more then 7 days (168 hours)");
        }
    }
}
