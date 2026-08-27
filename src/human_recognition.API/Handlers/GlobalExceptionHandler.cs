using human_recognition.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace human_recognition.Api.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        _logger.LogError(exception, "Exception occured: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Instance = httpContext.Request.Path
        };

        // Add your error exception
        switch (exception)
        {
            // Validation error
            case FluentValidation.ValidationException validationException:
                problemDetails.Title = "Validation Failed";
                problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.1";
                problemDetails.Status = StatusCodes.Status400BadRequest;
                problemDetails.Detail = "One or more validation errors occurred";
                // Detail errors
                problemDetails.Extensions["errors"] = validationException.Errors
                    .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                    .ToDictionary(
                        failureGroup => failureGroup.Key,
                        failureGroup => failureGroup.ToArray()
                    );
                break;

            // Conflict error
            case ConflictException ex:
                problemDetails.Title = "Data Conflict";
                problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.8";
                problemDetails.Status = StatusCodes.Status409Conflict;
                problemDetails.Detail = ex.Message;
                break;

            // Unauthorized error
            case UnauthorizedException ex:
                problemDetails.Title = "Unauthorized";
                problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc7235#section-3.1";
                problemDetails.Status = StatusCodes.Status401Unauthorized;
                problemDetails.Detail = ex.Message;
                break;

            // Not found error
            case NotFoundException ex:
                problemDetails.Title = "Data Not Found";
                problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.4";
                problemDetails.Status = StatusCodes.Status404NotFound;
                problemDetails.Detail = ex.Message;
                break;

            // Add your custom exception here:

            ///
            /// Detault / internal server error
            default:
                problemDetails.Title = "Internal Server Error";
                problemDetails.Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.6.1";
                problemDetails.Status = StatusCodes.Status500InternalServerError;
                problemDetails.Detail = "Internal Server Error";
                break;
        }

        httpContext.Response.StatusCode = problemDetails.Status.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}