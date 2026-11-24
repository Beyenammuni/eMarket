using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Branches.Commands.DeactivateBranch;

public sealed record DeactivateBranchCommand(
    Guid BusinessId,
    Guid BranchId
) : IRequest<Result>;
