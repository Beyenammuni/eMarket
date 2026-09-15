using eMarket.Domain.Common;

namespace eMarket.Application.Catalog.Products.Commands.UpdateProduct;

public sealed record UpdateProductResponse(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    Currency Currency,
    string Sku,
    Guid CategoryId,
    string? ImageUrl);
