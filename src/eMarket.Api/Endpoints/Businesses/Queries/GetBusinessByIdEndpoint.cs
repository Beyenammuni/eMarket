using eMarket.Application.Businesses.Queries.GetBusinessById;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Queries;

public static class GetBusinessByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetBusinessByIdEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/businesses/{id:guid}",
            async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetBusinessByIdQuery(id),
                    cancellationToken);

                return result.IsFailure
                    ? Results.NotFound(result.Error)
                    : Results.Ok(result.Value);
            })
            .WithName("GetBusinessById")
            .WithTags("Businesses");
            //.RequireAuthorization();

        return app;
    }
}
