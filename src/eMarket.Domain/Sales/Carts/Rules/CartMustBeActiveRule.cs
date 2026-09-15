using eMarket.Domain.Sales.Carts;

namespace eMarket.Domain.Sales.Carts.Rules;

public sealed class CartMustBeActiveRule(
    CartStatus status)
{
    public bool IsBroken()
        => status != CartStatus.Active;
}
