using eMarket.Application.Businesses.Commands;
using eMarket.Application.Businesses.Commands.CreateBusiness;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses;

public static class CreateBusinessEndpoint
{
    public static IEndpointRouteBuilder MapCreateBusinessEndpoint(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/businesses",
            async (
                CreateBusinessCommand command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(command, cancellationToken);

                if (result.IsFailure)
                {
                    return Results.BadRequest(result.Error);
                }

                return Results.Created(
                    $"/api/businesses/{result.Value.Id}",
                    result.Value);
            })
            .WithName("CreateBusiness")
            .WithTags("Businesses");
        return app;
    }
}
