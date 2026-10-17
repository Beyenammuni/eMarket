
using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Businesses.Commands.ActivateBusiness;
using eMarket.Application.Common.Authorization;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Commands;

public static class ActivateBusinessEndpoint
{
    public static IEndpointRouteBuilder MapActivateBusinessEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/businesses/{businessId:guid}/activate",
            async (
                Guid businessId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new ActivateBusinessCommand(businessId),
                    cancellationToken);

                return result.IsFailure
                    ? ApiResults.Failure(result.Error)
                    : Results.NoContent();
            })
            .WithName("ActivateBusiness")
            .WithTags("Businesses")
            .RequireBusinessPermission(Permissions.Business.Activate)
            .Produces(204)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        return app;
    }
}
