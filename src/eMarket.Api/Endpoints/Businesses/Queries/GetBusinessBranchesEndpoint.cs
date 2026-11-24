using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Businesses.Queries.GetBusinessBranches;
using eMarket.Application.Common.Authorization;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Queries;

public static class GetBusinessBranchesEndpoint
{
    public static IEndpointRouteBuilder MapGetBusinessBranchesEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/businesses/{businessId:guid}/branches",
            async (
                Guid businessId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new GetBusinessBranchesQuery(businessId);

                var result = await sender.Send(query, cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("GetBusinessBranches")
            .WithTags("Branches")
            .RequireBusinessPermission(Permissions.Branches.View);

        return app;
    }
}
