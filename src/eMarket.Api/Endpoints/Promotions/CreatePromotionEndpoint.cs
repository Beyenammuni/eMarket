
using eMarket.Api.Common;
using eMarket.Api.Common.Authorization;
using eMarket.Application.Common.Authorization;
using eMarket.Application.Promotions.Commands.CreatePromotion;
using MediatR;

namespace eMarket.Api.Endpoints.Promotions;

public static class CreatePromotionEndpoint
{
    public static IEndpointRouteBuilder MapCreatePromotionEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/promotions",
            async (
                CreatePromotionCommand command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    command,
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Created(
                    $"/api/promotions/{result.Value.PromotionId}",
                    result.Value);
            })
            .WithName("CreatePromotion")
            .WithTags("Promotions")
            .RequireBusinessPermission(Permissions.Promotions.Create);

        return app;
    }
}
