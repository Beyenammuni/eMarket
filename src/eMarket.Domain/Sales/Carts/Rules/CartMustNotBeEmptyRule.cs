using eMarket.Domain.Sales.Carts.Entities;

namespace eMarket.Domain.Sales.Carts.Rules;

public sealed class CartMustNotBeEmptyRule(
    IReadOnlyCollection<CartItem> items)
{
    public bool IsBroken()
        => items.Count == 0;
}
