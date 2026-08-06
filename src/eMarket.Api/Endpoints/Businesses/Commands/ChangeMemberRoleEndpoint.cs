using eMarket.Application.Businesses.Commands.ChangeMemberRole;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Commands;

public static class ChangeMemberRoleEndpoint
{
    public static IEndpointRouteBuilder MapChangeMemberRoleEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/businesses/{businessId:guid}/members/{userId:guid}/role",
            async (
                Guid businessId,
                Guid userId,
                ChangeMemberRoleRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new ChangeMemberRoleCommand(
                    businessId,
                    userId,
                    request.Role);

                var result = await sender.Send(command, cancellationToken);

                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("ChangeMemberRole")
            .WithTags("Businesses");
            //.RequireAuthorization();

        return app;
    }
}
