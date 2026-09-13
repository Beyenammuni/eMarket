using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Queries.GetMyOrders;

public sealed record GetMyOrdersQuery
    : IRequest<Result<List<GetMyOrdersResponse>>>;
