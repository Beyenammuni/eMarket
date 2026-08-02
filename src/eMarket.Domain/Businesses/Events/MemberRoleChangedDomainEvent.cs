using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Businesses.Events;

public sealed record MemberRoleChangedDomainEvent(
    BusinessId BusinessId,
    UserId UserId,
    BusinessRole NewRole)
    : DomainEvent;
