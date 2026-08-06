namespace eMarket.Application.Businesses.Commands.ChangeMemberRole;

public sealed record ChangeMemberRoleResponse
    (
    Guid BusinessId,
    Guid UserId,
    string Role);
