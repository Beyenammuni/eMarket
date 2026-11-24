
using eMarket.Application.Businesses.Queries.GetBusinessActivityLogs;
using MediatR;

namespace eMarket.Api.Endpoints.Businesses.Queries;

public static class MapGetBusinessActivityLogsEndpoint
{
    public static IEndpointRouteBuilder MapGetBusinessActivityLogs(
        this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/api/businesses/{businessId:guid}/activity-logs",
            async (
                Guid businessId,
                int page,
                int pageSize,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetBusinessActivityLogsQuery(
                        businessId,
                        page,
                        pageSize),
                    cancellationToken);

                return result.IsFailure
                    ? Results.Forbid()
                    : Results.Ok(result.Value);
            })
            .WithName("GetBusinessActivityLogs")
            .WithTags("Businesses");

        return app;
    }
}
