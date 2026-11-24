using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Branches.Commands.ActivateBranch;

public sealed record ActivateBranchCommand(
    Guid BusinessId,
    Guid BranchId
) : IRequest<Result>;
