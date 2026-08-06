public sealed record GetBusinessMembersResponse(
    Guid UserId,
    string Role,
    bool IsActive,
    DateTime JoinedAt);
