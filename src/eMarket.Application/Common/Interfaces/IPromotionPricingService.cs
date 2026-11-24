using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Application.Common.Models;

namespace eMarket.Application.Common.Interfaces;

public interface IPromotionPricingService
{
    Task<PromotionPricingResult> CalculateAsync(
        BusinessId businessId,
        ProductId productId,
        Money originalUnitPrice,
        CancellationToken cancellationToken = default);
}
