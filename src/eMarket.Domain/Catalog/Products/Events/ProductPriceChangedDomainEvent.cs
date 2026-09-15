using eMarket.Domain.Catalog.Products.ValueObjects;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Catalog.Products.Events;

public sealed record ProductPriceChangedDomainEvent(
    ProductId ProductId,
    Money Price)
    : DomainEvent;
