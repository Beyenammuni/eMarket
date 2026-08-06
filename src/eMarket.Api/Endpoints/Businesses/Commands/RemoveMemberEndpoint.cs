using eMarket.Application.Businesses.Commands.RemoveMember;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Commands;

public static class RemoveMemberEndpoint
{
    public static IEndpointRouteBuilder MapRemoveMemberEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapDelete(
            "/api/businesses/{businessId:guid}/members/{userId:guid}",
            async (
                Guid businessId,
                Guid userId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new RemoveMemberCommand(
                    businessId,
                    userId);

                var result = await sender.Send(command, cancellationToken);

                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("RemoveMember")
            .WithTags("Businesses");
            //.RequireAuthorization();

        return app;
    }
}
