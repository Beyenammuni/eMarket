
using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;

namespace eMarket.Domain.Sales.Orders.Entities;

public sealed class OrderItem
{
    private OrderItem()
    {
    }

    internal OrderItem(
        ProductId productId,
        string productName,
        Money originalUnitPrice,
        Money discountAmount,
        Money unitPrice,
        int quantity,
        Guid? variantId = null,
        string? variantName = null,
        Guid? promotionId = null,
        string? promotionName = null)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;

        OriginalUnitPrice = originalUnitPrice;
        DiscountAmount = discountAmount;
        UnitPrice = unitPrice;

        Quantity = quantity;
        VariantId = variantId;
        VariantName = variantName;

        PromotionId = promotionId;
        PromotionName = promotionName;
    }

    public Guid Id { get; private set; }

    public ProductId ProductId { get; private set; } = default!;

    public string ProductName { get; private set; } = default!;

    public Money OriginalUnitPrice { get; private set; } = default!;

    public Money DiscountAmount { get; private set; } = default!;

    public Money UnitPrice { get; private set; } = default!;

    public int Quantity { get; private set; }

    public Guid? VariantId { get; private set; }

    public string? VariantName { get; private set; }

    public Guid? PromotionId { get; private set; }

    public string? PromotionName { get; private set; }

    public decimal TotalDiscount =>
        DiscountAmount.Amount * Quantity;

    public decimal TotalPrice =>
        UnitPrice.Amount * Quantity;
}
