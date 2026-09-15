using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Businesses.Events;

public sealed record BusinessActivatedDomainEvent(
    BusinessId BusinessId)
    : DomainEvent;
