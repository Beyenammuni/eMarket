
using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;
using eMarket.Application.Promotions.Commands.CancelPromotion;
using MediatR;

namespace eMarket.Api.Endpoints.Promotions;

public static class CancelPromotionEndpoint
{
    public static IEndpointRouteBuilder MapCancelPromotionEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/promotions/{promotionId:guid}/cancel",
            async (
                Guid promotionId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new CancelPromotionCommand(promotionId);

                var result = await sender.Send(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("CancelPromotion")
            .WithTags("Promotions")
            .RequireBusinessPermission(Permissions.Promotions.Delete);

        return app;
    }
}
