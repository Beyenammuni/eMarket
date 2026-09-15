using eMarket.Domain.Sales.Orders;

namespace eMarket.Domain.Sales.Orders.Rules;

public sealed class OrderCanBePaidRule(
    OrderStatus status)
{
    public bool IsBroken()
        => status != OrderStatus.PendingPayment;
}
