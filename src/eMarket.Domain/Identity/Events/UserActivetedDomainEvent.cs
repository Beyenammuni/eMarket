using eMarket.Domain.Identity.ValueObjects;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Identity.Events;

public sealed record UserActivatedDomainEvent(
    UserId UserId)
    : DomainEvent;
