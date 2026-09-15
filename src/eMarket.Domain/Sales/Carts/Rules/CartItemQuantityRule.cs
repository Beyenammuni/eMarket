namespace eMarket.Domain.Sales.Carts.Rules;

public sealed class CartItemQuantityRule(
    int quantity)
{
    public bool IsBroken()
        => quantity <= 0;
}
