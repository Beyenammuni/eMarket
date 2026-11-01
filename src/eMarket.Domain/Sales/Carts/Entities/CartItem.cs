
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
        CartId cartId,
        ProductId productId,
        Guid? variantId,
        string? variantName,
        Money unitPrice,
        Quantity quantity)
    {
        Id = Guid.NewGuid();
        CartId = cartId;
        ProductId = productId;
        VariantId = variantId;
        VariantName = variantName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
    public Guid Id { get; private set; }
    public CartId CartId { get; private set; } = default!;

    public ProductId ProductId { get; private set; } = default!;

    public Guid? VariantId { get; private set; }

    public string? VariantName { get; private set; }

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
