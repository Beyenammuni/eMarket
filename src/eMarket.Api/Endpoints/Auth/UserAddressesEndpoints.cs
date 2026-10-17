using eMarket.Application.Identity.Addresses.Queries;
using eMarket.Application.Identity.Auth.Commands;
using MediatR;

namespace eMarket.Api.Endpoints.Auth;

public static class UserAddressesEndpoints
{
    public static IEndpointRouteBuilder MapUserAddressesEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/users/addresses")
            .WithTags("User Addresses")
            .RequireAuthorization();

        group.MapGet("/", GetAddresses);
        group.MapPost("/", CreateAddress);
        return app;
    }

    private static async Task<IResult> GetAddresses(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetUserAddressesQuery(),
            cancellationToken);

        return result.IsFailure
            ? Results.BadRequest(result.Error)
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> CreateAddress(
        CreateUserAddressCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            command,
            cancellationToken);

        return result.IsFailure
            ? Results.BadRequest(result.Error)
            : Results.Ok();
    }
}
