
using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;
using eMarket.Application.Promotions.Commands.PausePromotion;
using MediatR;

namespace eMarket.Api.Endpoints.Promotions;

public static class PausePromotionEndpoint
{
    public static IEndpointRouteBuilder MapPausePromotionEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/promotions/{promotionId:guid}/pause",
            async (
                Guid promotionId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new PausePromotionCommand(promotionId);

                var result = await sender.Send(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("PausePromotion")
            .WithTags("Promotions")
            .RequireBusinessPermission(Permissions.Promotions.Pause);

        return app;
    }
}
