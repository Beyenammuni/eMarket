namespace eMarket.Api.Endpoints.Businesses;

public static class BusinessEndpoints
{
    public static IEndpointRouteBuilder MapBusinessEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapCreateBusinessEndpoint();
        // app.MapUpdateBusinessEndpoint();
        // app.MapDeleteBusinessEndpoint();
        // app.MapGetBusinessEndpoint();

        return app;
    }
}
