namespace eMarket.Application.Catalog.Products.Queries.GetProducts;

public sealed record GetProductsResponse(
    IReadOnlyList<ProductDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
