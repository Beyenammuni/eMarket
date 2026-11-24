using eMarket.Domain.Identity;
using eMarket.Domain.Sales.Orders;
using eMarket.Domain.Sales.Orders.ValueObjects;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Sales.Orders.Events;

public sealed record OrderStatusChangedDomainEvent(
    OrderId OrderId,
    UserId UserId,
    OrderStatus Status)
    : DomainEvent;
