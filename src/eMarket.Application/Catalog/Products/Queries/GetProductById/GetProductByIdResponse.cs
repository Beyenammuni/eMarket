using eMarket.Domain.Common;

namespace eMarket.Application.Catalog.Products.Queries.GetProductById;

public sealed record GetProductByIdResponse(
    Guid Id,
    Guid BusinessId,
    Guid CategoryId,
    string Name,
    string Description,
    decimal Price,
    string Sku,
    string? ImageUrl,
    int StockQuantity,
    string Status,
    DateTime CreatedAt);
