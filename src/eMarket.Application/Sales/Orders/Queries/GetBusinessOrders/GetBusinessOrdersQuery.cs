using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Queries.GetBusinessOrders;

public sealed record GetBusinessOrdersQuery(
    Guid BusinessId)
    : IRequest<Result<List<GetBusinessOrdersResponse>>>;
