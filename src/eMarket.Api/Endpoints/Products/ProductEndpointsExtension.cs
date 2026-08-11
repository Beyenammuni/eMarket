namespace eMarket.Api.Endpoints.Products;

public static class ProductEndpointsExtension{
    public static IEndpointRouteBuilder MapEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapProductEndpoints();

        return app;
    }
}
