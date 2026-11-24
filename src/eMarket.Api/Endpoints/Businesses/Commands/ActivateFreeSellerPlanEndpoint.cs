using eMarket.Application.Businesses.Commands.ActivateFreeSellerPlan;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Commands;

public static class ActivateFreeSellerPlanEndpoint
{
    public static IEndpointRouteBuilder MapActivateFreeSellerPlanEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost(
            "/api/businesses/{businessId:guid}/seller-plan-subscription/free",
            async (
                Guid businessId,
                ActivateFreeSellerPlanRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new ActivateFreeSellerPlanCommand(
                    businessId,
                    request.SellerPlanId);

                var subscriptionId = await sender.Send(
                    command,
                    cancellationToken);

                return Results.Ok(new { subscriptionId });
            })
            .RequireAuthorization()
            .WithName("ActivateFreeSellerPlan")
            .WithTags("Seller Plans");

        return app;
    }
}

public sealed record ActivateFreeSellerPlanRequest(Guid SellerPlanId);
