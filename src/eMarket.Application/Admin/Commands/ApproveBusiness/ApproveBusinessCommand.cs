using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Admin.Commands.ApproveBusiness;

public sealed record ApproveBusinessCommand(
    Guid BusinessId) : IRequest<Result>;
