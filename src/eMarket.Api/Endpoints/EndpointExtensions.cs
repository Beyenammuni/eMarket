using eMarket.Api.Endpoints.Categories;

namespace eMarket.Api.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapCategoryEndpoints();

        return app;
    }
}
