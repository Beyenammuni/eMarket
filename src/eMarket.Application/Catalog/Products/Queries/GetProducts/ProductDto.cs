using eMarket.Domain.Common;

namespace eMarket.Application.Catalog.Products.Queries.GetProducts;

public sealed record ProductDto(
    Guid Id,
    Guid BusinessId,
    Guid CategoryId,
    string Name,
    decimal Price,
    string? ImageUrl,
    int StockQuantity,
    string Status);
