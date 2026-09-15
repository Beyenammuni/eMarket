using eMarket.Api.Common;
using eMarket.Application.Admin.Commands.CloseBusiness;
using eMarket.SharedKernel.Constants;
using MediatR;

namespace eMarket.Api.Endpoints.Admin;

public static class CloseBusinessEndpoint
{
    public static IEndpointRouteBuilder MapCloseBusinessEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/admin/businesses/{businessId:guid}/close",
            async (
                Guid businessId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new CloseBusinessCommand(businessId),
                    cancellationToken);

                return result.IsFailure
                    ? ApiResults.Failure(result.Error)
                    : Results.NoContent();
            })
            .WithName("CloseBusiness")
            .WithTags("Admin Businesses")
            .RequireAuthorization(policy =>
                policy.RequireRole(
                    Roles.Admin,
                    Roles.SuperAdmin))
            .Produces(204)
            .Produces(401)
            .Produces(403)
            .Produces(404);

        return app;
    }
}
