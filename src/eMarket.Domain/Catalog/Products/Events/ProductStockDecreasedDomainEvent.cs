using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Catalog.Products.Events;

public sealed record ProductStockDecreasedDomainEvent(
    ProductId ProductId,
    int Quantity)
    : DomainEvent;
