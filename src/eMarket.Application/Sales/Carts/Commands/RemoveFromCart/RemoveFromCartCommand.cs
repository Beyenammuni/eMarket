using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Carts.Commands.RemoveFromCart;

public sealed record RemoveFromCartCommand(
    Guid ProductId)
    : IRequest<Result>;
