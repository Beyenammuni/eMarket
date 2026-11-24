
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Promotions.Enums;
using eMarket.Domain.Promotions.ValueObjects;

namespace eMarket.Domain.Promotions.Entities;

public sealed class PromotionItem
{
    private PromotionItem()
    {
    }

    private PromotionItem(
        Guid id,
        PromotionId promotionId,
        ProductId productId,
        DiscountType discountType,
        decimal discountValue)
    {
        Id = id;
        PromotionId = promotionId;
        ProductId = productId;
        DiscountType = discountType;
        DiscountValue = discountValue;
        CreatedAt = DateTime.UtcNow;
    }

    public Guid Id { get; private set; }

    public PromotionId PromotionId { get; private set; } = default!;

    public ProductId ProductId { get; private set; } = default!;

    public DiscountType DiscountType { get; private set; }

    public decimal DiscountValue { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static PromotionItem Create(
        PromotionId promotionId,
        ProductId productId,
        DiscountType discountType,
        decimal discountValue)
    {
        ArgumentNullException.ThrowIfNull(promotionId);
        ArgumentNullException.ThrowIfNull(productId);

        if (!Enum.IsDefined(discountType))
            throw new ArgumentOutOfRangeException(
                nameof(discountType));

        if (discountValue <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(discountValue),
                "Discount value must be greater than zero.");

        if (discountType == DiscountType.Percentage &&
            discountValue >= 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(discountValue),
                "Percentage discount must be less than 100.");
        }

        return new PromotionItem(
            Guid.NewGuid(),
            promotionId,
            productId,
            discountType,
            discountValue);
    }

    public Money CalculateDiscountedPrice(Money originalPrice)
    {
        ArgumentNullException.ThrowIfNull(originalPrice);

        decimal discountedAmount;

        if (DiscountType == DiscountType.Percentage)
        {
            discountedAmount =
                originalPrice.Amount *
                (1 - DiscountValue / 100m);
        }
        else
        {
            if (DiscountValue >= originalPrice.Amount)
            {
                throw new InvalidOperationException(
                    "Discount must be less than the original price.");
            }

            discountedAmount =
                originalPrice.Amount - DiscountValue;
        }

        discountedAmount = decimal.Round(
            discountedAmount,
            2,
            MidpointRounding.AwayFromZero);

        return Money.Create(
            discountedAmount,
            originalPrice.Currency);
    }
    public void UpdateDiscount(
    DiscountType discountType,
    decimal discountValue)
    {
        if (!Enum.IsDefined(discountType))
            throw new ArgumentOutOfRangeException(nameof(discountType));

        if (discountValue <= 0)
            throw new ArgumentOutOfRangeException(nameof(discountValue));

        if (discountType == DiscountType.Percentage &&
            discountValue >= 100)
            throw new ArgumentOutOfRangeException(nameof(discountValue));

        DiscountType = discountType;
        DiscountValue = discountValue;
    }
}
