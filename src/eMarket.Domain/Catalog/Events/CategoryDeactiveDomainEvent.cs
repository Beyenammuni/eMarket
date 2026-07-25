using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Catalog.Events;

public sealed record CategoryDeactivatedDomainEvent(
    CategoryId CategoryId)
    : DomainEvent;
