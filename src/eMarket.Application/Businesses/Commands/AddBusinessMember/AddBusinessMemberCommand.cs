using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.AddBusinessMember;

public sealed record AddBusinessMemberCommand(
    Guid BusinessId,
    Guid UserId,
    int Role)
    : IRequest<Result<AddBusinessMemberResponse>>;
