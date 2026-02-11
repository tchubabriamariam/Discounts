using System.Net;
using System.Text.Json;
using Discounts.Application.Exceptions;

namespace Discounts.API.Infrustructure.Middlewares;


public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            // 400 - Bad Request
            ValidationException validationEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = validationEx.Message,
                Details = validationEx.Errors != null ? JsonSerializer.Serialize(validationEx.Errors) : null
            },
            FluentValidation.ValidationException fluentEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Validation failed",
                Details = JsonSerializer.Serialize(fluentEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }))
            },
            BusinessRuleViolationException businessEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = businessEx.Message
            },
            InvalidOperationException invalidOpEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = invalidOpEx.Message
            },
            ArgumentException argEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = argEx.Message
            },

            // 401 - Unauthorized
            UnauthorizedException unauthorizedEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = unauthorizedEx.Message
            },
            UnauthorizedAccessException unauthorizedAccessEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = unauthorizedAccessEx.Message
            },

            // 403 - Forbidden
            ForbiddenException forbiddenEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Forbidden,
                Message = forbiddenEx.Message
            },

            // 404 - Not Found
            NotFoundException notFoundEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = notFoundEx.Message
            },

            // 409 - Conflict
            AlreadyExistsException existsEx => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Conflict,
                Message = existsEx.Message
            },

            // 500 - Internal Server Error
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An unexpected error occurred. Please try again later.",
                Details = context.Request.Host.Host == "localhost" ? exception.Message : null 
            }
        };

        _logger.LogError(
            "Exception Type: {ExceptionType}, Status Code: {StatusCode}, Message: {Message}",
            exception.GetType().Name,
            errorResponse.StatusCode,
            errorResponse.Message);

        context.Response.StatusCode = errorResponse.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(errorResponse, jsonOptions));
    }
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
