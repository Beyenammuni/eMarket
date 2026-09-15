using eMarket.Domain.Identity;

namespace eMarket.Domain.Identity.Entities;

public sealed class UserRoleAssignment
{
    private UserRoleAssignment()
    {
    }

    public UserRoleAssignment(
        UserId userId,
        UserRole role)
    {
        UserId = userId;
        RoleId = role.Id;
    }

    public UserId UserId { get; private set; } = default!;

    public int RoleId { get; private set; }
}
