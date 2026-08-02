using eMarket.Domain.Businesses.ValueObjects;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Businesses.Events;

public sealed record BusinessRenamedDomainEvent(
    BusinessId BusinessId,
    BusinessName NewName)
    : DomainEvent;
