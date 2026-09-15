using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Commands.Deliver;

public sealed record DeliverCommand(
    Guid Id) : IRequest<Result>;
