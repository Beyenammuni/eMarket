using eMarket.Api.Common;
using eMarket.Application.Businesses.Queries.GetBusinessById;
using MediatR;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;

namespace eMarket.Api.Endpoints.Businesses.Queries;

public static class GetBusinessByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetBusinessByIdEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/businesses/{businessId:guid}",
            async (
                Guid businessId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetBusinessByIdQuery(businessId),
                    cancellationToken);

                return result.IsFailure
                    ? ApiResults.Failure(result.Error)
                    : Results.Ok(result.Value);
            })
            .WithName("GetBusinessById")
            .WithTags("Businesses")
             .RequireBusinessPermission(Permissions.Business.View);

        return app;
    }
}
