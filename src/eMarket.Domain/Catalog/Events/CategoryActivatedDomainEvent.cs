using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Catalog.Events;

public sealed record CategoryActivatedDomainEvent(
    CategoryId CategoryId)
    : DomainEvent;
