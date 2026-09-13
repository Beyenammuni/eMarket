using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Commands.MarkAsPaid;

public sealed record MarkAsPaidCommand(
    Guid Id)
    : IRequest<Result>;
