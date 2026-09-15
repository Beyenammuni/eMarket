using eMarket.Domain.Sales.Orders;

namespace eMarket.Domain.Sales.Orders.Rules;

public sealed class OrderCanBeCancelledRule(
    OrderStatus status)
{
    public bool IsBroken()
        => status is
            OrderStatus.Shipped or
            OrderStatus.Delivered or
            OrderStatus.Cancelled;
}
