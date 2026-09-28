using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Queries.GetBusinessOrderById;

public sealed record GetBusinessOrderByIdQuery(
    Guid BusinessId,
    Guid OrderId)
    : IRequest<Result<GetBusinessOrderByIdResponse>>;
