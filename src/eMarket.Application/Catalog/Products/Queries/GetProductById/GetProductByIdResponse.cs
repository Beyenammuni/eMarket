
using eMarket.Application.Catalog.Products.Queries.GetProducts;

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
    DateTime CreatedAt,
    IReadOnlyCollection<ProductVariantResponse> Variants,
    IReadOnlyCollection<ProductOptionGroupResponse> OptionGroups,
    PromotionPriceDto? Promotion = null);

public sealed record ProductVariantResponse(
    Guid Id,
    string Name,
    string? Sku,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    PromotionPriceDto? Promotion = null);

public sealed record ProductOptionGroupResponse(
    Guid Id,
    string Name,
    bool IsRequired,
    int MinSelections,
    int MaxSelections,
    int DisplayOrder,
    IReadOnlyCollection<ProductOptionResponse> Options);

public sealed record ProductOptionResponse(
    Guid Id,
    string Name,
    decimal AdditionalPrice,
    bool IsActive,
    int DisplayOrder);
