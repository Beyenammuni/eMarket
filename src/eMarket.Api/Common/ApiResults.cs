using System.Net;
using eMarket.SharedKernel.Results;
using Microsoft.AspNetCore.Mvc;

namespace eMarket.Api.Common;

public static class ApiResults
{
    public static IResult From(Result result, Func<IResult>? onSuccess = null)
        => result.IsSuccess
            ? onSuccess?.Invoke() ?? Results.Ok()
            : Failure(result.Error);

    public static IResult From<T>(Result<T> result, Func<T, IResult>? onSuccess = null)
        => result.IsSuccess
            ? onSuccess?.Invoke(result.Value!) ?? Results.Ok(result.Value)
            : Failure(result.Error);

    public static IResult Failure(Error error)
    {
        var status = GetStatusCode(error.Code);

        return Results.Problem(
            statusCode: status,
            title: GetTitle(status),
            detail: error.Description,
            extensions: new Dictionary<string, object?>
            {
                ["errorCode"] = error.Code
            });
    }

    public static async Task WriteProblemAsync(
        HttpResponse response,
        int statusCode,
        string title,
        string detail,
        string? errorCode = null,
        CancellationToken cancellationToken = default)
    {
        response.StatusCode = statusCode;
        response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail
        };

        if (!string.IsNullOrWhiteSpace(errorCode))
            problem.Extensions["errorCode"] = errorCode;

        problem.Extensions["traceId"] = response.HttpContext.TraceIdentifier;

        await response.WriteAsJsonAsync(problem, cancellationToken);
    }

    public static IResult Unauthorized() => Results.Problem(
        statusCode: StatusCodes.Status401Unauthorized,
        title: "Unauthorized",
        detail: "Authentication is required.");

    public static IResult Forbidden() => Results.Problem(
        statusCode: StatusCodes.Status403Forbidden,
        title: "Forbidden",
        detail: "You do not have permission to perform this operation.");

    public static int GetStatusCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return StatusCodes.Status400BadRequest;

        if (code.Contains("NotFound", StringComparison.OrdinalIgnoreCase) ||
            code.EndsWith(".NotFound", StringComparison.OrdinalIgnoreCase))
            return StatusCodes.Status404NotFound;

        if (code.Contains("Forbidden", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("AccessDenied", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("InsufficientPermissions", StringComparison.OrdinalIgnoreCase))
            return StatusCodes.Status403Forbidden;

        if (code.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("InvalidCredentials", StringComparison.OrdinalIgnoreCase))
            return StatusCodes.Status401Unauthorized;

        if (code.Contains("AlreadyExists", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("Duplicate", StringComparison.OrdinalIgnoreCase) ||
            code.EndsWith(".Exists", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("AlreadyActive", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("AlreadyInactive", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("AlreadyDeleted", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("SameName", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("SamePrice", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("SamePassword", StringComparison.OrdinalIgnoreCase) ||
            code.Contains("Conflict", StringComparison.OrdinalIgnoreCase))
            return StatusCodes.Status409Conflict;

        return StatusCodes.Status400BadRequest;
    }

    private static string GetTitle(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Bad Request",
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status403Forbidden => "Forbidden",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        StatusCodes.Status422UnprocessableEntity => "Unprocessable Entity",
        _ => ((HttpStatusCode)statusCode).ToString()
    };
}
