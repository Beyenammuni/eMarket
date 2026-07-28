using eMarket.Domain.Catalog.Categories;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Catalog.Categories.Events;

public sealed record CategoryDeactivatedDomainEvent(
    CategoryId CategoryId)
    : DomainEvent;
