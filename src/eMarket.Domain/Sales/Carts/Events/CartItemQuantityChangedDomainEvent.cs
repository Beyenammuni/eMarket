using eMarket.Domain.Catalog.Products;
using eMarket.Domain.Sales.Carts.ValueObjects;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Sales.Carts.Events;

public sealed record CartItemQuantityChangedDomainEvent(
    CartId CartId,
    ProductId ProductId,
    Quantity Quantity)
    : DomainEvent;
