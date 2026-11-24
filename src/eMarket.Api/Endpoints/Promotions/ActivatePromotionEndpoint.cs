using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;
using eMarket.Application.Promotions.Commands.ActivatePromotion;
using MediatR;

namespace eMarket.Api.Endpoints.Promotions;

public static class ActivatePromotionEndpoint
{
    public static IEndpointRouteBuilder MapActivatePromotionEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/promotions/{promotionId:guid}/activate",
            async (
                Guid promotionId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new ActivatePromotionCommand(promotionId);

                var result = await sender.Send(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("ActivatePromotion")
            .WithTags("Promotions")
            .RequireBusinessPermission(Permissions.Promotions.Activate);

        return app;
    }
}
