namespace eMarket.Api.Endpoints.Categories;

public static class CategoryEndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapCategoryEndpoints();

        return app;
    }
}
