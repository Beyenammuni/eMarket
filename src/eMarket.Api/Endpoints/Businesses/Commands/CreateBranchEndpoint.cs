using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Businesses.Commands.CreateBranch;
using eMarket.Application.Common.Authorization;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Commands;

public static class CreateBranchEndpoint
{
    public static IEndpointRouteBuilder MapCreateBranchEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/businesses/{businessId:guid}/branches",
            async (
                Guid businessId,
                CreateBranchCommand request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = request with
                {
                    BusinessId = businessId
                };

                var result = await sender.Send(command, cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Created(
                    $"/api/businesses/{businessId}/branches/{result.Value}",
                    new { branchId = result.Value });
            })
            .WithName("CreateBranch")
            .WithTags("Branches")
            .RequireBusinessPermission(Permissions.Branches.Manage);

        return app;
    }
}
