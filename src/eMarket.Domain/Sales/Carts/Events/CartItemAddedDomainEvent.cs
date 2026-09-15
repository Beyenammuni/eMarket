using eMarket.Domain.Catalog.Products;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Sales.Carts.Events;

public sealed record CartItemAddedDomainEvent(
    CartId CartId,
    ProductId ProductId)
    : DomainEvent;
