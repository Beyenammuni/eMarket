using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Commands.Cancel;

public sealed record CancelOrderCommand(
    Guid Id) : IRequest<Result>;
