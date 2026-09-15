using eMarket.Domain.Identity;
using eMarket.SharedKernel.DomainEvent;

namespace eMarket.Domain.Businesses.Events;

public sealed record OwnershipTransferredDomainEvent(
    BusinessId BusinessId,
    UserId PreviousOwner,
    UserId NewOwner)
    : DomainEvent;
