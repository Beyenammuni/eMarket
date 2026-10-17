using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Businesses.Commands.SuspendBusiness;

public sealed record SuspendBusinessCommand(Guid BusinessId) : IRequest<Result>;
