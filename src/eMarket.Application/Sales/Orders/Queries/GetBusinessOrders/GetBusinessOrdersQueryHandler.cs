using eMarket.Application.Common.IRepositories;
using eMarket.SharedKernel.Results;
using MediatR;

namespace eMarket.Application.Sales.Orders.Queries.GetBusinessOrders;

internal sealed class GetBusinessOrdersHandler
    : IRequestHandler<
        GetBusinessOrdersQuery,
        Result<List<GetBusinessOrdersResponse>>>
{
    private readonly IOrderRepository _orderRepository;

    public GetBusinessOrdersHandler(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<Result<List<GetBusinessOrdersResponse>>> Handle(
        GetBusinessOrdersQuery request,
        CancellationToken cancellationToken)
    {
        var orders =
            await _orderRepository.GetByBusinessIdAsync(
                request.BusinessId,
                cancellationToken);

        var response = orders
            .Select(order =>
                new GetBusinessOrdersResponse(
                    order.Id.Value,
                    order.Status.ToString(),
                    order.CreatedAt,
                    order.TotalAmount,
                    order.Items.Sum(x => x.Quantity)))
            .ToList();

        return Result<List<GetBusinessOrdersResponse>>.Success(
            response);
    }
}
