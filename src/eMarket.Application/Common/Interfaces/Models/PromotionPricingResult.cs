using eMarket.Domain.Catalog.Products.ValueObjects;

namespace eMarket.Application.Common.Models;

public sealed record PromotionPricingResult(
    Money OriginalUnitPrice,
    Money DiscountAmount,
    Money FinalUnitPrice,
    Guid? PromotionId,
    string? PromotionName);
