
using eMarket.Domain.Common;
using eMarket.Domain.Promotions.Enums;

namespace eMarket.Application.Catalog.Products.Queries.GetProducts;

public sealed record PromotionPriceDto(
    decimal OriginalPrice,
    decimal DiscountedPrice,
    DiscountType DiscountType,
    decimal DiscountValue,
    string PromotionName);

public sealed record ProductVariantDto(
    Guid Id,
    string Name,
    decimal Price,
    int StockQuantity,
    bool IsActive,
    PromotionPriceDto? Promotion = null);

public sealed record ProductDto(
    Guid Id,
    Guid BusinessId,
    Guid CategoryId,
    string Name,
    decimal Price,
    string? ImageUrl,
    int StockQuantity,
    string Status,
    IReadOnlyList<ProductVariantDto> Variants,
    PromotionPriceDto? Promotion = null);
