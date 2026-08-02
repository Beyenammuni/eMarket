using eMarket.Domain.Businesses.Entities;
using eMarket.Domain.Businesses.Events;
using eMarket.Domain.Businesses.Rules;
using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Businesses;

public sealed class Business : AggregateRoot<BusinessId>
{
    private readonly List<BusinessMember> _members = new();

    private Business()
    {
    }

    private Business(
        BusinessId id,
        BusinessName name,
        BusinessType type)
    {
        Id = id;
        Name = name;
        Type = type;
        Status = BusinessStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public BusinessName Name { get; private set; } = default!;

    public BusinessType Type { get; private set; } = default!;

    public BusinessStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public IReadOnlyCollection<BusinessMember> Members =>
        _members.AsReadOnly();
    //Create aa new Business
    public static Result<Business> Create(
    BusinessName name,
    BusinessType type,
    UserId ownerId)
    {
        var business = new Business(
            BusinessId.New(),
            name,
            type);

        business.AddInitialOwner(ownerId);

        business.AddDomainEvent(
            new BusinessCreatedDomainEvent(
                business.Id));

        return Result<Business>.Success(business);
    }

    private void AddInitialOwner(UserId ownerId)
    {
        _members.Add(
            new BusinessMember(
                BusinessMemberId.New(),
                ownerId,
                BusinessRole.Owner));
    }
    public Result Activate()
    {
        if (Status == BusinessStatus.Active)
        {
            return Result.Failure(BusinessErrors.AlreadyActive);
        }

        Status = BusinessStatus.Active;

        AddDomainEvent(
            new BusinessActivatedDomainEvent(Id));

        return Result.Success();
    }
    public Result Suspend()
    {
        if (Status == BusinessStatus.Suspended)
        {
            return Result.Failure(BusinessErrors.AlreadySuspended);
        }

        Status = BusinessStatus.Suspended;

        AddDomainEvent(
            new BusinessSuspendedDomainEvent(Id));

        return Result.Success();
    }
    public Result Close()
    {
        if (Status == BusinessStatus.Closed)
        {
            return Result.Failure(BusinessErrors.AlreadyClosed);
        }

        Status = BusinessStatus.Closed;

        AddDomainEvent(
            new BusinessClosedDomainEvent(Id));

        return Result.Success();
    }
    public Result AddMember(
    UserId userId,
    BusinessRole role)
    {
        if (new MemberCannotBeAddedTwiceRule(_members, userId).IsBroken())
        {
            return Result.Failure(BusinessErrors.MemberAlreadyExists);
        }

        var member = new BusinessMember(
            BusinessMemberId.New(),
            userId,
            role);

        _members.Add(member);

        AddDomainEvent(
            new MemberJoinedBusinessDomainEvent(
                Id,
                userId));

        return Result.Success();
    }
    public Result RemoveMember(UserId userId)
    {
        var member = GetActiveMember(userId);

        if (member is null)
        {
            return Result.Failure(BusinessErrors.MemberNotFound);
        }

        if (new CannotRemoveLastOwnerRule(_members, userId).IsBroken())
        {
            return Result.Failure(BusinessErrors.CannotRemoveLastOwner);
        }

        member.Deactivate();

        AddDomainEvent(
            new MemberRemovedBusinessDomainEvent(
                Id,
                userId));

        return Result.Success();
    }
    public Result ChangeMemberRole(
    UserId userId,
    BusinessRole newRole)
    {
        var member = GetActiveMember(userId);

        if (member is null)
        {
            return Result.Failure(BusinessErrors.MemberNotFound);
        }

        if (member.Role == newRole)
        {
            return Result.Failure(BusinessErrors.MemberAlreadyHasRole);
        }

        if (member.Role == BusinessRole.Owner)
        {
            return Result.Failure(BusinessErrors.CannotChangeOwnerRole);
        }

        member.ChangeRole(newRole);

        AddDomainEvent(
            new MemberRoleChangedDomainEvent(
                Id,
                userId,
                newRole));

        return Result.Success();
    }

    public Result TransferOwnership(
    UserId currentOwnerId,
    UserId newOwnerId)
    {
        if (currentOwnerId == newOwnerId)
        {
            return Result.Failure(BusinessErrors.TransferToSameOwner);
        }

        var currentOwner = _members.FirstOrDefault(x =>
            x.UserId == currentOwnerId &&
            x.IsActive);

        if (currentOwner is null)
        {
            return Result.Failure(BusinessErrors.CurrentOwnerNotFound);
        }

        if (currentOwner.Role != BusinessRole.Owner)
        {
            return Result.Failure(BusinessErrors.NotOwner);
        }

        var newOwner = _members.FirstOrDefault(x =>
            x.UserId == newOwnerId &&
            x.IsActive);

        if (newOwner is null)
        {
            return Result.Failure(BusinessErrors.NewOwnerNotFound);
        }

        currentOwner.ChangeRole(BusinessRole.Manager);

        newOwner.ChangeRole(BusinessRole.Owner);

        AddDomainEvent(
            new OwnershipTransferredDomainEvent(
                Id,
                currentOwnerId,
                newOwnerId));

        return Result.Success();
    }
    private BusinessMember? GetActiveMember(UserId userId)
    {
        return _members.FirstOrDefault(x =>
            x.UserId == userId &&
            x.IsActive);
    }

    private bool HasActiveMember(UserId userId)
    {
        return _members.Any(x =>
            x.UserId == userId &&
            x.IsActive);
    }

    private BusinessMember? GetOwner()
    {
        return _members.FirstOrDefault(x =>
            x.Role == BusinessRole.Owner &&
            x.IsActive);
    }

}
