using eMarket.Domain.Identity;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Businesses.Events;

public sealed record MemberJoinedBusinessDomainEvent(
    BusinessId BusinessId,
    UserId UserId)
    : DomainEvent;
