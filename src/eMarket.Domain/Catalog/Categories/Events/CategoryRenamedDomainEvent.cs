using eMarket.Domain.Catalog.Categories;
using eMarket.Domain.Catalog.Categories.ValueObjects;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Catalog.Categories.Events;

public sealed record CategoryRenamedDomainEvent(
    CategoryId CategoryId,
    CategoryName Name)
    : DomainEvent;
