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
    Money unitPrice,
    int quantity,
    Guid? variantId = null,
    string? variantName = null)
    {
        Id = Guid.NewGuid();
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        VariantId = variantId;
        VariantName = variantName;
    }
    public Guid Id { get; private set; }

    public ProductId ProductId { get; private set; } = default!;

    public string ProductName { get; private set; } = default!;

    public Money UnitPrice { get; private set; } = default!;

    public int Quantity { get; private set; }
    public Guid? VariantId { get; private set; }

    public string? VariantName { get; private set; }

    public decimal TotalPrice =>
        UnitPrice.Amount * Quantity;
}
