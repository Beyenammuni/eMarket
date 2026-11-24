using eMarket.Application.Businesses.Queries.GetSellerPlanSubscriptions;
using eMarket.Application.Businesses.Queries.GetSellerPlanPayments;
using eMarket.SharedKernel.Constants;
using MediatR;

namespace eMarket.Api.Endpoints.Admin;

public static class SellerPlanManagementEndpoints
{
    public static IEndpointRouteBuilder MapSellerPlanManagementEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/admin/seller-plan-management")
            .RequireAuthorization(policy =>
                policy.RequireRole(Roles.SuperAdmin));

        group.MapGet("/subscriptions", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetSellerPlanSubscriptionsQuery(),
                cancellationToken);

            return Results.Ok(result);
        });

        group.MapGet("/payments", async (
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var result = await sender.Send(
                new GetSellerPlanPaymentsQuery(),
                cancellationToken);

            return Results.Ok(result);
        });

        return app;
    }
}
