using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.Domain.Sales.Carts.ValueObjects;

namespace eMarket.Domain.Sales.Carts.Entities;

public sealed class CartItem
{
    private CartItem()
    {
    }

    internal CartItem(
        ProductId productId,
        Money unitPrice,
        Quantity quantity)
    {
        ProductId = productId;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public ProductId ProductId { get; private set; } = default!;

    public Money UnitPrice { get; private set; } = default!;

    public Quantity Quantity { get; private set; } = default!;
    public decimal TotalPrice =>
        UnitPrice.Amount * Quantity.Value;

    internal void IncreaseQuantity(int amount)
    {
        Quantity = Quantity.Increase(amount);
    }

    internal void ChangeQuantity(Quantity quantity)
    {
        Quantity = quantity;
    }

    internal void UpdatePrice(Money price)
    {
        UnitPrice = price;
    }
}
