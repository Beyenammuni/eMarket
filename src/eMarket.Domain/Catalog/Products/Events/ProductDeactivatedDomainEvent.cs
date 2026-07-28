using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Catalog.Products.Events;

public sealed record ProductDeactivatedDomainEvent(
    ProductId ProductId)
    : DomainEvent;
