
namespace eMarket.Domain.Businesses.Entities;

public sealed class BusinessMemberPermission
{
    private BusinessMemberPermission()
    {
    }

    private BusinessMemberPermission(
        BusinessMemberId businessMemberId,
        string permission,
        bool isGranted)
    {
        BusinessMemberId = businessMemberId;
        Permission = permission;
        IsGranted = isGranted;
    }

    public BusinessMemberId BusinessMemberId { get; private set; } = default!;

    public string Permission { get; private set; } = string.Empty;

    public bool IsGranted { get; private set; }

    public static BusinessMemberPermission Create(
        BusinessMemberId businessMemberId,
        string permission,
        bool isGranted)
    {
        if (string.IsNullOrWhiteSpace(permission))
            throw new ArgumentException(
                "Permission cannot be empty.",
                nameof(permission));

        return new BusinessMemberPermission(
            businessMemberId,
            permission.Trim(),
            isGranted);
    }

    public void SetGranted(bool isGranted)
    {
        IsGranted = isGranted;
    }
}
