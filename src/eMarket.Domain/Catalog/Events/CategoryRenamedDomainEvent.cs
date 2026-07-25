using eMarket.Domain.Catalog.ValueObjects;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Catalog.Events;

public sealed record CategoryRenamedDomainEvent(
    CategoryId CategoryId,
    CategoryName Name)
    : DomainEvent;
