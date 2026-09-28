using eMarket.Application.Identity.Users.GetUsers;
using eMarket.Application.Identity.Users.Queries.GetUserById;
using MediatR;

namespace eMarket.Api.Endpoints.Auth;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app
            .MapGroup("/api/users")
            .WithTags("Users")
            .RequireAuthorization();

        group.MapGet("/", GetUsers);
        group.MapGet("/{userId:guid}", GetUser);

        return app;
    }

    private static async Task<IResult> GetUsers(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetUsersQuery(),
            cancellationToken);

        return result.IsFailure
            ? Results.BadRequest(result.Error)
            : Results.Ok(result.Value);
    }

    private static async Task<IResult> GetUser(
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetUserQuery(userId),
            cancellationToken);

        return result.IsFailure
            ? Results.NotFound(result.Error)
            : Results.Ok(result.Value);
    }
}
