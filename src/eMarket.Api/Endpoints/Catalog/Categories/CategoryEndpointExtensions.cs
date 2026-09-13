namespace eMarket.Api.Endpoints.Catalog.Categories;

public static class CategoryEndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapCategoryEndpoints();

        return app;
    }
}
