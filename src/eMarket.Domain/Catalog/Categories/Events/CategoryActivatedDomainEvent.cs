using eMarket.Domain.Catalog.Categories;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Catalog.Categories.Events;

public sealed record CategoryActivatedDomainEvent(
    CategoryId CategoryId)
    : DomainEvent;
