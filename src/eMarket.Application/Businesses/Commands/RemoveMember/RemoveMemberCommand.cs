using MediatR;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Businesses.Commands.RemoveMember;

public sealed record RemoveMemberCommand(
    Guid BusinessId,
    Guid UserId)
    : IRequest<Result<RemoveMemberResponse>>;
