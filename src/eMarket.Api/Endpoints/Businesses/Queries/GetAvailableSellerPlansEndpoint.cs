using eMarket.Application.Businesses.Queries.GetSellerPlans;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Queries;

public static class GetAvailableSellerPlansEndpoint
{
    public static IEndpointRouteBuilder MapGetAvailableSellerPlansEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/seller-plans",
            async (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetSellerPlansQuery(true),
                    cancellationToken);

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetAvailableSellerPlans")
            .WithTags("Seller Plans");

        return app;
    }
}
