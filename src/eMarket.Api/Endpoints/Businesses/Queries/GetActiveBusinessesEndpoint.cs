using eMarket.Api.Common;
using eMarket.Application.Businesses.Queries.GetActiveBusinesses;
using MediatR;

public static class GetActiveBusinessesEndpoint
{
    public static IEndpointRouteBuilder MapGetActiveBusinessesEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/businesses/active",
            async (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetActiveBusinessesQuery(),
                    cancellationToken);

                if (result.IsFailure)
                    return ApiResults.Failure(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("GetActiveBusinesses")
            .WithTags("Businesses")
            .RequireAuthorization();

        return app;
    }
}
