using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Carts.Commands.AddToCart;

public sealed record AddToCartCommand(
    Guid ProductId,
    int Quantity)
    : IRequest<Result>;
