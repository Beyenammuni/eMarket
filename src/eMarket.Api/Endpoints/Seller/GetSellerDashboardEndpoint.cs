using eMarket.Api.Common;
using eMarket.Application.Businesses.Queries.GetSellerDashboard;
using eMarket.SharedKernel.Constants;
using MediatR;

namespace eMarket.Api.Endpoints.Seller;

public static class GetSellerDashboardEndpoint
{
    public static IEndpointRouteBuilder MapGetSellerDashboardEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/seller/dashboard",
            async (
                int? days,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var query = new GetSellerDashboardQuery(
                    days ?? 30);

                var result = await sender.Send(
                    query,
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("GetSellerDashboard")
            .WithTags("Seller Dashboard")
            .RequireAuthorization(policy =>
                policy.RequireRole(
                    Roles.Seller,
                    Roles.Admin,
                    Roles.SuperAdmin));

        return app;
    }
}
