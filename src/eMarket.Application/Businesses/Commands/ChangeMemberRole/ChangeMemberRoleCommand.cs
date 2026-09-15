using MediatR;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Businesses.Commands.ChangeMemberRole;

public sealed record ChangeMemberRoleCommand(
    Guid BusinessId,
    Guid UserId,
    int Role)
    : IRequest<Result<ChangeMemberRoleResponse>>;
