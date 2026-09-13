using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Carts.Commands.ClearCart;

public sealed record ClearCartCommand
    : IRequest<Result>;
