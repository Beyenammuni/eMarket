
using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;
using eMarket.Application.Promotions.Queries.GetPromotions;
using MediatR;

namespace eMarket.Api.Endpoints.Promotions;

public static class GetPromotionsEndpoint
{
    public static IEndpointRouteBuilder MapGetPromotionsEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/promotions",
            async (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetPromotionsQuery(),
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("GetPromotions")
            .WithTags("Promotions")
            .RequireBusinessPermission(Permissions.Promotions.View);

        return app;
    }
}
