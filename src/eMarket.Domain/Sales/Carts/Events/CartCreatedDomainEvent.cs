using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Sales.Carts.Events;

public sealed record CartCreatedDomainEvent(
    CartId CartId)
    : DomainEvent;
