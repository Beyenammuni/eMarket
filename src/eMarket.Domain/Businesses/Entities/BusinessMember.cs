using eMarket.Domain.Businesses.ValueObjects;
using eMarket.Domain.Identity;
using eMarket.SharedKernel.Common;
using eMarket.SharedKernel.Results;

namespace eMarket.Domain.Businesses.Entities;

public sealed class BusinessMember : Entity<BusinessMemberId>
{
    private BusinessMember()
    {
    }

    internal BusinessMember(
        BusinessMemberId id,
        UserId userId,
        BusinessRole role)
    {
        Id = id;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public UserId UserId { get; private set; } = default!;

    public BusinessRole Role { get; private set; } = default!;

    public bool IsActive { get; private set; }

    public DateTime JoinedAt { get; private set; }

    internal void ChangeRole(BusinessRole role)
    {
        Role = role;
    }

    internal Result Deactivate()
    {
        if (!IsActive)
        {
            return Result.Success();
        }

        IsActive = false;

        return Result.Success();
    }
    internal void Activate()
    {
        IsActive = true;
    }

}
