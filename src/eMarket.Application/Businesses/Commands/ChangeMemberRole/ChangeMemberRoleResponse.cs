namespace eMarket.Application.Businesses.Commands.ChangeMemberRole;

public sealed record ChangeMemberRoleResponse
    (
    Guid BusinessId,
    Guid UserId,
    int Role);
