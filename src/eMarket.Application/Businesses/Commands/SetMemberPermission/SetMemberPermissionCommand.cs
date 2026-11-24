
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.SetMemberPermission;

public sealed record SetMemberPermissionCommand(
    Guid BusinessId,
    Guid UserId,
    string Permission,
    bool IsGranted)
    : IRequest<Result>;
