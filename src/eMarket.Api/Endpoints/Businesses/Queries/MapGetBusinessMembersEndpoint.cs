using eMarket.Application.Businesses.Queries.GetBusinessById;
using eMarket.Application.Businesses.Queries.GetBusinessMembers;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Queries;

public static class MapGetBusinessMembersEndpoint
{
    public static IEndpointRouteBuilder MapMapGetBusinessMembersEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/businesses/Member",
            async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetBusinessMembersQuery(id),
                    cancellationToken);

                return result.IsFailure
                    ? Results.NotFound(result.Error)
                    : Results.Ok(result.Value);
            })
            .WithName("GetBusinessMember")
            .WithTags("BusinessesMember");
            //.RequireAuthorization();

        return app;
    }
}
