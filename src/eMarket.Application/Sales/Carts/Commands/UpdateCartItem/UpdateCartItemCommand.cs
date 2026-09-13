using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Carts.Commands.UpdateCartItem;

public sealed record UpdateCartItemCommand(
    Guid ProductId,
    int Quantity)
    : IRequest<Result>;
