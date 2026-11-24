using eMarket.Api.Common.Authorization;
using eMarket.Application.Businesses.Queries.GetCurrentSellerPlanSubscription;
using eMarket.Application.Common.Authorization;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Queries;

public static class GetCurrentSellerPlanSubscriptionEndpoint
{
    public static IEndpointRouteBuilder MapGetCurrentSellerPlanSubscriptionEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/businesses/{businessId:guid}/seller-plan-subscription/current",
            async (
                Guid businessId,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetCurrentSellerPlanSubscriptionQuery(businessId),
                    cancellationToken);

                return result is null
                    ? Results.NotFound()
                    : Results.Ok(result);
            })
            .RequireBusinessPermission(Permissions.Business.View)
            .WithName("GetCurrentSellerPlanSubscription")
            .WithTags("Seller Plans");

        return app;
    }
}
