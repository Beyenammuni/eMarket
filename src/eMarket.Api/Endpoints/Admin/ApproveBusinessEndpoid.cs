using eMarket.Api.Common;
using eMarket.Application.Admin.Businesses.Commands.ApproveBusiness;
using eMarket.Application.Admin.Commands.ApproveBusiness;
using eMarket.SharedKernel.Constants;
using MediatR;

namespace eMarket.Api.Endpoints.Admin;

public static class ApproveBusinessEndpoint
{
    public static IEndpointRouteBuilder MapApproveBusinessEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/admin/businesses/{businessId:guid}/approve",
            async (
                Guid businessId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new ApproveBusinessCommand(businessId),
                    cancellationToken);

                return result.IsFailure
                    ? ApiResults.Failure(result.Error)
                    : Results.NoContent();
            })
            .WithName("ApproveBusiness")
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
