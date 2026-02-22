// Copyright (C) TBC Bank. All Rights Reserved.

namespace Discounts.Application.Exceptions
{
    // Base exception for the application
    public class DiscountsException : Exception
    {
        public DiscountsException(string message) : base(message)
        {
        }

        public DiscountsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    // 404 Not Found
    public class NotFoundException : DiscountsException
    {
        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string entityName, object key)
            : base($"{entityName} with identifier '{key}' was not found.")
        {
        }
    }

    // 409 Conflict
    public class AlreadyExistsException : DiscountsException
    {
        public AlreadyExistsException(string message) : base(message)
        {
        }

        public AlreadyExistsException(string entityName, string fieldName, object value)
            : base($"{entityName} with {fieldName} '{value}' already exists.")
        {
        }
    }

    // 401 Unauthorized
    public class UnauthorizedException : DiscountsException
    {
        public UnauthorizedException(string message) : base(message)
        {
        }
    }

    // 403 Forbidden
    public class ForbiddenException : DiscountsException
    {
        public ForbiddenException(string message) : base(message)
        {
        }
    }

    // 400 Bad Request , Generic validation
    public class ValidationException : DiscountsException
    {
        public ValidationException(string message) : base(message)
        {
        }

        public Dictionary<string, string[]>? Errors { get; set; }
    }

    // 400 Business rule violations
    public class BusinessRuleViolationException : DiscountsException
    {
        public BusinessRuleViolationException(string message) : base(message)
        {
        }
    }

    public class InsufficientBalanceException : BusinessRuleViolationException
    {
        public InsufficientBalanceException(decimal required, decimal available)
            : base($"Insufficient balance. Required: {required:C}, Available: {available:C}")
        {
        }
    }

    public class ReservationExpiredException : BusinessRuleViolationException
    {
        public ReservationExpiredException()
            : base("Reservation has expired. Please create a new reservation.")
        {
        }
    }

    public class OfferNotAvailableException : BusinessRuleViolationException
    {
        public OfferNotAvailableException(string reason)
            : base($"This offer is not available: {reason}")
        {
        }
    }

    public class InsufficientCouponsException : BusinessRuleViolationException
    {
        public InsufficientCouponsException(int remaining, int requested)
            : base($"Only {remaining} coupons remaining. Cannot reserve {requested}.")
        {
        }
    }

    public class DuplicateReservationException : BusinessRuleViolationException
    {
        public DuplicateReservationException()
            : base("You already have an active reservation for this offer. Please complete or cancel it first.")
        {
        }
    }

    public class EditWindowExpiredException : BusinessRuleViolationException
    {
        public EditWindowExpiredException(int hours)
            : base($"Edit window has expired. Offers can only be edited within {hours} hours of creation.")
        {
        }
    }

    public class InvalidOfferStatusException : BusinessRuleViolationException
    {
        public InvalidOfferStatusException(string currentStatus, string expectedStatus)
            : base($"Offer is {currentStatus}. Expected: {expectedStatus}")
        {
        }
    }

    public class AccountInactiveException : UnauthorizedException
    {
        public AccountInactiveException()
            : base("Your account has been deactivated.")
        {
        }
    }

    public class UserRegistrationException : DiscountsException
    {
        public UserRegistrationException(string message) : base(message)
        {
        }
    }
}
