
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.ActivateBusiness;

public sealed record ActivateBusinessCommand(Guid BusinessId)
    : IRequest<Result>;
