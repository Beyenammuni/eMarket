
using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;
using eMarket.Application.Promotions.Queries.GetPromotionById;
using MediatR;

namespace eMarket.Api.Endpoints.Promotions;

public static class GetPromotionByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetPromotionByIdEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/promotions/{promotionId:guid}",
            async (
                Guid promotionId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetPromotionByIdQuery(promotionId),
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("GetPromotionById")
            .WithTags("Promotions")
            .RequireBusinessPermission(Permissions.Promotions.View);

        return app;
    }
}
