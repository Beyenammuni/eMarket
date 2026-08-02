using eMarket.Domain.Identity;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Businesses.Events;

public sealed record MemberRemovedBusinessDomainEvent(
    BusinessId BusinessId,
    UserId UserId)
    : DomainEvent;
