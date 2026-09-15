using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Businesses.Events;

public sealed record BusinessClosedDomainEvent(
    BusinessId BusinessId)
    : DomainEvent;
