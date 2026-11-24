
using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;
using eMarket.Application.Promotions.Commands.UpdatePromotion;
using MediatR;

namespace eMarket.Api.Endpoints.Promotions;

public static class UpdatePromotionEndpoint
{
    public static IEndpointRouteBuilder MapUpdatePromotionEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPut(
            "/api/promotions/{promotionId:guid}",
            async (
                Guid promotionId,
                UpdatePromotionRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new UpdatePromotionCommand(
                    promotionId,
                    request.Name,
                    request.Description,
                    request.StartsAtUtc,
                    request.EndsAtUtc,
                    request.Items);

                var result = await sender.Send(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("UpdatePromotion")
            .WithTags("Promotions")
            .RequireBusinessPermission(Permissions.Promotions.Update);

        return app;
    }
}

public sealed record UpdatePromotionRequest(
    string Name,
    string? Description,
    DateTime StartsAtUtc,
    DateTime EndsAtUtc,
    IReadOnlyCollection<UpdatePromotionItemRequest> Items);
