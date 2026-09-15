using eMarket.Api.Common;
using eMarket.Application.Businesses.Queries.GetBusinessById;
using eMarket.Application.Businesses.Queries.GetBusinessMembers;
using MediatR;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;

namespace eMarket.Api.Endpoints.Businesses.Queries;

public static class MapGetBusinessMembersEndpoint
{
    public static IEndpointRouteBuilder MapMapGetBusinessMembersEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/businesses/{businessId:guid}/Member",
            async (
                Guid businessId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetBusinessMembersQuery(businessId),
                    cancellationToken);

                return result.IsFailure
                    ? ApiResults.Failure(result.Error)
                    : Results.Ok(result.Value);
            })
            .WithName("GetBusinessMember")
            .WithTags("BusinessesMember")
             .RequireBusinessPermission(Permissions.Business.ManageMembers);

        return app;
    }
}
