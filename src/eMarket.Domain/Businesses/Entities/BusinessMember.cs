using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Businesses.Entities;

public sealed class BusinessMember
{
    private BusinessMember()
    {
    }

    public BusinessMember(
        BusinessId businessId,
        UserId userId,
        BusinessRole role)
    {
        Id = BusinessMemberId.New();
        BusinessId = businessId;
        UserId = userId;
        Role = role;
        IsActive = true;
        JoinedAt = DateTime.UtcNow;
    }

    public BusinessMemberId Id { get; private set; } = default!;

    public BusinessId BusinessId { get; private set; } = default!;

    public UserId UserId { get; private set; } = default!;

    public BusinessRole Role { get; private set; } = default!;

    public bool IsActive { get; private set; }

    public DateTime JoinedAt { get; private set; }

    public Result ChangeRole(BusinessRole newRole)
    {
        if (Role == newRole)
        {
            return Result.Success();
        }

        Role = newRole;

        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
        {
            return Result.Success();
        }

        IsActive = true;

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Success();
        }

        IsActive = false;

        return Result.Success();
    }
}
