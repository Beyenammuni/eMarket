using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Sales.Carts.Events;

public sealed record CartClearedDomainEvent(
    CartId CartId)
    : DomainEvent;
