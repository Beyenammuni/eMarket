using eMarket.Domain.Identity.ValueObjects;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Identity.Events;

public sealed record UserSuspendedDomainEvent(
    UserId UserId)
    : DomainEvent;
