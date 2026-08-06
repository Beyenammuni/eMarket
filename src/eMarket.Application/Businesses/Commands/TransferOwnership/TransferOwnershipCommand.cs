using MediatR;
using eMarket.SharedKernel.Results;

namespace eMarket.Application.Businesses.Commands.TransferOwnership;

public sealed record TransferOwnershipCommand(
    Guid BusinessId,
    Guid NewOwnerId)
    : IRequest<Result<TransferOwnershipResponse>>;
