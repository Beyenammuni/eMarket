using eMarket.Application.Common.Interfaces;
using eMarket.Application.Common.IRepositories;
using eMarket.Application.Common.Models;
using eMarket.Domain.Businesses;
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;

namespace eMarket.Application.Services;

public sealed class PromotionPricingService
    : IPromotionPricingService
{
    private readonly IPromotionRepository _promotionRepository;

    public PromotionPricingService(
        IPromotionRepository promotionRepository)
    {
        _promotionRepository = promotionRepository;
    }

    public async Task<PromotionPricingResult> CalculateAsync(
        BusinessId businessId,
        ProductId productId,
        Money originalUnitPrice,
        CancellationToken cancellationToken = default)
    {
        var promotions =
            await _promotionRepository.GetActiveForProductAsync(
                businessId,
                productId,
                DateTime.UtcNow,
                cancellationToken);

        // No active promotion.
        if (promotions.Count == 0)
        {
            return CreateWithoutPromotion(originalUnitPrice);
        }

        // Never apply multiple promotions to the same product.
        if (promotions.Count > 1)
        {
            throw new InvalidOperationException(
                $"Multiple active promotions found for product {productId.Value}.");
        }

        var promotion = promotions[0];

        var promotionItem = promotion.Items
            .Single(x => x.ProductId == productId);

        Money finalUnitPrice;

        try
        {
            finalUnitPrice =
                promotionItem.CalculateDiscountedPrice(
                    originalUnitPrice);
        }
        catch (InvalidOperationException)
        {
            // A fixed discount cannot exceed the selected variant price.
            // In that case, do not apply this promotion to this variant.
            return CreateWithoutPromotion(originalUnitPrice);
        }

        var discountAmount =
            originalUnitPrice.Subtract(finalUnitPrice);

        return new PromotionPricingResult(
            originalUnitPrice,
            discountAmount,
            finalUnitPrice,
            promotion.Id.Value,
            promotion.Name);
    }

    private static PromotionPricingResult CreateWithoutPromotion(
        Money originalUnitPrice)
    {
        var zeroDiscount =
            Money.Zero(originalUnitPrice.Currency);

        return new PromotionPricingResult(
            originalUnitPrice,
            zeroDiscount,
            originalUnitPrice,
            null,
            null);
    }
}
