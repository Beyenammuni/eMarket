using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Businesses.Events;

public sealed record BusinessCreatedDomainEvent(
    BusinessId BusinessId)
    : DomainEvent;
