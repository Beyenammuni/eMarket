
using eMarket.Api.Common;
using eMarket.Application.Promotions.Queries.GetActivePromotions;
using MediatR;

namespace eMarket.Api.Endpoints.Promotions;

public static class GetActivePromotionsEndpoint
{
    public static IEndpointRouteBuilder MapGetActivePromotionsEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/promotions/active",
            async (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetActivePromotionsQuery(),
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("GetActivePromotions")
            .WithTags("Promotions")
            .AllowAnonymous();

        return app;
    }
}
