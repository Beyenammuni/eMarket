
using eMarket.Api.Common.Authorization;
using eMarket.Application.Businesses.Queries.GetSellerPlanFeatures;
using eMarket.Application.Common.Authorization;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Queries;

public static class GetSellerPlanFeaturesEndpoint
{
    public static IEndpointRouteBuilder MapGetSellerPlanFeaturesEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/businesses/{businessId:guid}/seller-plan-features",
            async (
                Guid businessId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetSellerPlanFeaturesQuery(businessId),
                    cancellationToken);

                return Results.Ok(result);
            })
            .RequireBusinessPermission(Permissions.Business.View)
            .WithName("GetSellerPlanFeatures")
            .WithTags("Seller Plans");

        return app;
    }
}
