using eMarket.Domain.Sales.Orders.Entities;

namespace eMarket.Domain.Sales.Orders.Rules;

public sealed class OrderMustHaveItemsRule(
    IReadOnlyCollection<OrderItem> items)
{
    public bool IsBroken()
        => items.Count == 0;
}
