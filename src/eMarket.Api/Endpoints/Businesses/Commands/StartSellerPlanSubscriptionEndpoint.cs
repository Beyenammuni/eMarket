
using eMarket.Application.Businesses.Commands.StartSellerPlanSubscription;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Commands;

public static class StartSellerPlanSubscriptionEndpoint
{
    public static IEndpointRouteBuilder MapStartSellerPlanSubscriptionEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/businesses/{businessId:guid}/seller-plan-subscription",
            async (
                Guid businessId,
                StartSellerPlanSubscriptionRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new StartSellerPlanSubscriptionCommand(
                    businessId,
                    request.SellerPlanId,
                    request.ReturnUrl);

                var result = await sender.Send(command, cancellationToken);

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("StartSellerPlanSubscription")
            .WithTags("Seller Plans");

        return app;
    }
}

public sealed record StartSellerPlanSubscriptionRequest(
    Guid SellerPlanId,
    string ReturnUrl);
