using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Commands.Ship;

public sealed record ShipCommand(
    Guid Id) : IRequest<Result>;
