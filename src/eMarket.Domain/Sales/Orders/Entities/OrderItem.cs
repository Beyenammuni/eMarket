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
        int quantity)
    {
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public ProductId ProductId { get; private set; } = default!;

    public string ProductName { get; private set; } = default!;

    public Money UnitPrice { get; private set; } = default!;

    public int Quantity { get; private set; }

    public decimal TotalPrice =>
        UnitPrice.Amount * Quantity;
}
