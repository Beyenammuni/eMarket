using eMarket.Application.Businesses.Queries.GetMyBusinesses;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Queries;

public static class GetMyBusinessesEndpoint
{
    public static IEndpointRouteBuilder MapGetMyBusinessesEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/businesses/my",
            async (
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetMyBusinessesQuery(),
                    cancellationToken);

                if (result.IsFailure)
                    return Results.BadRequest(result.Error);

                return Results.Ok(result.Value);
            })
            .WithName("GetMyBusinesses")
            .WithTags("Businesses");
            //.RequireAuthorization();

        return app;
    }
}
