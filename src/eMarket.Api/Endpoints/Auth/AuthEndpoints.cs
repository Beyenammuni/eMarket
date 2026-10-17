using eMarket.Api.Common;
using eMarket.Application.Identity.Auth.Commands.ChangePassword;
using eMarket.Application.Identity.Auth.Commands.ForgotPassword;
using eMarket.Application.Identity.Auth.Commands.Login;
using eMarket.Application.Identity.Auth.Commands.Logout;
using eMarket.Application.Identity.Auth.Commands.RefreshToken;
using eMarket.Application.Identity.Auth.Commands.Register;
using eMarket.Application.Identity.Auth.Commands.ResetPassword;
using MediatR;

namespace eMarket.Api.Endpoints.Identity;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Authentication").RequireRateLimiting("auth");

        group.MapPost("/register", Register);
        group.MapPost("/login", Login);
        group.MapPost("/refresh", Refresh);
        group.MapPost("/logout", Logout);
        group.MapPost("/forgot-password", ForgotPassword);
        group.MapPost("/reset-password", ResetPassword);
        group.MapPost("/change-password", ChangePassword).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> Register(RegisterCommand command, ISender sender, CancellationToken cancellationToken)
        => ToResult(await sender.Send(command, cancellationToken));


    private static async Task<IResult> Login(
        LoginCommand command,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result.IsFailure)
            return ApiResults.Failure(result.Error);

        var login = result.Value!;

        httpContext.Response.Cookies.Append(
            "pazora_refresh_token",
            login.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/api/auth"
            });

        return Results.Ok(new
        {
            login.UserId,
            login.Email,
            login.AccessToken
        });
    }

    private static async Task<IResult> Refresh(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!httpContext.Request.Cookies.TryGetValue(
                "pazora_refresh_token",
                out var refreshToken) ||
            string.IsNullOrWhiteSpace(refreshToken))
        {
            return Results.Unauthorized();
        }

        var result = await sender.Send(
            new RefreshTokenCommand(refreshToken),
            cancellationToken);

        if (result.IsFailure)
        {
            httpContext.Response.Cookies.Delete(
                "pazora_refresh_token",
                new CookieOptions
                {
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/api/auth"
                });

            return Results.Unauthorized();
        }

        var refreshed = result.Value!;

        httpContext.Response.Cookies.Append(
            "pazora_refresh_token",
            refreshed.RefreshToken,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                Path = "/api/auth"
            });

        return Results.Ok(new
        {
            refreshed.UserId,
            refreshed.Email,
            refreshed.AccessToken
        });
    }


    private static async Task<IResult> Logout(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        httpContext.Request.Cookies.TryGetValue(
            "pazora_refresh_token",
            out var refreshToken);

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await sender.Send(
                new LogoutCommand(refreshToken),
                cancellationToken);
        }

        httpContext.Response.Cookies.Delete(
            "pazora_refresh_token",
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Lax,
                Path = "/api/auth"
            });

        return Results.NoContent();
    }

    private static async Task<IResult> ForgotPassword(ForgotPasswordCommand command, ISender sender, CancellationToken cancellationToken)
        => ToResult(await sender.Send(command, cancellationToken));

    private static async Task<IResult> ResetPassword(ResetPasswordCommand command, ISender sender, CancellationToken cancellationToken)
        => ToResult(await sender.Send(command, cancellationToken));

    private static async Task<IResult> ChangePassword(ChangePasswordCommand command, ISender sender, CancellationToken cancellationToken)
        => ToResult(await sender.Send(command, cancellationToken));

    private static IResult ToResult(eMarket.SharedKernel.Results.Result result)
        => result.IsFailure ? ApiResults.Failure(result.Error) : Results.Ok();

    private static IResult ToResult<T>(eMarket.SharedKernel.Results.Result<T> result)
        => result.IsFailure ? ApiResults.Failure(result.Error) : Results.Ok(result.Value);
}
