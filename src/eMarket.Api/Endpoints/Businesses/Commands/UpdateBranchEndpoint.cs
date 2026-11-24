using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Businesses.Commands.UpdateBranch;
using eMarket.Application.Common.Authorization;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Commands;

public static class UpdateBranchEndpoint
{
    public static IEndpointRouteBuilder MapUpdateBranchEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/businesses/{businessId:guid}/branches/{branchId:guid}",
            async (
                Guid businessId,
                Guid branchId,
                UpdateBranchCommand request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = request with
                {
                    BusinessId = businessId,
                    BranchId = branchId
                };

                var result = await sender.Send(command, cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.NoContent();
            })
            .WithName("UpdateBranch")
            .WithTags("Branches")
            .RequireBusinessPermission(Permissions.Branches.Manage);

        return app;
    }
}
