using eMarket.Api.Common;
using eMarket.Application.Identity.Auth.Commands.ChangePassword;
using eMarket.Application.Identity.Auth.Commands.ForgotPassword;
using eMarket.Application.Identity.Auth.Commands.Login;
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
        group.MapPost("/forgot-password", ForgotPassword);
        group.MapPost("/reset-password", ResetPassword);
        group.MapPost("/change-password", ChangePassword).RequireAuthorization();

        return app;
    }

    private static async Task<IResult> Register(RegisterCommand command, ISender sender, CancellationToken cancellationToken)
        => ToResult(await sender.Send(command, cancellationToken));

    private static async Task<IResult> Login(LoginCommand command, ISender sender, CancellationToken cancellationToken)
        => ToResult(await sender.Send(command, cancellationToken));

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
