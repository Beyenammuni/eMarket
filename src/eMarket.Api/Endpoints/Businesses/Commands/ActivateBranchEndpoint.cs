using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Businesses.Branches.Commands.ActivateBranch;
using eMarket.Application.Common.Authorization;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Commands;

public static class ActivateBranchEndpoint
{
    public static IEndpointRouteBuilder MapActivateBranchEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/businesses/{businessId:guid}/branches/{branchId:guid}/activate",
            async (
                Guid businessId,
                Guid branchId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new ActivateBranchCommand(
                    businessId,
                    branchId);

                var result = await sender.Send(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.NoContent();
            })
            .WithName("ActivateBranch")
            .WithTags("Branches")
            .RequireBusinessPermission(Permissions.Branches.Manage);

        return app;
    }
}
