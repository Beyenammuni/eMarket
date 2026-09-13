using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(
    Guid Id)
    : IRequest<Result<GetOrderByIdResponse>>;
