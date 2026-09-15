namespace eMarket.Application.Businesses.Commands.RemoveMember;

public sealed record RemoveMemberResponse(
    Guid BusinessId,
    Guid UserId);
