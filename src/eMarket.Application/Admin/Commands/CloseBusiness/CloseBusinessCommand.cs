using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Admin.Commands.CloseBusiness;

public sealed record CloseBusinessCommand(
    Guid BusinessId)
    : IRequest<Result>;
