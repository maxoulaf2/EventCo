using EventCo.Domain.Common;
using EventCo.Domain.Events.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;

namespace EventCo.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, problemDetails) = exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                new ValidationProblemDetails(
                    validationException.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray()))),
            EventNotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                new ProblemDetails { Title = "Ressource introuvable", Detail = notFoundException.Message }),
            DomainException domainException => (
                StatusCodes.Status400BadRequest,
                new ProblemDetails { Title = "Règle métier violée", Detail = domainException.Message }),
            _ => (0, null as ProblemDetails),
        };

        if (problemDetails is null)
            return false;

        problemDetails.Status = statusCode;
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(), cancellationToken);

        return true;
    }
}
