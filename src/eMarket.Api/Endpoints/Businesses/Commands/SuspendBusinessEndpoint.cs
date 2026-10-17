
using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Businesses.Commands.SuspendBusiness;
using eMarket.Application.Common.Authorization;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Commands;

public static class SuspendBusinessEndpoint
{
    public static IEndpointRouteBuilder MapSuspendBusinessEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/businesses/{businessId:guid}/suspend",
            async (
                Guid businessId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new SuspendBusinessCommand(businessId),
                    cancellationToken);

                return result.IsFailure
                    ? ApiResults.Failure(result.Error)
                    : Results.NoContent();
            })
            .WithName("SuspendBusiness")
            .WithTags("Businesses")
            .RequireBusinessPermission(Permissions.Business.Suspend)
            .Produces(204)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        return app;
    }
}
