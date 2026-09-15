using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Carts.Queries.GetCart;

public sealed record GetCartQuery
    : IRequest<Result<GetCartResponse>>;
