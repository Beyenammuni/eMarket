using System.Text.Json;
using System.Linq;
using FluentValidation;
using eMarket.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace eMarket.Api.Common;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = httpContext.TraceIdentifier;
        if (exception is BadHttpRequestException || exception is eMarket.SharedKernel.Exceptions.ValidationException)
            _logger.LogWarning(exception, "Request failed. TraceId: {TraceId}", traceId);
        else
            _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", traceId);

        var response = CreateResponse(httpContext, exception, traceId);

        httpContext.Response.StatusCode = response.Status!.Value;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken);

        return true;
    }

    private ProblemDetails CreateResponse(HttpContext httpContext, Exception exception, string traceId)
    {
        var response = exception switch
        {
            FluentValidation.ValidationException validation =>
                ValidationResponse(validation),
            eMarket.SharedKernel.Exceptions.ValidationException validation =>
                ValidationResponse(validation),
            SharedKernel.Exceptions.ConcurrencyException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = "The resource was modified by another request."
            },
            NotFoundException => new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = _environment.IsDevelopment() ? exception.Message : "The requested resource was not found."
            },
            DomainException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Business rule violation",
                Detail = _environment.IsDevelopment() ? exception.Message : "The operation violates a business rule."
            },
            BadHttpRequestException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = _environment.IsDevelopment() ? exception.Message : "The request is invalid."
            },
            JsonException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid JSON",
                Detail = "The request body contains invalid JSON."
            },
            ArgumentException => new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Bad Request",
                Detail = _environment.IsDevelopment() ? exception.Message : "The request is invalid."
            },
            DbUpdateConcurrencyException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = "The resource was modified by another request."
            },
            DbUpdateException => new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Database conflict",
                Detail = "The operation could not be completed because of a data conflict."
            },
            OperationCanceledException when httpContext.RequestAborted.IsCancellationRequested => new ProblemDetails
            {
                Status = 499,
                Title = "Client Closed Request",
                Detail = "The request was cancelled by the client."
            },
            _ => new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred."
            }
        };

        response.Extensions["traceId"] = traceId;
        return response;
    }

    private static ProblemDetails ValidationResponse(FluentValidation.ValidationException exception)
    {
        var response = new ValidationProblemDetails(
            exception.Errors
                .GroupBy(x => string.IsNullOrWhiteSpace(x.PropertyName) ? "request" : x.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.ErrorMessage).Distinct().ToArray()))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Detail = "One or more validation errors occurred."
        };

        response.Extensions["errorCode"] = "Validation.Failed";
        return response;
    }

    private static ProblemDetails ValidationResponse(
        eMarket.SharedKernel.Exceptions.ValidationException exception)
    {
        var response = new ValidationProblemDetails(
            exception.Errors.ToDictionary(kv => kv.Key, kv => kv.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed",
            Detail = exception.Message
        };

        response.Extensions["errorCode"] = "Validation.Failed";
        return response;
    }
}
