using eMarket.Api.Common;
using eMarket.Application.Businesses.Queries.GetAvailableBusinessMembers;
using MediatR;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;

namespace eMarket.Api.Endpoints.Businesses.Queries;

public static class MapGetAvailableBusinessMembersEndpoint
{
    public static IEndpointRouteBuilder MapGetAvailableBusinessMembers(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/businesses/{businessId:guid}/available-members",
            async (
                Guid businessId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetAvailableBusinessMembersQuery(businessId),
                    cancellationToken);

                return result.IsFailure
                    ? ApiResults.Failure(result.Error)
                    : Results.Ok(result.Value);
            })
            .WithName("GetAvailableBusinessMembers")
            .WithTags("BusinessesMember")
            .RequireBusinessPermission(
                Permissions.Business.ManageMembers);

        return app;
    }
}
