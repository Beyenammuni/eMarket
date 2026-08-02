using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Businesses.Events;

public sealed record BusinessSuspendedDomainEvent(
    BusinessId BusinessId)
    : DomainEvent;
