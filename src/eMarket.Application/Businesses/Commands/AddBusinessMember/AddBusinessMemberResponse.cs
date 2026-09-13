namespace eMarket.Application.Businesses.Commands.AddBusinessMember;

public sealed record AddBusinessMemberResponse(
    Guid BusinessId,
    Guid UserId,
    int Role);
