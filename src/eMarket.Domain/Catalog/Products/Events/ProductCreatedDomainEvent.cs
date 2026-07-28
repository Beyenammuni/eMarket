using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Catalog.Products.Events;

public sealed record ProductCreatedDomainEvent(
    ProductId ProductId)
    : DomainEvent;
