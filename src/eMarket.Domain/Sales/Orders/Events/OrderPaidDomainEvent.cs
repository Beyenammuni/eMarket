using eMarket.Domain.Sales.Orders;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Sales.Orders.Events;

public sealed record OrderPaidDomainEvent(
    OrderId OrderId)
    : DomainEvent;
