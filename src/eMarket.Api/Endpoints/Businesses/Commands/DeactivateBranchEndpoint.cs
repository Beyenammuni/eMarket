using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Businesses.Branches.Commands.DeactivateBranch;
using eMarket.Application.Common.Authorization;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Commands;

public static class DeactivateBranchEndpoint
{
    public static IEndpointRouteBuilder MapDeactivateBranchEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/businesses/{businessId:guid}/branches/{branchId:guid}/deactivate",
            async (
                Guid businessId,
                Guid branchId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new DeactivateBranchCommand(
                    businessId,
                    branchId);

                var result = await sender.Send(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.NoContent();
            })
            .WithName("DeactivateBranch")
            .WithTags("Branches")
            .RequireBusinessPermission(Permissions.Branches.Manage);

        return app;
    }
}
