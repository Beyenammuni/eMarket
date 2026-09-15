using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Catalog.Products.Events;

public sealed record ProductStockIncreasedDomainEvent(
    ProductId ProductId,
    int Quantity)
    : DomainEvent;
